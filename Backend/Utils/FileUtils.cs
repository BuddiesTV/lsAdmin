using System.Security.Cryptography;
using System.Text;

namespace Backend.Utils
{
    public class FileUtils
    {
        public string GetFileHash(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return Encoding.Default.GetString(md5.ComputeHash(stream));
                }
            }
        }
    }
}
