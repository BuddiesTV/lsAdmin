using Backend.Managers.ServerManagerComponents;

namespace Backend.Managers
{
    public class ServerManager
    {
        public ServerDownloader serverDownloader;

        public ServerManager()
        {
            serverDownloader = new();
        }
    }
}
