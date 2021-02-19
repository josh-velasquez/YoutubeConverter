using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace YoutubeConverter
{
    internal static class Downloader
    {
        public static string Download(string url, string fileName)
        {
            string currentDir = Directory.GetCurrentDirectory();
            string newFileName = currentDir + "\\" + fileName;
            using (var client = new WebClient())
            {
                client.DownloadFile(url, newFileName);
            }
            return newFileName;
        }

        public static string Download(byte[] fileBytes, string fileName, string fileType)
        {
            string currentDir = Directory.GetCurrentDirectory();
            string newFileName = currentDir + "\\" + fileName + "." + fileType;
            try
            {
                File.WriteAllBytes(newFileName, fileBytes);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to download file: " + e);
            }
            return newFileName;
        }
    }
}