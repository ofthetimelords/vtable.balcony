using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using Vtable.Balcony.Configuration;
using Vtable.Balcony.Models;

namespace Vtable.Balcony.Factory
{
    public class CameraServiceFactory
    {
        private readonly AppServices _services;
        private readonly IDictionary<string, ICameraService> _cameraservices;

        public CameraServiceFactory(
            IOptions<AppServices> serviceOptions,
            IEnumerable<ICameraService> cameraServices
            )
        {
            this._services = serviceOptions.Value ?? throw new ArgumentNullException(nameof(serviceOptions));
            this._cameraservices = cameraServices.ToDictionary(c => c.Name, c => c, StringComparer.OrdinalIgnoreCase) ?? throw new ArgumentNullException(nameof(cameraServices));
        }

        public ICameraService Get() => this._cameraservices[this._services.Camera ?? throw new InvalidOperationException("No camera service was configured")];
    }
}
