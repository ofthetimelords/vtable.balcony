using Vtable.Balcony.Helpers;
using Vtable.Balcony.Models;

namespace Vtable.Balcony.CameraServices.Tapo
{
    public class TapoCredentialsFactory : ICredentialsFactory<TapoCredentials>
    {
        private IConfigurationSection Section { get; }

        public TapoCredentialsFactory(IConfiguration config)
        {
            this.Section = config.GetSection($"{Constants.ApplicationName}:Tapo:Credentials");
        }

        public TapoCredentials? Get()
        {
            return this?.Section?.Get<TapoCredentials>();
        }
    }
}