namespace Vtable.Balcony.Models
{
    public interface IConfigurationFactory<T> where T : class
    {
        T? Get();
    }
}
