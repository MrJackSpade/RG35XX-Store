using AppStore.Git;
using System.Text.Json.Serialization;

namespace AppStore.Models
{
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(AppMeta))]
    [JsonSerializable(typeof(AppMeta[]))]
    [JsonSerializable(typeof(List<AppMeta>))]
    [JsonSerializable(typeof(GitFile))]
    [JsonSerializable(typeof(GitFile[]))]
    [JsonSerializable(typeof(List<GitFile>))]
    [JsonSerializable(typeof(GitCommitInfo))]
    [JsonSerializable(typeof(GitCommitInfo[]))]
    [JsonSerializable(typeof(List<GitCommitInfo>))]
    public partial class AppMetaContext : JsonSerializerContext
    {
    }
}
