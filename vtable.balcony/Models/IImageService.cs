namespace Vtable.Balcony.Models
{
    public interface IImageService
    {
        string Name { get; }

        Task<IState> SubmitImageAsync(IPayload payload, CancellationToken token);

        Task WaitUntilReadyAsync(IState state, CancellationToken token);

        Task<byte[]> RetrieveImageAsync(IState state, CancellationToken token);
    }
}
