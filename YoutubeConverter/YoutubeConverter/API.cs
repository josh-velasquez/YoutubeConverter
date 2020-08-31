using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace YoutubeConverter
{
    internal class API
    {
        public string GetHtml(string url, List<KeyValuePair<string, string>> headers = null)
        {
            string html = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }
            request.AutomaticDecompression = DecompressionMethods.GZip;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }
            return html;
        }

        public Song GetSongInfo(string title, string apiKey)
        {
            string rooturl = "https://shazam.p.rapidapi.com/search?locale=en-US&offset=0&limit=5&term=";
            string searchUrl = rooturl + ExtractTerms(title);
            List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("x-rapidapi-host", "shazam.p.rapidapi.com"),
                new KeyValuePair<string, string>("x-rapidapi-key", apiKey)
            };

            string html = GetHtml(searchUrl, headers);
            Song song = GetSongInfo(html);
            return new Song();
        }

        private Song GetSongInfo(string html)
        {
            // Convert html result to json object
            return new Song();
        }

        private string ExtractTerms(string title)
        {
            string[] invalidTerms = { "Lyrics", "lyrics", "(Lyrics)", "(lyrics)" };
            string[] terms = title.Split(' ');
            string searchTerms = "";
            foreach (string term in terms)
            {
                if (!invalidTerms.Contains(term))
                {
                    searchTerms += "%20" + term;
                }
            }
            return searchTerms;
        }
    }
}