using Microsoft.Extensions.Options;
using OpenCvSharp;
using System.Drawing;
using Vtable.Balcony.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Vtable.Balcony.CameraServices.Tapo
{
    public partial class TapoService : ICameraService
    {
        public string Name => "Tapo";
        private readonly string _rtspUrl;
        private TapoConfiguration Config { get; }

        public TapoService(ICredentialsFactory<TapoCredentials> credentialsFactory, IConfigurationFactory<TapoConfiguration> configurationFactory)
        {
            var creds = credentialsFactory.Get();

            if (creds is null)
                throw new InvalidOperationException("Camera credentials are missing or misconfigured");

            this._rtspUrl = $"rtsp://{creds.User}:{creds.Password}@{creds.Endpoint}/stream1";
            this.Config = configurationFactory.Get() ?? throw new InvalidOperationException("Could not retrieve Tapo configuration details");
        }

        public System.Drawing.Size RetrieveFrame(out byte[] imageData)
        {
            using var capture = new VideoCapture(this._rtspUrl);

            if (!capture.IsOpened())
                throw new InvalidOperationException("Unable to open the camera's stream");

            for (int i = 0; i < 5; i++)
                capture.Grab();

            using var frame = new Mat();

            if (!capture.Read(frame) || frame.Empty())
                throw new InvalidOperationException("Unable to retrieve a frame from the camera");

            using var corrected = this.RemoveDistortion(frame);
            using var cropped = this.CropImage(corrected);

            Cv2.ImEncode(".jpg", cropped, out imageData);

            return new System.Drawing.Size(cropped.Width, cropped.Height);
        }

        private Mat CropImage(Mat input)
        {
            if (this.Config.CropTop == 0 && this.Config.CropRight == 0 && this.Config.CropBottom == 0 && this.Config.CropLeft == 0)
                return input;

            return new Mat(input, new Rect(this.Config.CropLeft, this.Config.CropTop, input.Width - this.Config.CropRight, input.Height - this.Config.CropBottom));
        }


        private Mat RemoveDistortion(Mat input)
        {
            if (this.Config.FixDistortionStrength == 0)
                return input;

            int w = input.Width;
            int h = input.Height;

            float cx = w / 2f;
            float cy = h / 2f;

            Mat dst = new Mat(h, w, input.Type());

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = (x - cx) / cx;
                    float dy = (y - cy) / cy;

                    float r = MathF.Sqrt(dx * dx + dy * dy);

                    float factor = 1 + this.Config.FixDistortionStrength * (r * r);

                    float sx = cx + dx * factor * cx;
                    float sy = cy + dy * factor * cy;

                    if (sx >= 0 && sx < w && sy >= 0 && sy < h)
                        dst.Set(y, x, input.Get<Vec3b>((int)sy, (int)sx));
                }
            }

            return dst;
        }
    }
}
