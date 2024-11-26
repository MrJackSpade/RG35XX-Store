using System.Diagnostics;
using System.Text.Json.Serialization;

namespace AppStore.Models
{
    [DebuggerDisplay("Root = {Root}, Name = {Name}")]
    public class Architecture
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("root")]
        public string Root { get; set; }
    }
}