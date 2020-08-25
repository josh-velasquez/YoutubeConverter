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

        private string ExtractUrl(string html)
        {
            Regex regex = new Regex("");
            return "url";
        }

        /// <summary>
        /// Gets the youtube ID from the youtube url
        /// </summary>
        /// <param name="url">Url that the id needs to be extracted from</param>
        /// <returns></returns>
        private string GetYoutubeId(string url)
        {
            Regex regex = new Regex("(?<=v\\=|youtu\\.be\\/)\\w+");
            MatchCollection matches = regex.Matches(url);
            return matches[0].Value;
        }

        private string ExtractTitle(string html)
        {
            return "Title";
        }

        private void OnConvertClick(object sender, RoutedEventArgs e)
        {
            string html;
            API api = new API();
            Downloader videoDownloader = new Downloader();
            Files file = new Files();
            try
            {
                html = api.GetHtml(urlTextBox.Text);
                string songTitle = ExtractTitle(html);
                string downloadUrl = ExtractUrl(html);
                string filePath = videoDownloader.Download(downloadUrl);
                Song songInfo = api.GetSongInfo(songTitle);
                file.CreateAndMoveToAlbum(filePath, songInfo.Album);
            }
            catch (Exception error)
            {
                Debug.WriteLine("Failed to fetch HTML data: " + error);
                
            }
        }
    }