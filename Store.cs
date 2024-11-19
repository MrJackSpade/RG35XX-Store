using RG35XX.Core.Drawing;
using RG35XX.Core.Extensions;
using RG35XX.Core.Fonts;
using RG35XX.Core.GamePads;
using RG35XX.Libraries;
using System.Text.Json;

namespace AppStore
{
    public class Store
    {
        private readonly ConsoleRenderer _consoleRenderer;

        private readonly GamePadReader _gamePadReader;

        private readonly HttpClient _httpClient;

        private readonly StorageProvider _storageProvider;

        public Store()
        {
            HttpClientHandler handler = new()
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) => true
            };

            _httpClient = new HttpClient(handler);

            _gamePadReader = new GamePadReader();
            _gamePadReader.Initialize();

            _consoleRenderer = new ConsoleRenderer(ConsoleFont.Px437_IBM_VGA_8x16);
            _consoleRenderer.Initialize(640, 480);

            _storageProvider = new StorageProvider();
            _gamePadReader.Initialize();
        }

        public async Task Execute()
        {
            try
            {
                string indexUrl = $"https://raw.githubusercontent.com/MrJackSpade/RG35XX-Store/refs/heads/main/index.json?cache_buster={DateTime.Now.Ticks}";

                string jsonContent = await _httpClient.GetStringAsync(indexUrl);

                var options = new JsonSerializerOptions { TypeInfoResolver = StoreItemContext.Default };

                List<StoreItem> items = JsonSerializer.Deserialize<List<StoreItem>>(jsonContent, options)!;

                await this.DisplayStoreItems(items);
            }
            catch (Exception ex)
            {
                _consoleRenderer.WriteLine(ex.Message, Color.Red, Color.Black);
                _consoleRenderer.WriteLine(ex.StackTrace, Color.Red, Color.Black);
                _gamePadReader.ClearBuffer();
                _gamePadReader.WaitForInput();
                Environment.Exit(1);
            }
        }

        private async Task DisplayStoreItems(List<StoreItem> items)
        {
            string boxTop = "┌" + new string('─', _consoleRenderer.Width - 2) + "┐";
            string boxBottom = "└" + new string('─', _consoleRenderer.Width - 2) + "┘";

            int selectedIndex = 0;

            int pageSize = _consoleRenderer.Height / 4;

            int startIndex = (selectedIndex / pageSize) * pageSize;

            try
            {
                do
                {
                    _consoleRenderer.Clear(false);
                    _gamePadReader.ClearBuffer();

                    _consoleRenderer.AutoFlush = false;

                    for (int i = startIndex; i < startIndex + pageSize; i++)
                    {
                        if (i >= items.Count)
                        {
                            break;
                        }

                        Color renderColor = i == selectedIndex ? Color.Green : Color.White;

                        StoreItem item = items[i];

                        string itemText = this.Enclose($"{item.Name} by {item.Author}");
                        string description = this.Enclose(item.Description);

                        _consoleRenderer.Write(boxTop, renderColor, Color.Black);
                        _consoleRenderer.Write(itemText, renderColor, Color.Black);
                        _consoleRenderer.Write(description, renderColor, Color.Black);
                        _consoleRenderer.Write(boxBottom, renderColor, Color.Black);
                    }

                    _consoleRenderer.Flush();

                    GamepadKey key = _gamePadReader.WaitForInput();

                    if (key == GamepadKey.B_DOWN)
                    {
                        Environment.Exit(0);
                    }

                    if (key == GamepadKey.B_UP)
                    {
                        selectedIndex--;

                        if (selectedIndex < 0)
                        {
                            selectedIndex = items.Count - 1;
                        }
                    }

                    if (key is GamepadKey.B_DOWN or GamepadKey.MENU_DOWN)
                    {
                        selectedIndex++;

                        if (selectedIndex >= items.Count)
                        {
                            selectedIndex = 0;
                        }
                    }

                    if (key is GamepadKey.A_DOWN or GamepadKey.START_DOWN)
                    {
                        await this.ShowDetails(items[selectedIndex]);
                    }

                    if (key is GamepadKey.UP)
                    {
                        selectedIndex--;

                        if (selectedIndex < 0)
                        {
                            selectedIndex = items.Count - 1;
                        }
                    }

                    if (key is GamepadKey.DOWN)
                    {
                        selectedIndex++;

                        if (selectedIndex >= items.Count)
                        {
                            selectedIndex = 0;
                        }
                    }

                    if (key is GamepadKey.LEFT)
                    {
                        selectedIndex -= pageSize;

                        if (selectedIndex < 0)
                        {
                            selectedIndex = items.Count - 1;
                        }
                    }

                    if (key is GamepadKey.RIGHT)
                    {
                        selectedIndex += pageSize;

                        if (selectedIndex >= items.Count)
                        {
                            selectedIndex = 0;
                        }
                    }
                } while (true);
            }
            finally
            {
                _consoleRenderer.AutoFlush = true;
                _consoleRenderer.Flush();
            }
        }

        private async Task Download(string fileName, string fileUrl, string localPath)
        {
            _consoleRenderer.WriteLine($"Downloading [{fileName}]...");

            _consoleRenderer.WriteLine("Waiting for response...");

            using MemoryStream memoryStream = new();

            using HttpResponseMessage response = await _httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            long? totalBytes = response.Content.Headers.ContentLength;

            using Stream contentStream = await response.Content.ReadAsStreamAsync();

            long totalRead = 0L;
            byte[] buffer = new byte[8192];
            bool isMoreToRead = true;

            do
            {
                int read = await contentStream.ReadAsync(buffer, 0, buffer.Length);

                if (read == 0)
                {
                    isMoreToRead = false;
                    this.ReportProgress(totalRead, totalBytes);
                    continue;
                }

                await memoryStream.WriteAsync(buffer, 0, read);

                totalRead += read;

                this.ReportProgress(totalRead, totalBytes);
            } while (isMoreToRead);

            FileInfo fileInfo = new(localPath);

            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }

            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            using FileStream fileStream = new(localPath, FileMode.Create, FileAccess.Write);

            memoryStream.Seek(0, SeekOrigin.Begin);

            await memoryStream.CopyToAsync(fileStream);
        }

        private async Task Download(StoreItem storeItem)
        {
            string[] filePaths = storeItem.Files.ToArray();

            string[] downloadUrls = storeItem.DownloadUrls.ToArray();

            for (int i = 0; i < filePaths.Length; i++)
            {
                string filePath = filePaths[i];

                string downloadUrl = downloadUrls[i];

                string localPath = Path.Combine(_storageProvider.MMC, "Roms", "Apps", filePath);

                await this.Download(filePath, downloadUrl, localPath);
            }

            _consoleRenderer.WriteLine();

            _consoleRenderer.WriteLine("Download complete! Press any key to continue.", Color.Green, Color.Black);

            _gamePadReader.ClearBuffer();

            _gamePadReader.WaitForInput();
        }

        private string Enclose(string text)
        {
            if (text.Length > _consoleRenderer.Width - 2)
            {
                text = text[..(_consoleRenderer.Width - 2)];
            }

            text = "│" + text.PadRight(_consoleRenderer.Width - 2, ' ') + "│";

            return text;
        }

        private void ReportProgress(long totalRead, long? totalBytes)
        {
            _consoleRenderer.ClearLine(false);

            int progressBarWith = _consoleRenderer.Width - "Progress [ ]".Length;

            if (totalBytes.HasValue)
            {
                double progress = (double)totalRead / totalBytes.Value;
                int bars = (int)(progressBarWith * progress);

                string progressBar = new('#', bars);

                progressBar = progressBar.PadRight(progressBarWith, ' ');

                _consoleRenderer.Write($"Progress [{progressBar}]");
            }
            else
            {
                _consoleRenderer.Write($"Downloaded {totalRead} bytes");
            }
        }

        private async Task ShowDetails(StoreItem storeItem)
        {
            _consoleRenderer.Clear();
            _gamePadReader.ClearBuffer();

            _consoleRenderer.WriteLine($"Name: {storeItem.Name}");
            _consoleRenderer.WriteLine($"Author: {storeItem.Author}");
            _consoleRenderer.WriteLine($"Description: {storeItem.Description}");
            _consoleRenderer.WriteLine($"Files:");

            bool filesExist = false;

            foreach (string filePath in storeItem.Files)
            {
                bool thisFileExists = false;
                string prefix = "  ";

                string localPath = Path.Combine(_storageProvider.MMC, "Roms", "Apps", filePath);

                if (File.Exists(localPath))
                {
                    thisFileExists = true;
                    filesExist = true;
                    prefix = "! ";
                }

                Color color = thisFileExists ? Color.Yellow : Color.White;

                _consoleRenderer.WriteLine($"{prefix}{filePath}", color, Color.Black);
            }

            if (filesExist)
            {
                _consoleRenderer.WriteLine();
                _consoleRenderer.WriteLine("!!! Some files already exist on your local device !!!", Color.Yellow, Color.Black);
            }

            _consoleRenderer.WriteLine();
            _consoleRenderer.WriteLine("[Press A to download]", Color.Green, Color.Black);
            _consoleRenderer.WriteLine("[Press B to go back]", Color.Red, Color.Black);

            do
            {
                _consoleRenderer.Flush();

                GamepadKey key = _gamePadReader.WaitForInput();

                if (key is GamepadKey.B_DOWN or GamepadKey.MENU_DOWN)
                {
                    break;
                }

                if (key is GamepadKey.A_DOWN or GamepadKey.START_DOWN)
                {
                    try
                    {
                        _consoleRenderer.AutoFlush = true;
                        await this.Download(storeItem);
                    }
                    finally
                    {
                        _consoleRenderer.AutoFlush = false;
                    }

                    break;
                }
            } while (true);
        }
    }
}