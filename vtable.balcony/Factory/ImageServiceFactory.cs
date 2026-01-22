using Microsoft.Extensions.Options;
using Vtable.Balcony.CameraServices;
using Vtable.Balcony.Configuration;
using Vtable.Balcony.Models;

namespace Vtable.Balcony.Factory
{
    public class ImageServiceFactory
    {
        private readonly AppServices _services;
        private readonly IDictionary<string, IImageService> _imageServices;

        public ImageServiceFactory(
            IOptions<AppServices> serviceOptions,
            IEnumerable<IImageService> imageServices
            )
        {
            this._services = serviceOptions.Value ?? throw new ArgumentNullException(nameof(serviceOptions));
            this._imageServices = imageServices.ToDictionary(c => c.Name, c => c, StringComparer.OrdinalIgnoreCase) ?? throw new ArgumentNullException(nameof(imageServices));
        }

        public IImageService Get() => this._imageServices[this._services.Image ?? throw new InvalidOperationException("No image service was configured")];
    }
}
