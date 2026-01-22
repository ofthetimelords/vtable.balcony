namespace Vtable.Balcony.Models
{
    public interface ICredentialsFactory<T> where T : class
    {
        public T? Get();
    }
}
