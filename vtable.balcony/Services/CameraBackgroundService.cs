using Microsoft.Extensions.Options;
using OpenCvSharp;
using System.Globalization;
using Vtable.Balcony.CameraServices;
using Vtable.Balcony.Configuration;
using Vtable.Balcony.Factory;
using Vtable.Balcony.Helpers;
using Vtable.Balcony.ImageServices.Flux24bklein;
using Vtable.Balcony.Models;

namespace Vtable.Balcony.Services
{
    /// <summary>
    /// Takes snapshots from the camera and generates images off of them
    /// </summary>
    public class CameraBackgroundService : BackgroundService
    {
        private CameraServiceFactory CameraServiceFactory { get; }
        private ImageServiceFactory ImageServiceFactory { get; }
        private IPayloadFactory PayloadFactory { get; }
        private AppConfig AppConfig { get; }
        private ILogger<CameraBackgroundService> Logger { get; }
        private RefreshTokenSource RefreshTokenSource { get; }
        private FileService FileService { get; }

        public string? LastGeneratedImageTheme { get; private set; }
        public DateTimeOffset? LastGeneratedImageTtimestamp { get; private set; }


        public CameraBackgroundService(CameraServiceFactory cameraServiceFactory,
            ImageServiceFactory imageServiceFactory,
            IPayloadFactory payloadFactory,
            IOptions<AppConfig> appConfig,
            RefreshTokenSource refreshTokenSource,
            FileService fileService,
            ILogger<CameraBackgroundService> logger)
        {
            this.CameraServiceFactory = cameraServiceFactory;
            this.ImageServiceFactory = imageServiceFactory;
            this.PayloadFactory = payloadFactory;
            this.AppConfig = appConfig.Value;
            this.RefreshTokenSource = refreshTokenSource;
            this.FileService = fileService;
            this.Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, this.RefreshTokenSource.Get());

                while (!linkedTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        var timestamp = DateTimeOffset.Now.ToString(Constants.FileDateFormat, CultureInfo.InvariantCulture);

                        var imageData = await this.CreateCameraSnapshot(timestamp, stoppingToken);

                        if (imageData is not null)
                            this.LastGeneratedImageTheme = await this.GenerateImage(imageData, timestamp, stoppingToken);

                        await Task.Delay(TimeSpan.FromMinutes(this.AppConfig.RefreshPeriodMinutes), linkedTokenSource.Token);
                    }
                    catch (OperationCanceledException) { }
                }

                this.RefreshTokenSource.Reset();
            }
        }

        private async Task<ImageData> CreateCameraSnapshot(string timestamp, CancellationToken stoppingToken)
        {
            var factory = this.CameraServiceFactory.Get();
            var dimensions = factory.RetrieveFrame(out byte[] imageData);

            bool obfuscatedOk;
            byte[]? obfuscated = null;
            if (this.AppConfig.ObfuscateSnapshots)
            {
                obfuscatedOk = ImageEditing.ObfuscateImage(imageData, out obfuscated);
            }
            else
                obfuscated = imageData;

            var genThumbOk = ImageEditing.CreateThumbnail(obfuscated, out byte[] genImageThumb);


            var file = this.FileService.GetImagePath(Constants.SnapPath, timestamp, Constants.ImageExtension);
            var fileThumb = this.FileService.GetImagePath(Constants.SnapThumbPath, timestamp, Constants.ImageExtension);

            await File.WriteAllBytesAsync(file, obfuscated, stoppingToken);
            await File.WriteAllBytesAsync(fileThumb, genImageThumb, stoppingToken);
            ImageEditing.SetImageMetadata(file, timestamp);

            this.FileService.CleanOldFiles(Constants.SnapPath);
            this.FileService.CleanOldFiles(Constants.SnapThumbPath);

            this.Logger.LogInformation($"Saved a camera snapshot");

            return new ImageData
            {
                Dimensions = dimensions,
                Image = imageData
            };
        }

        private async Task<string> GenerateImage(ImageData sourceImage, string timestamp, CancellationToken stoppingToken)
        {
            var imageService = this.ImageServiceFactory.Get();
            var payload = await this.PayloadFactory.Get(sourceImage) as FluxPayload;

            if (payload is null)
                throw new InvalidOperationException("Payload could not be created");

            this.LastGeneratedImageTtimestamp = DateTimeOffset.Now;

            var state = await imageService.SubmitImageAsync(payload, stoppingToken);
            await imageService.WaitUntilReadyAsync(state, stoppingToken);
            var genImageData = await imageService.RetrieveImageAsync(state, stoppingToken);

            var genThumbOk = ImageEditing.CreateThumbnail(genImageData, out byte[] genImageThumb);

            var file = this.FileService.GetImagePath(Constants.GenPath, timestamp, Constants.ImageExtension);
            var fileThumb = this.FileService.GetImagePath(Constants.GenThumbPath, timestamp, Constants.ImageExtension);

            await File.WriteAllBytesAsync(file, genImageData, stoppingToken);
            await File.WriteAllBytesAsync(fileThumb, genImageThumb, stoppingToken);
            ImageEditing.SetImageMetadata(file, payload.Theme);

            this.FileService.CleanOldFiles(Constants.GenPath);
            this.FileService.CleanOldFiles(Constants.GenThumbPath);

            this.Logger.LogInformation($"Generated an image with theme {payload.Theme}");

            return payload.Theme;
        }

    }
}