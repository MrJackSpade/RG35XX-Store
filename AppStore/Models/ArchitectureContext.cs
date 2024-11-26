using System.Text.Json.Serialization;
using AppStore.Models;

namespace AppStore
{
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(Architecture))]
    [JsonSerializable(typeof(Architecture[]))]
    public partial class ArchitectureContext : JsonSerializerContext
    {
    }
}