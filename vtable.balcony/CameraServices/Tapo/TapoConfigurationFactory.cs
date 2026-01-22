using Vtable.Balcony.Helpers;
using Vtable.Balcony.Models;

namespace Vtable.Balcony.CameraServices.Tapo
{
    public class TapoConfigurationFactory : IConfigurationFactory<TapoConfiguration>
    {
        private IConfigurationSection Section { get; }

        public TapoConfigurationFactory(IConfiguration config)
        {
            this.Section = config.GetSection($"{Constants.ApplicationName}:Tapo");
        }

        public TapoConfiguration? Get()
        {
            return this.Section?.Get<TapoConfiguration>();
        }
    }
}
