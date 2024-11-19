using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace AppStore
{
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(StoreItem))]
    [JsonSerializable(typeof(List<StoreItem>))] 
    public partial class StoreItemContext : JsonSerializerContext
    {
    }

    public class StoreItem()
    {
        [JsonPropertyName("author")]
        public required string Author { get; set; }

        [JsonPropertyName("branch")]
        public required string Branch { get; set; }

        [JsonPropertyName("description")]
        public required string Description { get; set; }

        public IEnumerable<string> DownloadUrls
        {
            get
            {
                foreach (string file in Files)
                {
                    yield return $"{RootUrl}/{file}";
                }
            }
        }

        [JsonPropertyName("files")]
        public required List<string> Files { get; set; } = [];

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("repo")]
        public required string Repo { get; set; }

        [JsonPropertyName("root")]
        public required string Root { get; set; }

        public string RootUrl => $"https://raw.githubusercontent.com/{Author}/{Repo}/refs/heads/{Branch}/{Root}";
    }
}