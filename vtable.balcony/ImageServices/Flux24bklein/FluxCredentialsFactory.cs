using Vtable.Balcony.CameraServices;
using Vtable.Balcony.Helpers;
using Vtable.Balcony.Models;

namespace Vtable.Balcony.ImageServices.Flux24bklein
{
    public class FluxCredentialsFactory : ICredentialsFactory<FluxCredentials>
    {
        private IConfigurationSection Section { get; }

        public FluxCredentialsFactory(IConfiguration config)
        {
            this.Section = config.GetSection($"{Constants.ApplicationName}:Flux2_Klein_4b_Edit");
        }

        public FluxCredentials? Get()
        {
            return this?.Section?.Get<FluxCredentials>();
        }
    }
}
