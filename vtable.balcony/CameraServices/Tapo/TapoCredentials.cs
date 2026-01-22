using Vtable.Balcony.Models;

namespace Vtable.Balcony.CameraServices.Tapo
{
    public record TapoCredentials
    {
        public string? User { get; set; }
        public string? Password { get; set; }
        public string? Endpoint { get; set; }
    }
}