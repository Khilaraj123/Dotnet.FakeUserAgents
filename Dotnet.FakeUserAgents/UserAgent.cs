using Dotnet.FakeUserAgents.Internal;
using System.Diagnostics.CodeAnalysis;

namespace Dotnet.FakeUserAgents;

/// <summary>
/// Provides realistic browser user agent strings from a built-in database.
/// Filtering methods never throw for "no match" — they degrade gracefully.
/// Use <see cref="TryGet"/> when you need strict matching instead.
/// </summary>
public sealed class UserAgent
{
    private readonly Random _random;

    /// <summary>Creates an instance backed by a thread-safe shared random generator.</summary>
    public UserAgent() : this(Random.Shared) { }

    /// <summary>Creates an instance with a deterministic sequence — handy for tests.</summary>
    public UserAgent(int seed) : this(new Random(seed)) { }

    /// <summary>Creates an instance backed by your own generator (must be thread-safe if shared across threads).</summary>
    public UserAgent(Random? random) => _random = random ?? Random.Shared;

    // ---- Random ---------------------------------------------------------------

    public UserAgentInfo GetRandom() => Get();

    public UserAgentInfo GetRandom(DeviceType device) => Get(device: device);

    public UserAgentInfo GetRandom(OperatingSystemType os) => Get(os: os);

    public UserAgentInfo GetRandom(DeviceType device, OperatingSystemType os) => Get(device: device, os: os);

    // ---- By browser -----------------------------------------------------------

    public UserAgentInfo GetByBrowser(Browser browser) => Get(browser);

    public UserAgentInfo GetByBrowser(Browser browser, DeviceType device) => Get(browser, device);

    public UserAgentInfo GetByBrowser(Browser browser, OperatingSystemType os) => Get(browser, os: os);

    public UserAgentInfo GetByBrowser(Browser browser, DeviceType device, OperatingSystemType os) => Get(browser, device, os);

    /// <summary>Browser by (case-insensitive) name, e.g. "chrome". Unknown names fall back to a random entry.</summary>
    public UserAgentInfo GetByBrowser(string browser) =>
        TryParseBrowser(browser, out var b) ? GetByBrowser(b) : GetRandom();

    public UserAgentInfo GetByBrowser(string browser, DeviceType device) =>
        TryParseBrowser(browser, out var b) ? GetByBrowser(b, device) : GetRandom(device);

    public UserAgentInfo GetByBrowser(string browser, OperatingSystemType os) =>
        TryParseBrowser(browser, out var b) ? GetByBrowser(b, os) : GetRandom(os);

    public UserAgentInfo GetByBrowser(string browser, DeviceType device, OperatingSystemType os) =>
        TryParseBrowser(browser, out var b) ? GetByBrowser(b, device, os) : GetRandom(device, os);

    // ---- By device / OS -------------------------------------------------------

    public UserAgentInfo GetByDevice(DeviceType device) => Get(device: device);

    public UserAgentInfo GetByOs(OperatingSystemType os) => Get(os: os);

    // ---- General --------------------------------------------------------------

    /// <summary>
    /// Returns a matching entry. If the exact combination doesn't exist (e.g. Safari on Windows),
    /// it degrades gracefully: relaxes OS first, then device, then browser — finally returning anything.
    /// </summary>
    public UserAgentInfo Get(Browser? browser = null, DeviceType? device = null, OperatingSystemType? os = null)
    {
        foreach (var (b, d, o) in FallbackChain(browser, device, os))
        {
            if (TryPick(b, d, o, out var info))
            {
                return info;
            }
        }

        return UaDatabase.DefaultEntry; // ultimate safety net; only hit if the embedded database failed to load
    }

    /// <summary>Strict match — no fallback. Returns false when the combination isn't in the database.</summary>
    public bool TryGet(Browser? browser, DeviceType? device, OperatingSystemType? os, [NotNullWhen(true)] out UserAgentInfo? info) =>
        TryPick(browser, device, os, out info);

    // ---- Dictionary helpers ---------------------------------------------------

    public Dictionary<string, string> GetRandomAsDictionary() => GetRandom().ToDictionary();

    public Dictionary<string, string> GetAsDictionary(
        Browser? browser = null, DeviceType? device = null, OperatingSystemType? os = null) =>
        Get(browser, device, os).ToDictionary();

    // ---- Helpers --------------------------------------------------------------

    /// <summary>Parses a browser name case-insensitively ("chrome", "Firefox", "EDGE", ...).</summary>
    public static bool TryParseBrowser(string? name, out Browser browser)
    {
        if (Enum.TryParse(name?.Trim(), ignoreCase: true, out browser) && Enum.IsDefined(browser))
        {
            return true;
        }

        browser = default;
        return false;
    }

    private bool TryPick(Browser? browser, DeviceType? device, OperatingSystemType? os, [NotNullWhen(true)] out UserAgentInfo? info)
    {
        UserAgentInfo? picked = null;
        int seen = 0;

        foreach (var entry in UaDatabase.Entries)
        {
            if ((browser is null || entry.Browser == browser) &&
                (device is null || entry.DeviceType == device) &&
                (os is null || entry.Os == os))
            {
                seen++;
                // Reservoir sampling: uniform pick in a single pass, no temporary list.
                if (_random.Next(seen) == 0)
                {
                    picked = entry;
                }
            }
        }

        info = picked;
        return picked is not null;
    }

    private static IEnumerable<(Browser? b, DeviceType? d, OperatingSystemType? o)> FallbackChain(Browser? b, DeviceType? d, OperatingSystemType? o)
    {
        var seen = new HashSet<(Browser?, DeviceType?, OperatingSystemType?)>();

        if (seen.Add((b, d, o))) yield return (b, d, o); // exact match

        if (b is not null)
        {
            if (seen.Add((b, d, null)))
                yield return (b, d, null); // relax OS

            if (seen.Add((b, null, o)))
                yield return (b, null, o); // relax device

            if (seen.Add((b, null, null))) 
                yield return (b, null, null); // browser only
        }

        if (seen.Add((null, d, o)))
            yield return (null, d, o); // relax browser

        if (seen.Add((null, d, null)))
            yield return (null, d, null); // device only

        if (seen.Add((null, null, o))) 
            yield return (null, null, o); // os only

        if (seen.Add((null, null, null))) 
            yield return (null, null, null); // anything at all
    }
}