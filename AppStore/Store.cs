using AppStore.Git;
using AppStore.Models;
using AppStore.Pages;
using RG35XX.Core.Drawing;
using RG35XX.Core.Extensions;
using RG35XX.Libraries;
using RG35XX.Libraries.Dialogs;
using System.Text.Json;

namespace AppStore
{
    public partial class Store
    {
        private const string APP_VERSION = "1.0.0";

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
                    Alert alert = new("AppStore version mismatch", "Please update the AppStore to the latest version");

                    await _application.ShowDialog(alert);
                }

                if(!_appLauncher.IsDmenuLnPatched())
                {
                    Confirm confirm = new("dmenu_ln not patched", "Dmenu_ln must be patched to continue, and will require a restart");

                    DialogResult result = await _application.ShowDialog(confirm);

                    if (result == DialogResult.OK)
                    {
                        AppLauncher.PatchDmenuLn();

                        Utilities.Reboot();
                    } else
                    {
                        System.Environment.Exit(1);
                    }
                }

                string jsonContent = await GetFile("index.json");

                List<StoreItem> items = JsonSerializer.Deserialize<List<StoreItem>>(jsonContent, jsonSerializerOptions)!;

                ItemListPage itemListPage = new(items);

                itemListPage.OnItemSelected += this.ItemListPage_OnItemSelected;

                _application.OpenPage(itemListPage);
            }
            catch (Exception ex)
            {
                _consoleRenderer.WriteLine(ex.Message, Color.Red, Color.Black);
                _consoleRenderer.WriteLine(ex.StackTrace, Color.Red, Color.Black);
                _gamePadReader.ClearBuffer();
                _gamePadReader.WaitForInput();
                Environment.Exit(1);
            }

            await _application.WaitForClose();
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