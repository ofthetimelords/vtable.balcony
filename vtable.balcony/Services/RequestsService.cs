using Microsoft.Extensions.Options;
using Vtable.Balcony.Configuration;
using Vtable.Balcony.Factory;
using Vtable.Balcony.Helpers;
using Vtable.Balcony.Models;
using static System.Net.Mime.MediaTypeNames;

namespace Vtable.Balcony.Services
{
    public class RequestsService
    {
        private AppConfig AppConfig { get; }
        private ILogger<CameraBackgroundService> Logger { get; }
        private CameraBackgroundService CameraBackgroundService { get; }
        private FileService FileService { get; }

        public string? LastGeneratedImageTheme => this.CameraBackgroundService.LastGeneratedImageTheme;
        public DateTimeOffset? LastGeneratedImageTtimestamp => this.CameraBackgroundService.LastGeneratedImageTtimestamp;

        public RequestsService(
            IOptions<AppConfig> appConfig,
            CameraBackgroundService cameraBackgroundService,
            IPayloadFactory payloadFactory,
            RefreshTokenSource refreshTokenSource,
            FileService fileService,
            ILogger<CameraBackgroundService> logger)
        {
            this.AppConfig = appConfig.Value;
            this.CameraBackgroundService = cameraBackgroundService;
            FileService = fileService;
            Logger = logger;
        }

        /// <summary>
        /// Deletes a snapshot and its related generated image (if exists), along with any thumbnails
        /// </summary>
        /// <param name="name">The name of the file, without the extension, formatted as yyyyMMdd-hhMM</param>
        /// <returns></returns>
        public void DeleteImage(string name)
        {
            var sanitisedName = new string(name.Where(c => char.IsDigit(c) || c == '-').ToArray());

            this.FileService.DeleteFile(sanitisedName);
        }

        public Task<byte[]> GetCameraSnapshot(string name, bool thumb)
        {
            var sanitisedName = new string(name.Where(c => char.IsDigit(c) || c == '-').ToArray());
            var image = this.FileService.GetImagePath(thumb ? Constants.SnapThumbPath : Constants.SnapPath, sanitisedName, Constants.ImageExtension);

            return File.Exists(image) ? File.ReadAllBytesAsync(image) : File.ReadAllBytesAsync(Path.Combine(AppContext.BaseDirectory, "Static", "notfound.jpg"));
        }

        public Task<byte[]> GetGeneratedImage(string name, bool thumb)
        {
            var sanitisedName = new string(name.Where(c => char.IsDigit(c) || c == '-').ToArray());
            var image = this.FileService.GetImagePath(thumb ? Constants.GenThumbPath : Constants.GenPath, sanitisedName, Constants.ImageExtension);

            return File.Exists(image) ? File.ReadAllBytesAsync(image) : File.ReadAllBytesAsync(Path.Combine(AppContext.BaseDirectory, "Static", "notfound.jpg"));
        }

        public string? GetNewestCameraSnapshot()
        {
            var files = Directory.GetFiles(this.FileService.GetImagesPath(Constants.SnapPath)).OrderByDescending(f => f);
            return files.FirstOrDefault();
        }
        public string? GetNewestGeneratedImage()
        {
            var files = Directory.GetFiles(this.FileService.GetImagesPath(Constants.GenPath)).OrderByDescending(f => f);
            return files.FirstOrDefault();
        }

        public IList<ImageTuple> GetPreviousImages()
        {
            // Use the snaps as a master list. If generation for a snap has failed that's unfortunate, but it's impossible to have a generated image without a snap
            var snaps = Directory.GetFiles(this.FileService.GetImagesPath(Constants.SnapPath));

            // That leads to some path add and removal though...
            var files = snaps.Select(Path.GetFileName).AsParallel().Select(s => new ImageTuple
                {
                    SnapFile = s,
                    GenFile = s,
                    SnapDescription = ImageEditing.GetImageMetadata(this.FileService.GetImagePath(Constants.SnapPath, s)),
                    GenDescription = ImageEditing.GetImageMetadata(this.FileService.GetImagePath(Constants.GenPath, s))
                }
            ).OrderByDescending(f => f.SnapFile);

            return (files.Count() > 0 ? files.Skip(1) : files).ToList();
        }

    }
}