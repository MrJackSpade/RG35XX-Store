using System.Text.Json.Serialization;

namespace AppStore.Models
{

    public class StoreItem()
    {
        [JsonPropertyName("architectures")]
        public Architecture[] Architectures { get; set; } = [];

        [JsonPropertyName("author")]
        public string Author { get; set; }

        [JsonPropertyName("branch")]
        public string Branch { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("launcher")]
        public string Launcher { get; set; }

        [JsonPropertyName("installer")]
        public string Installer { get; set; }

        [JsonPropertyName("icon")]
        public string? Icon { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("repo")]
        public string Repo { get; set; }

        public string GetRoot(string architecture)
        {
            Architecture? a = Architectures.FirstOrDefault(a => a.Name == architecture) ?? 
                              throw new ArgumentException($"Architecture {architecture} not found");

            return $"https://raw.githubusercontent.com/{Author}/{Repo}/refs/heads/{Branch}/{a.Root}";
        }

        public string GetRoot(int architectureIndex)
        {
            return $"https://raw.githubusercontent.com/{Author}/{Repo}/refs/heads/{Branch}/{Architectures[architectureIndex].Root}";
        }
    }
}