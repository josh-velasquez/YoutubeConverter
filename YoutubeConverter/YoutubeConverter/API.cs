using System;
using System.IO;
using System.Net;

namespace YoutubeConverter
{
    internal class API
    {
        public string GetHtml(string url)
        {
            string html = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }
            return html;
        }

        public Song GetSongInfo(string title)
        {
            return new Song();
        }
    }
}