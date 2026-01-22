namespace Vtable.Balcony.Models
{
    public interface IPayload
    {
        byte[] ImageBytes { get; }
        string OutputFormat { get; }
        string Prompt { get; }
        string Theme { get; }
    }
}