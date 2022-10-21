using Backend.Managers;
using Backend.Enums;

namespace Backend
{
    public class Program
    {
        public ServerManager serverManager;

        public Program()
        {
            serverManager = new();

            serverManager.serverDownloader.DownloadServer(Branch.dev, Platform.x64_win32, new Modules[] { Modules.csharp }, "D:/ServerManager/Tests");
        }

        static void Main(string[] args)
        {
            new Program();

            Console.Read();
        }
    }
}