using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace YoutubeConverter
{
    internal static class API
    {
        /// <summary>
        /// Gets the html result from the request
        /// </summary>
        /// <param name="url">Target url to hit</param>
        /// <param name="headers">Optional headers that can be included</param>
        /// <returns></returns>
        public static string GetHtml(string url, List<KeyValuePair<string, string>> headers = null)
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

        /// <summary>
        /// Gets the song info from the Shazam api
        /// </summary>
        /// <param name="title">Title of the song</param>
        /// <param name="apiKey">API key from Shazam</param>
        /// <returns></returns>
        public static Song GetSongInfo(string title, string apiKey)
        {
            string searchTrackRootUrl = "https://shazam.p.rapidapi.com/search?locale=en-US&offset=0&limit=5&term=";
            string albumRootUrl = "https://shazam.p.rapidapi.com/songs/get-details?locale=en-US&key=";
            string searchTrackUrl = searchTrackRootUrl + ExtractTerms(title);
            List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("x-rapidapi-host", "shazam.p.rapidapi.com"),
                new KeyValuePair<string, string>("x-rapidapi-key", apiKey)
            };
            string songInfo = GetHtml(searchTrackUrl, headers);
            // Check if there is no song retrieved
            if (songInfo == "{}")
            {
                return null;
            }
            Song song = GetSongInfo(songInfo);
            string albumUrl = albumRootUrl + song.KeyId;
            string albumInfo = GetHtml(albumUrl, headers);
            song.Album = GetAlbum(albumInfo);
            return song;
        }

        /// <summary>
        /// Gets the album name from a json string
        /// </summary>
        /// <param name="html">json that contains the album name</param>
        /// <returns></returns>
        private static string GetAlbum(string html)
        {
            dynamic result = JsonConvert.DeserializeObject(html);
            string album = result.sections[0].metadata[0].text;
            return album;
        }

        /// <summary>
        /// Gets the lyrics of the song (uses the same html from the albumUrl result)
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private static string GetLyrics(string html)
        {
            dynamic result = JsonConvert.DeserializeObject(html);
            string lyrics = result.sections[1].text;
            return lyrics;
        }

        /// <summary>
        /// Gets the song information
        /// </summary>
        /// <param name="html">json that contains the search information</param>
        /// <returns></returns>
        private static Song GetSongInfo(string html)
        {
            Song song = new Song();
            dynamic result = JsonConvert.DeserializeObject(html);
            string title = result.tracks.hits[0].track.title;
            string artist = result.tracks.hits[0].track.subtitle;
            string key = result.tracks.hits[0].track.key;
            song.Title = title;
            song.KeyId = key;
            song.Artist = artist;
            return song;
        }

        /// <summary>
        /// Extracts key words that is used for the Shazam api
        /// </summary>
        /// <param name="title">Title of the song</param>
        /// <returns></returns>
        private static string ExtractTerms(string title)
        {
            string[] invalidTerms = { "Lyrics", "lyrics", "(Lyrics)", "(lyrics)", "+", "-", "_", "mp4" };
            string[] terms = title.Split(new char[] { ' ', '.', ',' });
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