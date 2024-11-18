using AppStore;
using System.Text.Json;

namespace RG35XX.AppStore.Models
{
    internal class StoreClient(HttpClient httpClient)
    {
        private const string ROOT_URL = "https://github.com/MrJackSpade/RG35XX-Store/blob/main/";

        private readonly HttpClient _httpClient = httpClient;

        private List<StoreItem> _items;

        public async Task Fetch()
        {
            string json = await _httpClient.GetStringAsync(ROOT_URL + "index.json");

            _items = JsonSerializer.Deserialize<List<StoreItem>>(json);
        }
    }
}