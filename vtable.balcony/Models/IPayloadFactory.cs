using Vtable.Balcony.Helpers;

namespace Vtable.Balcony.Models
{
    public interface IPayloadFactory
    {
        Task<IPayload?> Get(ImageData sourceImage);
    }
}