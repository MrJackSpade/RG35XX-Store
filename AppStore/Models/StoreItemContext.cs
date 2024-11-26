using System.Text.Json.Serialization;
using AppStore.Models;

namespace AppStore
{
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(StoreItem))]
    [JsonSerializable(typeof(List<StoreItem>))]
    public partial class StoreItemContext : JsonSerializerContext
    {
    }
}