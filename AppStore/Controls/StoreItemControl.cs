using AppStore.Models;
using RG35XX.Libraries.Controls;

namespace AppStore.Controls
{
    public class StoreItemControl(StoreItem item) : Control
    {
        public StoreItem Item { get; } = item;
    }
}