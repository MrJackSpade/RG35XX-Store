using System.Text.Json.Serialization;

namespace AppStore
{
    internal class StoreItem
    {
        [JsonPropertyName("author")]
        public string Author { get; set; }

        [JsonPropertyName("branch")]
        public string Branch { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

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
        public List<string> Files { get; set; } = [];

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("repo")]
        public string Repo { get; set; }

        [JsonPropertyName("root")]
        public string Root { get; set; }

        public string RootUrl => $"https://raw.githubusercontent.com/{Author}/{Repo}/refs/heads/{Branch}/{Root}";
    }
}