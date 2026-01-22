using System.Drawing;

namespace Vtable.Balcony.ImageServices.Flux24bklein
{
    public class FluxPayloadConfig
    {
        public string? PromptsFile { get; init; }
        public Size ImageSize { get; init; }
        public int InferenceSteps { get; init; }
        public bool SafetyChecker { get; init; }
        public int ThemeSkipCache { get; init; } = 2;
    }
}
