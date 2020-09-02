using System;
using System.Diagnostics;
using System.IO;
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
            SetDefaultDirectory();
        }

        private void SetDefaultDirectory()
        {
            string currentDir = Directory.GetCurrentDirectory();
            targetDirTextBox.Text = currentDir;
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
            return regex.Matches(html)[6].Groups[1].Value;
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
                //Extract youtube ID from url
                string id = GetYoutubeId(urlTextBox.Text);

                // Get the html page of the resulting api call to youtube 320
                string html = api.GetHtml(youtube320 + id);

                // Grab title of the song
                string songTitle = ExtractTitle(html);

                // Get the download url from the html page
                string downloadUrl = ExtractUrl(html);

                // --------------------------------------------------------------------------------------------------------
                // Hit an api to get the song information
                Song songInfo = api.GetSongInfo(songTitle, "320d5e38afmsh4303ea9c1b8f26dp1ba115jsnee49a5333256");
                // --------------------------------------------------------------------------------------------------------

                // Download the mp3 file form the
                string filePath = videoDownloader.Download(downloadUrl, songInfo.Title);

                // Move the song to its album folder
                string songFilePath = file.CreateAndMoveToAlbum(filePath, targetDirTextBox.Text, songInfo.Title, songInfo.Album);

                // Update the file information
                Mp3.UpdateSongInformation(filePath, songInfo);
            }
            catch (Exception error)
            {
                Debug.WriteLine("Failed to fetch HTML data: " + error);
            }
        }
    }
}