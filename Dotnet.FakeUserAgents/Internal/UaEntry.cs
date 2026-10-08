using System.Text.Json.Serialization;

namespace Dotnet.FakeUserAgents.Internal
{
    internal sealed class UaEntry
    {
        [JsonPropertyName("userAgent")] public string UserAgent { get; set; } = string.Empty;
        [JsonPropertyName("browser")] public string Browser { get; set; } = string.Empty;
        [JsonPropertyName("browserVersion")] public string BrowserVersion { get; set; } = string.Empty;
        [JsonPropertyName("deviceType")] public string DeviceType { get; set; } = string.Empty;
        [JsonPropertyName("os")] public string Os { get; set; } = string.Empty;
        [JsonPropertyName("osVersion")] public string OsVersion { get; set; } = string.Empty;
        [JsonPropertyName("device")] public string? Device { get; set; }
    }
}
