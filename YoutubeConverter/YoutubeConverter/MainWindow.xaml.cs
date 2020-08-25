using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;

namespace YoutubeConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
        }

        /// <summary>
        /// Extracts the download url from the resulting html
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private string ExtractUrl(string html)
        {
            //Regex regex = new Regex("href=\"(https:\\/\\/s03.ytapivmp3.*?)\"");
            Regex regex = new Regex("href=\"(.*?)\"");
            return regex.Matches(html)[5].Groups[1].Value;
        }

        /// <summary>
        /// Gets the youtube ID from the youtube url
        /// </summary>
        /// <param name="url">Url that the id needs to be extracted from</param>
        /// <returns></returns>
        private string GetYoutubeId(string url)
        {
            Regex regex = new Regex("(?<=v\\=|youtu\\.be\\/)\\w+");
            return regex.Matches(url)[0].Value;
        }

        /// <summary>
        /// Extracts the title of the YouTube video
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private string ExtractTitle(string html)
        {
            Regex regex = new Regex("<title>(.*?) \\| 320YouTube<\\/title>");
            return regex.Matches(html)[0].Groups[1].Value;
        }

        private void OnConvertClick(object sender, RoutedEventArgs e)
        {
            string youtube320 = "https://www.320youtube.com/watch?v=";
            API api = new API();
            Downloader videoDownloader = new Downloader();
            Files file = new Files();
            try
            {
                // Extract youtube ID from url
                string id = GetYoutubeId(urlTextBox.Text);

                // Get the html page of the resulting api call to youtube 320
                string html = api.GetHtml(youtube320 + id);

                // Grab title of the song
                string songTitle = ExtractTitle(html);

                // Get the download url from the html page
                string downloadUrl = ExtractUrl(html);

                //// Hit an api to get the song information
                //Song songInfo = api.GetSongInfo(songTitle);

                //// Download the mp3 file form the
                string filePath = videoDownloader.Download(downloadUrl, "test.mp3");
                Debug.WriteLine("FILEPATH: " + filePath);

                file.CreateAndMoveToAlbum(filePath, "test.mp3", "songInfo.Album");
            }
            catch (Exception error)
            {
                Debug.WriteLine("Failed to fetch HTML data: " + error);
            }
        }
    }
}