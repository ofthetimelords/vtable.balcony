using Microsoft.Extensions.Options;
using Vtable.Balcony.Configuration;
using Vtable.Balcony.Helpers;

namespace Vtable.Balcony.Services
{
    /// <summary>
    /// Provides methods for handling image files
    /// </summary>
    public class FileService
    {
        private AppConfig AppConfig { get; }
        private ILogger<FileService> Logger { get; }

        public FileService(IOptions<AppConfig> appConfig, ILogger<FileService> logger)
        {
            this.AppConfig = appConfig.Value;
            this.Logger = logger;
        }

        public void CleanOldFiles(string imagePath)
        {
            var path = this.GetImagesPath(imagePath);

            if (this.AppConfig.ImagesToKeep == 0)
                return;

            var files = Directory.GetFiles(path);

            var toDelete = files.OrderByDescending(f => f).Skip(this.AppConfig.ImagesToKeep).ToList();
            toDelete.ForEach(File.Delete);

            this.Logger.LogInformation($"Deleted {toDelete.Count} files with under {path}");
        }

        public string GetImagePath(string? targetDirectory, string? timestamp, string extension = "")
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(targetDirectory, nameof(targetDirectory));
            ArgumentException.ThrowIfNullOrWhiteSpace(timestamp, nameof(timestamp));

            var path = Path.Combine(this.GetBaseImagePath(), targetDirectory);
            if (!Path.Exists(path))
                Directory.CreateDirectory(path);

            return Path.Combine(this.GetBaseImagePath(), targetDirectory, timestamp + extension);
        }

        public string GetImagesPath(string targetDirectory)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(targetDirectory, nameof(targetDirectory));

            var path = Path.Combine(this.GetBaseImagePath(), targetDirectory);
            if (!Path.Exists(path))
                Directory.CreateDirectory(path);

            return Path.Combine(this.GetBaseImagePath(), targetDirectory);
        }

        private string GetBaseImagePath()
        {
            return !string.IsNullOrWhiteSpace(this.AppConfig.ImagesPath) ? this.AppConfig.ImagesPath : Path.GetTempPath();
        }

        public void DeleteFile(string timestamp)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(timestamp, nameof(timestamp));

            string[] images = [
                this.GetImagePath(Constants.SnapPath, timestamp, Constants.ImageExtension),
                    this.GetImagePath(Constants.SnapThumbPath, timestamp, Constants.ImageExtension),
                    this.GetImagePath(Constants.GenPath, timestamp, Constants.ImageExtension),
                    this.GetImagePath(Constants.GenThumbPath, timestamp, Constants.ImageExtension)
                ];

            foreach (string image in images)
                if (File.Exists(image))
                    File.Delete(image);
        }
    }
}
