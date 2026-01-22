using Vtable.Balcony.Models;

namespace Vtable.Balcony.Helpers
{
    /// <summary>
    /// Used to enforce a refresh when the API endpoint is called.
    /// </summary>
    public partial class RefreshTokenSource
    {
        public RefreshTokenSource() {
            this.TokenSource = new();
        }

        private CancellationTokenSource TokenSource { get; set; }

        public CancellationToken Get() => this.TokenSource.Token;
        public void Trigger()
        {
            this.TokenSource.Cancel();
        }

        public void Reset()
        {
            this.TokenSource.Dispose();
            this.TokenSource = new CancellationTokenSource();
        }
    }


    #region FluxService Disposable pattern
    public partial class RefreshTokenSource : IAsyncDisposable, IDisposable
    {
        private bool _disposed;

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);

            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            if (this._disposed) return;

            if (this.TokenSource != null)
                this.TokenSource.Dispose();
            this._disposed = true;

            GC.SuppressFinalize(this);
        }

        protected virtual ValueTask DisposeAsyncCore()
        {
            try
            {
                this.Dispose();
                return default;
            }
            catch (Exception exc)
            {
                return ValueTask.FromException(exc);
            }
        }
    }
    #endregion
}
