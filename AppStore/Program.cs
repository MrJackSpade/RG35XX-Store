using RG35XX.Libraries;

namespace AppStore
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            await Utilities.CorrectSystemTime();

            Store store = new();

            await store.Execute();
        }
    }
}