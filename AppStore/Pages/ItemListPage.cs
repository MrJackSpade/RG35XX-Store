using AppStore.Controls;
using AppStore.Models;
using RG35XX.Core.Drawing;
using RG35XX.Core.GamePads;
using RG35XX.Libraries;
using RG35XX.Libraries.Controls;
using System.Reflection;

namespace AppStore.Pages
{
    public class ItemListPage : Page
    {
        public event EventHandler<StoreItem>? OnItemSelected;

        public ItemListPage(IEnumerable<StoreItem> items, string version)
        {
            Window iconWindow = new()
            {
                Bounds = new Bounds(0, 0, 1, 1),
                Title = $"App Store {version}",
            };

            this.AddControl(iconWindow);

            ListBox itemView = new()
            {
                Bounds = new Bounds(0, 0, 1, 1),
                ItemHeight = 0.125f,
            };

            itemView.OnKeyPressed += this.AppView_OnKeyPressed;

            iconWindow.AddControl(itemView);

            foreach (StoreItem item in items)
            {
                StoreItemControl itemDisplay = new(item)
                {
                    BackgroundColor = Color.Transparent
                };

                PictureBox icon = new()
                {
                    Bounds = new Bounds(0, 0, 0.10f, 1),
                    Image = new Bitmap("Images/loading.png"),
                    BackgroundColor = Color.Transparent,
                };

                if (!string.IsNullOrWhiteSpace(item.Icon))
                {
                    string iconUrl = item.GetRoot(0) + "/" + item.Icon;

                    icon.TryLoadImageAsync(iconUrl);
                }

                Label nameLabel = new()
                {
                    Bounds = new Bounds(0.1f, 0, 0.90f, 0.4f),
                    Text = item.Name,
                    BackgroundColor = Color.Transparent,
                };

                Label descriptionLabel = new()
                {
                    Bounds = new Bounds(0.1f, 0.4f, 0.90f, 0.6f),
                    Text = item.Description,
                    BackgroundColor = Color.Transparent,
                    FontSize = 0.4f
                };

                itemDisplay.AddControl(icon);
                itemDisplay.AddControl(nameLabel);
                itemDisplay.AddControl(descriptionLabel);

                itemView.AddControl(itemDisplay);
            }
        }

        private void AppView_OnKeyPressed(object? sender, GamepadKey e)
        {
            if (e.IsCancel())
            {
                System.Environment.Exit(0);
            }

            if (!e.IsAccept())
            {
                return;
            }

            ListBox source = (ListBox)sender!;

            if (source.SelectedItem is not StoreItemControl itemControl)
            {
                return;
            }

            OnItemSelected?.Invoke(this, itemControl.Item);
        }
    }
}