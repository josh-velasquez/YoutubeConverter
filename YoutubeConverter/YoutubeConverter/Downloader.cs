using System.IO;
using System.Net;

namespace YoutubeConverter
{
    internal class Downloader
    {
        public string Download(string url)
        {
            string currentDir = Directory.GetCurrentDirectory();
            string fileName = currentDir + "song.mp3";
            using (var client = new WebClient())
            {
                client.DownloadFile(url, fileName);
            }
            return "path";
        }
    }
}