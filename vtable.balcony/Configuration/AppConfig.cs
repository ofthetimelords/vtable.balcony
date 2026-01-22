namespace Vtable.Balcony.Configuration
{
    public record AppConfig
    {
        public string? PrivilegedKey { get; init; }
        public int RefreshPeriodMinutes { get; init; } = 1;
        public int ImagesToKeep { get; init; } = 100;
        public int ImagesToShow { get; init; } = 48;
        public string? ImagesPath { get; init; }
        public bool ObfuscateSnapshots { get; init; } = true;
    }
}
