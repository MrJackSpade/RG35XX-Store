using AppStore.Git;
using AppStore.Models;
using RG35XX.Core.Drawing;
using RG35XX.Core.Fonts;
using RG35XX.Libraries;
using RG35XX.Libraries.Controls;

namespace AppStore.Pages
{
    internal class ItemInstallerPage : Page
    {
        private readonly Application _application;

        private readonly DeviceInfo _deviceInfo;

        private readonly GitCommitInfo _gitCommitInfo;

        private readonly PictureBox _installerLog;

        private readonly string _installPath;

        private readonly ConsoleRenderer _renderer;

        private readonly StoreItem _storeItem;

        private readonly AppLauncher _appLauncher;

        public ItemInstallerPage(StoreItem item, GitCommitInfo gitCommitInfo, Application application, DeviceInfo deviceInfo, AppLauncher appLauncher, string installPath)
        {
            _storeItem = item;
            _gitCommitInfo = gitCommitInfo;
            _installPath = installPath;
            _application = application;
            _appLauncher = appLauncher;
            _deviceInfo = deviceInfo;

            Window installerWindow = new()
            {
                Bounds = new Bounds(0, 0, 1, 1),
                Title = item.Name,
            };

            this.AddControl(installerWindow);

            _installerLog = new()
            {
                Bounds = new Bounds(0, 0f, 1f, 1f),
                BackgroundColor = Color.Black
            };

            installerWindow.AddControl(_installerLog);

            _renderer = new ConsoleRenderer(ConsoleFont.Px437_IBM_VGA_8x16, null);
            _renderer.Initialize(640, 480);
        }

        public async Task Execute()
        {
            string architecture = _deviceInfo.GetArchitecture();

            HttpClient httpClient = new();

            foreach (GitFile file in _gitCommitInfo.Files)
            {
                string targetPath = Path.Combine(_installPath, file.Path);

                string? localDirectory = Path.GetDirectoryName(targetPath);

                if (string.IsNullOrWhiteSpace(localDirectory))
                {
                    throw new InvalidOperationException("Invalid directory");
                }

                string remotePath = _storeItem.GetRoot(architecture) + "/" + file.Path;

                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }

                if (!Directory.Exists(localDirectory))
                {
                    Directory.CreateDirectory(localDirectory);
                }

                using FileStream fileStream = new(targetPath, FileMode.Create);

                this.WriteLine($"Downloading {file.Path}...");

                HttpResponseMessage response = await httpClient.GetAsync(remotePath);

                if (response.IsSuccessStatusCode)
                {
                    await response.Content.CopyToAsync(fileStream);

                    this.WriteLine("Done", Color.Green);
                }
                else
                {
                    this.WriteLine("Failed", Color.Red);
                }
            }
        }

        private void WriteLine(string text, Color fontColor)
        {
            _renderer.WriteLine(text, fontColor);
            Bitmap bitmap = _renderer.Render();
            _installerLog.Image = bitmap;
        }

        private void WriteLine(string text)
        {
            this.WriteLine(text, Color.White);
        }
    }
}