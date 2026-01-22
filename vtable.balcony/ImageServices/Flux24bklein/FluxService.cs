using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vtable.Balcony.ImageServices;
using Vtable.Balcony.ImageServices.Flux24bklein;
using Vtable.Balcony.Models;
using static System.Net.WebRequestMethods;

namespace Vtable.Balcony
{
    public partial class FluxService : IImageService, IAsyncDisposable, IDisposable
    {
        // TODO: Abstract this
        private readonly string _apiEndpoint = "https://queue.fal.run/fal-ai/flux-2/klein/4b/edit";
        private FluxCredentials Credentials { get; }
        private HttpClient HttpClient { get; }

        public string Name => "Flux2_Klein_4b_Edit";

        public FluxService(ICredentialsFactory<FluxCredentials> credentialsFactory)
        {
            if (credentialsFactory is null)
                throw new ArgumentNullException(nameof(credentialsFactory));

            this.Credentials = credentialsFactory.Get() ?? throw new InvalidOperationException("Flux credentials were not set or are ivalid");
            this.HttpClient = new HttpClient();

            this.HttpClient.DefaultRequestHeaders.Authorization =new AuthenticationHeaderValue(this.Credentials.AuthType, this.Credentials.Key);

        }

        public async Task<IState> SubmitImageAsync(IPayload payload, CancellationToken token)
        {
            var json = JsonSerializer.Serialize(payload as FluxPayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Submit image
            var response = await this.HttpClient.PostAsync(this._apiEndpoint, content);
            var result = JsonObject.Parse(await response.Content.ReadAsStringAsync());

            if (result is null)
                throw new InvalidOperationException("Could not receive a response from the Flux API");

            var statusUrl = result["status_url"]?.GetValue<string>();
            var responseUrl = result["response_url"]?.GetValue<string>();

            return new FluxState(
                responseUrl ?? throw new InvalidOperationException("Response from the Flux API is invalid (status_url not found)"),
                statusUrl ?? throw new InvalidOperationException("Response from the Flux API is invalid (status_url not found)")
                );
        }

        public async Task WaitUntilReadyAsync(IState state, CancellationToken token)
        {
            while (true)
            {
                var response = await this.HttpClient.GetAsync(state.StatusUrl);
                var result = JsonObject.Parse(await response.Content.ReadAsStringAsync());

                if (result is null)
                    throw new InvalidOperationException("Could not retrieve the status of the request from the Flux API");

                var status = result["status"] is not null ? result["status"]?.GetValue<string>() : throw new InvalidOperationException("Response from the Flux API is invalid (status not found)");

                if (status == "COMPLETED")
                    break;

                await Task.Delay(5000);
            }
        }

        public async Task<byte[]> RetrieveImageAsync(IState state, CancellationToken token)
        {
            var response = await this.HttpClient.GetAsync(state.ResponseUrl, token);
            var stringResponse = await response.Content.ReadAsStringAsync();
            var result = JsonObject.Parse(stringResponse);

            if (result is null)
                throw new InvalidOperationException("Could not retrieve the generated image's details from the Flux API");

            var imageUrl = result?["images"]?[0]?["url"]?.GetValue<string>();

            if (imageUrl is null)
                throw new InvalidOperationException("Could not receive the URL of the generated image from the Flux API");

            var imageDataResponse = await this.HttpClient.GetAsync(imageUrl);
            var resp = await imageDataResponse.Content.ReadAsByteArrayAsync(token);
            return resp;
        }
    }

    #region FluxService Disposable pattern
    public partial class FluxService : IImageService, IAsyncDisposable, IDisposable
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

            this.HttpClient.Dispose();
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
