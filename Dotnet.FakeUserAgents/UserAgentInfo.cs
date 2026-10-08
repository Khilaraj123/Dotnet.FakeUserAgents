namespace Dotnet.FakeUserAgents;

/// <summary>A user agent string plus the metadata describing it.</summary>
public sealed record UserAgentInfo
{
    /// <summary>The full user agent string.</summary>
    public required string UserAgent { get; init; }

    public required Browser Browser { get; init; }
    public required string BrowserVersion { get; init; }
    public required DeviceType DeviceType { get; init; }
    public required OperatingSystemType Os { get; init; }
    public required string OsVersion { get; init; }

    /// <summary>Hardware model where known (e.g. "Pixel 8"); null when not applicable.</summary>
    public string? Device { get; init; }

    /// <summary>The full info as a dictionary with keys:
    /// userAgent, browser, browserVersion, deviceType, os, osVersion, device.</summary>
    public Dictionary<string, string> ToDictionary() => new()
    {
        ["userAgent"] = UserAgent,
        ["browser"] = Browser.ToString(),
        ["browserVersion"] = BrowserVersion,
        ["deviceType"] = DeviceType.ToString(),
        ["os"] = Os.ToString(),
        ["osVersion"] = OsVersion,
        ["device"] = Device ?? string.Empty
    };

    /// <summary>Returns the raw user agent string.</summary>
    public override string ToString() => UserAgent;

    /// <summary>Lets you assign results straight to a string.</summary>
    //public static implicit operator string?(UserAgentInfo? info) => info?.UserAgent;
}