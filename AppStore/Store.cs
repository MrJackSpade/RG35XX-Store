using AppStore.Git;
using AppStore.Models;
using AppStore.Pages;
using RG35XX.Libraries;
using RG35XX.Libraries.Dialogs;
using System.Text.Json;

namespace AppStore
{
    public partial class Store
    {
        private const string REMOTE_PATH = "https://raw.githubusercontent.com/MrJackSpade/RG35XX-Store/refs/heads/main/Release/{0}/AppStore";

        private const string APP_VERSION = "1.0.4";

        private static readonly JsonSerializerOptions jsonSerializerOptions = new() { TypeInfoResolver = StoreItemContext.Default };

        private readonly AppLauncher _appLauncher;

        private readonly Application _application;

        private readonly ConsoleRenderer _consoleRenderer;

        private readonly DeviceInfo _deviceInfo;

        private readonly GamePadReader _gamePadReader;

        private readonly HttpClient _httpClient;

        private readonly StorageProvider _storageProvider;

        private readonly Func<string, Task<string>> GetFile;

        public Store()
        {
            HttpClientHandler handler = new()
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) => true
            };

            _deviceInfo = new DeviceInfo();
            _httpClient = new HttpClient(handler);
            _storageProvider = new StorageProvider();
            _application = new Application(640, 480);
            _appLauncher = new AppLauncher();

#if DEBUG
            GetFile = this.GetPrivateFile;
#else
            GetFile = GetPublicFile;
#endif
        }

        public async Task Execute()
        {
            _application.Execute();

            try
            {
                string version = await GetFile("version");

                if (version != APP_VERSION)
                {
                    await this.UpdateAppStore();
                }

                if (!_appLauncher.IsDmenuLnPatched())
                {
                    await this.PatchDMenuLn();
                }

                string jsonContent = await GetFile("index.json");

                List<StoreItem> items = JsonSerializer.Deserialize<List<StoreItem>>(jsonContent, jsonSerializerOptions)!;

                ItemListPage itemListPage = new(items, APP_VERSION);

                itemListPage.OnItemSelected += this.ItemListPage_OnItemSelected;

                _application.OpenPage(itemListPage);
            }
            catch (Exception ex)
            {
                await _application.ShowDialog(new Alert("Error", ex.ToString()));

                throw;
            }

            await _application.WaitForClose();
        }

        private async Task PatchDMenuLn()
        {
            Confirm confirm = new("dmenu_ln not patched", "Dmenu_ln must be patched to continue, and will require a restart");

            DialogResult result = await _application.ShowDialog(confirm);

            if (result == DialogResult.OK)
            {
                AppLauncher.PatchDmenuLn();

                Utilities.Reboot();
            }
            else
            {
                System.Environment.Exit(1);
            }
        }

        private async Task UpdateAppStore()
        {
            Confirm alert = new("AppStore version mismatch", "Please update the AppStore to the latest version");

            DialogResult result = await _application.ShowDialog(alert);

            if (result == DialogResult.OK)
            {
                string appPath = Path.Combine(AppContext.BaseDirectory, nameof(AppStore));

                string downloadUrl = string.Format(REMOTE_PATH, new DeviceInfo().GetArchitecture());

                string command = $"wget -O \"{appPath}\" {downloadUrl}; \"{appPath}\"";

                new AppLauncher().LaunchAndExit(command);
            }
            else
            {
                File.AppendAllText("/AppStore.log", "Update rejected");

                System.Environment.Exit(1);
            }
        }

        private async Task<string> GetPrivateFile(string fileName)
        {
            DirectoryInfo? current = new(Directory.GetCurrentDirectory());

            do
            {
                string checkPath = Path.Combine(current.FullName, fileName);

                if (File.Exists(checkPath))
                {
                    return await File.ReadAllTextAsync(checkPath);
                }

                current = current.Parent;
            } while (current != null);

            throw new FileNotFoundException("index.json not found");
        }

        private async Task<string> GetPublicFile(string fileName)
        {
            string indexUrl = $"https://raw.githubusercontent.com/MrJackSpade/RG35XX-Store/refs/heads/main/{fileName}?cache_buster={DateTime.Now.Ticks}";

            return await _httpClient.GetStringAsync(indexUrl);
        }

        private void ItemListPage_OnItemSelected(object? sender, StoreItem item)
        {
            ItemDetailsPage detailsPage = new(item, _storageProvider, _application, _deviceInfo, _appLauncher);

            detailsPage.SetDetails(null);

            Task task = Task.Run(async () =>
            {
                GitRemoteReader remoteReader = new(item.Author, item.Repo, item.Architectures[0].Root, item.Branch);

                GitCommitInfo? commitInfo = await remoteReader.GetCommitInfoAsync();

                detailsPage.SetDetails(commitInfo);
            });

            _application.OpenPage(detailsPage);
        }
    }
}