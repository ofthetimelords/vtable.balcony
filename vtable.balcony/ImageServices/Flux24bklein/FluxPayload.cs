using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Vtable.Balcony.Helpers;
using Vtable.Balcony.Models;

namespace Vtable.Balcony.ImageServices.Flux24bklein
{
    public record FluxPayload : IPayload
    {
        [property: JsonPropertyName("image_urls")]
        public string[] ImageUrl => this.ImageBytes is null ?
                    throw new ArgumentNullException(nameof(this.ImageBytes)) :
                    [$"data:image/jpeg;base64,{Convert.ToBase64String(this.ImageBytes)}"];

        [property: JsonPropertyName("prompt")]
        public string Prompt { get; }
        
        [JsonIgnore]
        public string Theme { get; }

        [property: JsonPropertyName("image_size")]
        [property: JsonConverter(typeof(FluxSizeJsonConverter))]
        public Size ImageSize { get; private set; }

        [property: JsonIgnore]
        public byte[] ImageBytes { get; }

        [property: JsonPropertyName("seed")]
        public int Seed { get; }

        [property: JsonPropertyName("num_inference_steps")]
        public int InferenceSteps { get; }

        [property: JsonPropertyName("output_format")]
        public string OutputFormat => "jpeg";

        [property: JsonPropertyName("enable_safety_checker")]
        public bool SafetyChecker { get; }

        public FluxPayload(string prompt, string theme, Size imageSize, ImageData imageData, int? seed = null, int? inferenceSteps = null, bool safetyChecker = false)
        {
            this.Seed = seed ?? Random.Shared.Next(0, 1000000);
            this.InferenceSteps = inferenceSteps ?? 6;
            this.ImageBytes = imageData.Image;
            this.Prompt = prompt ?? throw new ArgumentNullException(nameof(prompt));
            this.Theme = theme;
            this.ImageSize = imageSize;
            this.SafetyChecker = safetyChecker;
        }
    }
}
