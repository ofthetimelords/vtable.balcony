using OpenCvSharp;
using RedCorners.ExifLibrary;

namespace Vtable.Balcony.Helpers
{
    public static class ImageEditing
    {
        public static bool ObfuscateImage(byte[] imageData, out byte[] result)
        {
            using var source = Cv2.ImDecode(imageData, ImreadModes.Color);
            using var blurred = new Mat();
            using var shrunk = new Mat();
            var pixelated = new Mat();

            Cv2.GaussianBlur(source, blurred, new Size(15, 15), 0);
            Cv2.Resize(blurred, shrunk, new Size(), 0.05, 0.05, InterpolationFlags.Area);
            Cv2.Resize(shrunk, pixelated, new Size(1024, 576), 0, 0, InterpolationFlags.Nearest); // Rethink the size here

            return Cv2.ImEncode(".jpg", pixelated, out result);
        }

        public static bool CreateThumbnail(byte[] imageData, out byte[] result)
        {
            using var source = Cv2.ImDecode(imageData, ImreadModes.Color);
            using var shrunk = new Mat();
            var aspect = source.Width / (double) source.Height;

            Cv2.Resize(source, shrunk, new Size(150 * aspect, 150), 1, 1, InterpolationFlags.Area); // Rethink the size here

            return Cv2.ImEncode(".jpg", shrunk, out result);
        }

        public static void SetImageMetadata(string file, string description)
        {
            var openFile = ImageFile.FromFile(file);
            openFile.Properties.Set(ExifTag.ImageDescription, description);
            openFile.Save(file);
        }

        public static string GetImageMetadata(string file)
        {
            if (!File.Exists(file)) // Generation may have failed. Skip gracefully
                return string.Empty;

            var openFile = ImageFile.FromFile(file);
            var success = openFile.Properties.TryGetValue(ExifTag.ImageDescription, out var description);
            return success ? (description?.Value?.ToString()) ?? string.Empty : string.Empty;
        }
    }
}
