using AppStore.Extensions;
using AppStore.Git;
using AppStore.Models;
using RG35XX.Core.Drawing;
using RG35XX.Core.GamePads;
using RG35XX.Libraries;
using RG35XX.Libraries.Controls;
using RG35XX.Libraries.Dialogs;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppStore.Pages
{
    public class ItemDetailsPage : Page
    {
        private AppMeta? _appMeta;

        private readonly string _installPath;

        private readonly StoreItem _storeItem;

        private Button _actionButton;

        private Button _updateButton;

        private Control _detailsPane;

        private GitCommitInfo _gitCommitInfo;

        private List<string> _screenshots = [];

        private int _selectedScreenshot = -1;

        private readonly Application _application;

        private readonly DeviceInfo _deviceInfo;

        private readonly string _appMetaPath;

        private readonly AppLauncher _appLauncher;

        public ItemDetailsPage(StoreItem item, StorageProvider storageProvider, Application application, DeviceInfo deviceInfo, AppLauncher appLauncher)
        {
            _storeItem = item;
            _appLauncher = appLauncher;
            _application = application;
            _deviceInfo = deviceInfo;

            if (!storageProvider.IsSDMounted())
            {
                _installPath = Path.Combine(storageProvider.MMC, "Roms", "APPS", "AppStoreData", item.Author, item.Repo);
            }
            else
            {
                _installPath = Path.Combine(storageProvider.SD, "Roms", "APPS", "AppStoreData", item.Author, item.Repo);
            }

            if(!Directory.Exists(_installPath))
            {
                Directory.CreateDirectory(_installPath);
            }

            _appMetaPath = Path.Combine(_installPath, "appMeta.json");

            Window detailsWindow = new()
            {
                Bounds = new Bounds(0, 0, 1, 1),
                Title = item.Name,
            };

            this.AddControl(detailsWindow);

            detailsWindow.AddControl(new Label
            {
                Bounds = new Bounds(0, 0, 1, 0.1f),
                Text = "Connecting to Git..."
            });
        }

        public override void OnKey(GamepadKey key)
        {
            base.OnKey(key);

            if (key.IsCancel())
            {
                this.Close();
            }

            if (key == GamepadKey.DOWN)
            {
                this.SelectNext();
            }

            if (key == GamepadKey.UP)
            {
                this.SelectPrevious();
            }
        }

        public void SetDetails(GitCommitInfo? commitInfo)
        {
            if (File.Exists(_appMetaPath))
            {
                string appMeta = File.ReadAllText(_appMetaPath);

                _appMeta = JsonSerializer.Deserialize<AppMeta>(appMeta, _jsonSerializerOptions);
            }

            string updatedDisplay = "Connecting to Git...";

            if (commitInfo is null)
            {
                commitInfo = _appMeta?.CommitInfo;
            } else
            {
                updatedDisplay = $"{commitInfo.CommitDate:yyyy-MM-dd HH:mm:ss}";

                if(_appMeta is not null && _appMeta.CommitInfo.CommitDate < commitInfo.CommitDate)
                {
                    this.SetUpdate();
                }
            }

            if(commitInfo is null)
            {
                return;
            }

            _gitCommitInfo = commitInfo;

            _screenshots = commitInfo.Files.Where(f => f.Path.StartsWith("Screenshots/") && f.Path.EndsWith(".png")).Select(f => f.Path).ToList();

            float detailsTop = 0;

            if (_screenshots.Count > 0)
            {           
                string screenshotUrl = _storeItem.GetRoot(0) + "/" + _screenshots[0];

                _selectedScreenshot = 0;

                PictureBox screenshot = new()
                {
                    Bounds = new Bounds(0, 0, 1, 0.5f),
                    ScaleMode = ScaleMode.PreserveAspectRatio,
                    Image = new Bitmap("Images/loading.png"),
                    IsSelectable = true
                };

                screenshot.OnKeyPressed += this.Screenshot_OnKeyPressed;

                detailsTop = 0.5f;

                Task t = Task.Run(async () => await screenshot.TryLoadImageAsync(screenshotUrl));

                this.AddControl(screenshot);
            }

            _detailsPane = new TextArea
            {
                Bounds = new Bounds(0, detailsTop, 1, 1 - detailsTop - 0.05f),
                Text = $"""
                Name: {_storeItem.Name}
                Description: {_storeItem.Description}
                Size: {commitInfo.TotalSize.ToFileSizeString()}
                Files: {commitInfo.FileCount}
                Last Update: {updatedDisplay}
                """,
                IsSelectable = false,
                BackgroundColor = FormColors.ControlLight
            };

            this.AddControl(_detailsPane);

            if (_appMeta is null)
            {
                this.SetInstall();
            }
            else
            {
                this.SetOpen();
            }
        }

        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            TypeInfoResolver = AppMetaContext.Default,
            WriteIndented = true
        };

        private async void InstallButton_Click(object? sender, EventArgs e)
        {
            string appDataPath = Path.Combine(_installPath, "AppData");

            ItemInstallerPage installerPage = new(_storeItem, _gitCommitInfo, _application, _deviceInfo, _appLauncher, appDataPath);

            _application.OpenPage(installerPage);

            try
            {
                await installerPage.Execute();

                installerPage.Close();

                _appMeta = new AppMeta() { CommitInfo = _gitCommitInfo };

                string commitInfo = JsonSerializer.Serialize(_appMeta, _jsonSerializerOptions);

                File.WriteAllText(_appMetaPath, commitInfo);

                if (!string.IsNullOrWhiteSpace(_storeItem.Installer))
                {
                    string installerPath = Path.Combine(appDataPath, _storeItem.Installer);

                    _appLauncher.LaunchAndExit(installerPath);
                }

                this.SetOpen();

                if(_updateButton is not null)
                {
                    this.RemoveControl(_updateButton);
                }
            } catch(Exception ex)
            {
                //TODO: Alert
            }
        }

        private void OpenButton_Click(object? sender, EventArgs e)
        {
            string openPath = Path.Combine(_installPath, "AppData", _storeItem.Launcher);

            openPath = $"\"{openPath}\"";

            Task t = Task.Run(() => _application.ShowDialog(new Alert("Launching", "Please wait...", false)));

            _appLauncher.LaunchAndExit(openPath);
        }

        private void Screenshot_OnKeyPressed(object? sender, GamepadKey e)
        {
            int lastScreenshot = _selectedScreenshot;

            switch (e)
            {
                case GamepadKey.LEFT:
                    _selectedScreenshot--;

                    if (_selectedScreenshot < 0)
                    {
                        _selectedScreenshot = _screenshots.Count - 1;
                    }

                    break;

                case GamepadKey.RIGHT:
                    _selectedScreenshot++;

                    if (_selectedScreenshot >= _screenshots.Count)
                    {
                        _selectedScreenshot = 0;
                    }

                    break;
            }

            if (lastScreenshot != _selectedScreenshot)
            {
                lastScreenshot = _selectedScreenshot;

                string screenshotUrl = _storeItem.GetRoot(0) + "/" + _screenshots[_selectedScreenshot];

                PictureBox screenshot = (PictureBox)sender;

                screenshot.Image = new Bitmap("Images/loading.png");

                Task t = Task.Run(async () => await screenshot.TryLoadImageAsync(screenshotUrl));
            }
        }

        private void SetInstall()
        {
            if (_actionButton is not null)
            {
                this.RemoveControl(_actionButton);
            }

            _actionButton = new()
            {
                Bounds = new Bounds(.5f, 0.9f, .5f, 0.1f),
                Text = "Install",
                IsSelectable = true
            };

            _actionButton.Click += this.InstallButton_Click;
            this.AddControl(_actionButton);
        }

        private void SetOpen()
        {
            if (_actionButton is not null)
            {
                this.RemoveControl(_actionButton);
            }

            _actionButton = new()
            {
                Bounds = new Bounds(.5f, 0.9f, .5f, 0.1f),
                Text = "Open",
                IsSelectable = true
            };

            _actionButton.Click += this.OpenButton_Click;
            this.AddControl(_actionButton);
        }

        private void SetUpdate()
        {
            if (_updateButton is not null)
            {
                this.RemoveControl(_updateButton);
            }

            _updateButton = new()
            {
                Bounds = new Bounds(.0f, 0.9f, .5f, 0.1f),
                Text = "Update",
                IsSelectable = true
            };

            _updateButton.Click += this.InstallButton_Click;
            this.AddControl(_updateButton);
        }
    }
}