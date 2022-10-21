using Backend.Enums;
using Backend.Utils;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace Backend.Managers.ServerManagerComponents
{
    public class ServerDownloader
    {
        private HttpClient httpClient = new();

        public async void DownloadServer(Branch branch, Platform platform, Modules[] modules, String path)
        {
            if (Directory.Exists(path))
            {
                if(Directory.EnumerateFileSystemEntries(path).Any())
                {
                    Console.WriteLine($"Tried to download Server to path {path}, but failed because the directory is not empty.");
                    return;
                }
            } else
            {
                Directory.CreateDirectory(path);
            }

            CheckIfAllDirectoriesExists(path);
            await DownloadAllFiles(path, BuildLinks(branch, platform, modules));
            BuildServerTOMLConfigFile(path, modules);
        }

        private Dictionary<string, string> BuildLinks(Branch branch, Platform platform, Modules[] modules)
        {
            Dictionary<string, string> links = new Dictionary<string, string>();

            #region Shared Files
            links.Add("data/vehmodels.bin", $"https://cdn.altv.mp/data/{branch}/data/vehmodels.bin");
            links.Add("data/vehmods.bin", $"https://cdn.altv.mp/data/{branch}/data/vehmods.bin");
            links.Add("data/clothes.bin", $"https://cdn.altv.mp/data/{branch}/data/clothes.bin");
            links.Add("data/pedmodels.bin", $"https://cdn.altv.mp/data/{branch}/data/pedmodels.bin");
            #endregion

            #region OS Dependend Files
            if(platform == Platform.x64_linux)
            {
                if(modules.Contains(Modules.javascript))
                {
                    links.Add("modules/libjs-module.so", $"https://cdn.altv.mp/js-module/{branch}/{platform}/modules/js-module/libjs-module.so");
                    links.Add("libnode.so.102", $"https://cdn.altv.mp/js-module/{branch}/{platform}/modules/js-module/libnode.so.102");
                }
                if (modules.Contains(Modules.csharp))
                {
                    links.Add("modules/libcsharp-module.so", $"https://cdn.altv.mp/coreclr-module/{branch}/{platform}/modules/libcsharp-module.so");
                    links.Add("AltV.Net.Host.runtimeconfig.json", $"https://cdn.altv.mp/coreclr-module/{branch}/{platform}/AltV.Net.Host.runtimeconfig.json");
                    links.Add("AltV.Net.Host.dll", $"https://cdn.altv.mp/coreclr-module/{branch}/{platform}/AltV.Net.Host.dll");
                }
                links.Add("start.sh", $"https://cdn.altv.mp/others/start.sh");
                links.Add("altv-server", $"https://cdn.altv.mp/server/{branch}/x64_linux/altv-server");
            } if(platform == Platform.x64_win32 || platform == Platform.win32)
            {
                if (modules.Contains(Modules.javascript))
                {
                    links.Add("modules/js-module.dll", $"https://cdn.altv.mp/js-module/{branch}/{platform}/modules/js-module/js-module.dll");
                    links.Add("libnode.dll", $"https://cdn.altv.mp/js-module/{branch}/{platform}/modules/js-module/libnode.dll");
                }
                if (modules.Contains(Modules.csharp))
                {
                    links.Add("modules/csharp-module.dll", $"https://cdn.altv.mp/coreclr-module/{branch}/{platform}/modules/csharp-module.dll");
                    links.Add("AltV.Net.Host.runtimeconfig.json", $"https://cdn.altv.mp/coreclr-module/{branch}/{platform}/AltV.Net.Host.runtimeconfig.json");
                    links.Add("AltV.Net.Host.dll", $"https://cdn.altv.mp/coreclr-module/{branch}/{platform}/AltV.Net.Host.dll");
                }
                links.Add("altv-server.exe", $"https://cdn.altv.mp/server/{branch}/{platform}/altv-server.exe");
            }
            #endregion

            #region ByteCode Module
            if(modules.Contains(Modules.byteCodeModule) && modules.Contains(Modules.javascript))
            {
                if(branch == Branch.release)
                {
                    if (platform == Platform.x64_linux)
                    {
                        links.Add("modules/libjs-bytecode-module.so", $"https://cdn.altv.mp/js-bytecode-module/{branch}/{platform}/modules/libjs-bytecode-module.so");
                    }
                    if (platform == Platform.x64_win32 || platform == Platform.win32)
                    {
                        links.Add("modules/js-bytecode-module.dll", $"https://cdn.altv.mp/js-bytecode-module/{branch}/{platform}/modules/js-bytecode-module.dll");
                    }
                } else
                {
                    Console.WriteLine($"Cannot download the ByteCode Module for branch {branch}");
                }
            }
            #endregion

            return links;
        }

        private void CheckIfAllDirectoriesExists(string path)
        {
            if (!Directory.Exists($"{path}/data"))
            {
                Directory.CreateDirectory($"{path}/data");
            }
            if (!Directory.Exists($"{path}/modules"))
            {
                Directory.CreateDirectory($"{path}/modules");
            }
        }

        private async Task DownloadAllFiles(string path, Dictionary<string, string> links)
        {
            foreach (var link in links)
            {
                string pathForFile = $"{path}/{link.Key}";
                string linkForFile = link.Value;

                await DownloadFile(linkForFile, pathForFile);
            }

            Console.WriteLine($"Downloaded {links.Count} Files to {path}");
        }

        private async Task<string> DownloadFile(string linkForFile, string pathForFile)
        {
            var fileInfo = new FileInfo(pathForFile);
            Console.WriteLine($"Start download of [{fileInfo.Name}].");

            var response = await httpClient.GetAsync(linkForFile);
            response.EnsureSuccessStatusCode();
            await using var ms = await response.Content.ReadAsStreamAsync();
            await using var fs = File.Create(fileInfo.FullName);
            ms.Seek(0, SeekOrigin.Begin);
            ms.CopyTo(fs);

            Console.WriteLine($"File saved as [{fileInfo.Name}].");
            return fileInfo.FullName;
        }

        private void BuildServerTOMLConfigFile(string path, Modules[] modules)
        {
            TomlTable toml = new TomlTable
            {
                ["announce"] = false,
                ["description"] = "alt:V Server created with lsAdmin",
                ["gamemode"] = "Freeroam",
                ["host"] = "0.0.0.0",
                ["language"] = "en",
                //Add Modules, currently TOML/Tommy cannot append strings to TOMLNode, so idk what i do now | going to fix it later, not important(for now)
                //["modules"] = ,
                ["name"] = "alt:V Server created with lsAdmin",
                ["players"] = 337,
                ["port"] = 7788,
                ["resources"] = new TomlNode[] { },
                ["website"] = "example.com"
            };

            using (StreamWriter writer = File.CreateText($"{path}/server.toml"))
            {
                toml.WriteTo(writer);
                Console.WriteLine($"File saved as [server.toml].");
                writer.Flush();
            }
        }
    }
}
