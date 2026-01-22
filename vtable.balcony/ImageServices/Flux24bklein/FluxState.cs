using System.Security.Cryptography.X509Certificates;
using Vtable.Balcony.Models;

namespace Vtable.Balcony.ImageServices.Flux24bklein
{
    public record FluxState(
        string ResponseUrl,
        string StatusUrl
        ) : IState;
}
