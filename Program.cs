using AppStore;

namespace RG35XX.AppStore
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Store store = new();

            await store.Execute();
        }
    }
}