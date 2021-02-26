using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace YoutubeConverter
{
    internal static class Downloader
    {
        /// <summary>
        /// Downloads the file from a url and saves it based on the filename
        /// </summary>
        /// <param name="url"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Downloads files from bytes
        /// </summary>
        /// <param name="fileBytes"></param>
        /// <param name="fileName"></param>
        /// <param name="fileType"></param>
        /// <returns></returns>
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