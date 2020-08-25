using System.IO;
using System.Net;

namespace YoutubeConverter
{
    internal class Downloader
    {
        public string Download(string url, string fileName)
        {
            string currentDir = Directory.GetCurrentDirectory();
            string newFileName = currentDir + "\\" + fileName;
            using (var client = new WebClient())
            {
                client.DownloadFile(url, newFileName);
            }
            return newFileName;
        }
    }
}