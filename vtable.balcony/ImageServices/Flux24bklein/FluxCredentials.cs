using System.Net;
using Vtable.Balcony.Models;

namespace Vtable.Balcony.ImageServices.Flux24bklein
{
    public record FluxCredentials
    {
        public string? Key { get; set; }
        public string AuthType { get; set; } = "Key";
    }
}
