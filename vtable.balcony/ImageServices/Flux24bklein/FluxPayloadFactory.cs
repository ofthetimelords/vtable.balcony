using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Vtable.Balcony.Helpers;
using Vtable.Balcony.Models;

namespace Vtable.Balcony.ImageServices.Flux24bklein
{
    public class FluxPayloadFactory : IPayloadFactory
    {
        private FluxPayloadConfig? Config { get; }

        // Used to increase randomness
        private Queue<string> ThemesNegativeCache { get; } = new Queue<string>();

        public FluxPayloadFactory(IConfiguration config)
        {
            var section = config.GetSection($"{Constants.ApplicationName}:FluxPayload");
            this.Config = section?.Get<FluxPayloadConfig>();
        }

        public async Task<IPayload?> Get(ImageData sourceImage)
        {
            if (this.Config == null)
                return null;

            if (this.Config.PromptsFile is null)
                throw new InvalidOperationException("The prompts file could not be found");

            var prompts = JsonObject.Parse(await File.ReadAllTextAsync(this.Config.PromptsFile))?["Prompts"]?.AsObject().ToDictionary(p => p.Key, p => p.Value?.ToString()).ToList();

            if (prompts is null)
                throw new InvalidOperationException("The prompts file was empty or malformatted");

            var random = this.SelectRandomTheme(prompts);

            var theme = random.Key;
            var prompt = random.Value;

            if (prompt is null)
                throw new InvalidOperationException("The selected prompt from the prompts file has no actual text");

            return new FluxPayload(
                prompt,
                theme,
                this.Config.ImageSize,
                sourceImage,
                Random.Shared.Next(0, 100000),
                this.Config.InferenceSteps,
                this.Config.SafetyChecker
                );
        }

        private KeyValuePair<string, string?> SelectRandomTheme(List<KeyValuePair<string, string?>> themes)
        {
            if (this.Config == null)
                throw new InvalidOperationException("Flux Payload configuration was null");

            if (this.ThemesNegativeCache.Count >= this.Config.ThemeSkipCache)
                this.ThemesNegativeCache.Dequeue();

            var filteredThemes = themes.ExceptBy(this.ThemesNegativeCache, k => k.Key).ToList();

            var chosen = filteredThemes[Random.Shared.Next(0, filteredThemes.Count)];
            this.ThemesNegativeCache.Enqueue(chosen.Key);

            return chosen;
        }

    }
}
