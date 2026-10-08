using System.Text.Json.Serialization;

namespace Dotnet.FakeUserAgents.Internal
{

    [JsonSerializable(typeof(UaEntry[]))]
    internal sealed partial class UaJsonContext : JsonSerializerContext;
}
