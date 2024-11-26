using System.Diagnostics;
using System.Text;

namespace AppStore.Git
{
    public class GitCommitInfo
    {
        public DateTime CommitDate { get; set; }

        public List<GitFile> Files { get; set; } = [];

        public ulong TotalSize => (ulong)Files.Sum(f => f.Size);

        public int FileCount => Files.Count;
    }

    public class GitFile
    {
        public string Path { get; set; }

        public long Size { get; set; }
    }

    public class GitRemoteReader
    {
        private readonly string _author;

        private readonly string _branch;

        private readonly string _credentials;

        private readonly string _path;

        private readonly string _repoName;

        private readonly string _tempDir;

        public GitRemoteReader(string author, string repoName, string path, string branch, string credentials = "")
        {
            _author = author;
            _repoName = repoName;
            _path = path;
            _branch = branch;
            _credentials = credentials;
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        }

        public async Task<GitCommitInfo> GetCommitInfoAsync()
        {
            List<GitFile> files = [];
            DateTime commitDate;

            try
            {
                // Create temporary directory
                Directory.CreateDirectory(_tempDir);

                // Initialize git repo
                await this.ExecuteGitCommandAsync("init");

                // Add remote
                string repoUrl = this.BuildRepoUrl();
                await this.ExecuteGitCommandAsync($"remote add origin {repoUrl}");

                // Fetch specific branch and path
                await this.ExecuteGitCommandAsync($"fetch origin {_branch} --depth 1");

                // Get commit date
                string commitDateStr = await this.ExecuteGitCommandAsync("log -1 --format=%cI FETCH_HEAD");
                commitDate = DateTime.Parse(commitDateStr);

                // Get file listing
                string filesCommand = $"ls-tree -r --full-tree -l FETCH_HEAD:{_path}";
                string filesOutput = await this.ExecuteGitCommandAsync(filesCommand);

                foreach (string line in filesOutput.Split('\n'))
                {
                    string[] parts = line.Replace('\t', ' ').Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    files.Add(new GitFile
                    {
                        Path = parts[4],
                        Size = long.Parse(parts[3])
                    });
                }
            }
            finally
            {
                // Cleanup: Remove temporary directory
                try
                {
                    if (Directory.Exists(_tempDir))
                    {
                        Directory.Delete(_tempDir, true);
                    }
                }
                catch
                {
                    // Best effort cleanup
                }
            }

            return new GitCommitInfo
            {
                CommitDate = commitDate,
                Files = files
            };
        }

        private string BuildRepoUrl()
        {
            if (string.IsNullOrEmpty(_credentials))
            {
                return $"https://github.com/{_author}/{_repoName}.git";
            }

            return $"https://{_credentials}@github.com/{_author}/{_repoName}.git";
        }

        private async Task<string> ExecuteGitCommandAsync(string arguments)
        {
            using Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = _tempDir
                }
            };

            StringBuilder output = new();
            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    output.AppendLine(args.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Git command failed with exit code {process.ExitCode}");
            }

            return output.ToString().Trim();
        }
    }
}