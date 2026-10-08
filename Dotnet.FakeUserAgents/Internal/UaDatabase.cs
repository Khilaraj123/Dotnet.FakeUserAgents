using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dotnet.FakeUserAgents.Internal;

internal static class UaDatabase
{
    private const string ResourceName = "Dotnet.FakeUserAgents.Data.useragents.json";

    private static readonly Lazy<ImmutableArray<UserAgentInfo>> s_entries =
        new(Load, LazyThreadSafetyMode.ExecutionAndPublication);

    public static ImmutableArray<UserAgentInfo> Entries => s_entries.Value;

    // Used only if the embedded resource is missing or corrupt — the library keeps working.
    internal static ImmutableArray<UserAgentInfo> FallbackEntries { get; } = ImmutableArray.Create(
        new UserAgentInfo
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36",
            Browser = Browser.Chrome,
            BrowserVersion = "124.0.0.0",
            DeviceType = DeviceType.Desktop,
            Os = OperatingSystemType.Windows,
            OsVersion = "10/11"
        },
        new UserAgentInfo
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:126.0) Gecko/20100101 Firefox/126.0",
            Browser = Browser.Firefox,
            BrowserVersion = "126.0",
            DeviceType = DeviceType.Desktop,
            Os = OperatingSystemType.Windows,
            OsVersion = "10/11"
        },
        new UserAgentInfo
        {
            UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.4 Safari/605.1.15",
            Browser = Browser.Safari,
            BrowserVersion = "17.4",
            DeviceType = DeviceType.Desktop,
            Os = OperatingSystemType.MacOS,
            OsVersion = "10.15.7"
        },
        new UserAgentInfo
        {
            UserAgent = "Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Mobile Safari/537.36",
            Browser = Browser.Chrome,
            BrowserVersion = "124.0.0.0",
            DeviceType = DeviceType.Mobile,
            Os = OperatingSystemType.Android,
            OsVersion = "14",
            Device = "Pixel 8"
        },
        new UserAgentInfo
        {
            UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 17_4 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.4 Mobile/15E148 Safari/604.1",
            Browser = Browser.Safari,
            BrowserVersion = "17.4",
            DeviceType = DeviceType.Mobile,
            Os = OperatingSystemType.IOS,
            OsVersion = "17.4",
            Device = "iPhone"
        });

    public static UserAgentInfo DefaultEntry => FallbackEntries[0];

    private static ImmutableArray<UserAgentInfo> Load()
    {
        try
        {
            var assembly = typeof(UaDatabase).Assembly;
            using var stream = assembly.GetManifestResourceStream(ResourceName)
                ?? (assembly.GetManifestResourceNames()
                    .FirstOrDefault(n => n.EndsWith("useragents.json", StringComparison.OrdinalIgnoreCase)) is { } fallbackName
                    ? assembly.GetManifestResourceStream(fallbackName)
                    : null);
            if (stream is null) return FallbackEntries;

            var raw = JsonSerializer.Deserialize(stream, UaJsonContext.Default.UaEntryArray);
            if (raw is null || raw.Length == 0) return FallbackEntries;

            var builder = ImmutableArray.CreateBuilder<UserAgentInfo>(raw.Length);
            foreach (var e in raw)
            {
                if (!Enum.TryParse<Browser>(e.Browser, ignoreCase: true, out var browser) ||
                    !Enum.TryParse<DeviceType>(e.DeviceType, ignoreCase: true, out var device) ||
                    !Enum.TryParse<OperatingSystemType>(e.Os, ignoreCase: true, out var os))
                {
                    continue; // skip malformed rows instead of failing the whole database
                }

                builder.Add(new UserAgentInfo
                {
                    UserAgent = e.UserAgent,
                    Browser = browser,
                    BrowserVersion = e.BrowserVersion,
                    DeviceType = device,
                    Os = os,
                    OsVersion = e.OsVersion,
                    Device = string.IsNullOrWhiteSpace(e.Device) ? null : e.Device
                });
            }

            return builder.Count > 0 ? builder.ToImmutable() : FallbackEntries;
        }
        catch
        {
            // Anything goes wrong (resource missing, bad JSON, ...) → still functional with safe defaults.
            return FallbackEntries;
        }
    }
}

