using System.Drawing;

namespace Vtable.Balcony.Models
{
    public interface ICameraService
    {
        string Name { get; }
        Size RetrieveFrame(out byte[] imageData);
    }
}
