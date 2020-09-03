using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
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

        private void UpdateStatusUI(string message)
        {
            this.Dispatcher.Invoke(() =>
            {
                statusListBox.Items.Add(message);
            });
        }

        private void Start(string apikey, string targetDir, string url, string keywords)
        {
            string youtube320 = "https://www.320youtube.com/watch?v=";
            API api = new API();
            Downloader videoDownloader = new Downloader();
            Files file = new Files();
            try
            {
                //Extract youtube ID from url
                string id = GetYoutubeId(url);

                // Get the html page of the resulting api call to youtube 320
                UpdateStatusUI("Sending request to YouTube320 Api...");
                string html = api.GetHtml(youtube320 + id);

                // Grab title of the song
                string songTitle = ExtractTitle(html);

                // Get the download url from the html page
                string downloadUrl = ExtractUrl(html);

                // Hit an api to get the song information
                UpdateStatusUI("Getting song info...");
                Song songInfo = api.GetSongInfo(songTitle, apikey);

                // Download the mp3 file form the
                UpdateStatusUI("Downloading song...");
                string filePath = videoDownloader.Download(downloadUrl, songInfo.Title);

                // Move the song to its album folder
                string songFilePath = file.CreateAndMoveToAlbum(filePath, targetDir, songInfo.Title, songInfo.Album);

                // Update the file information
                UpdateStatusUI("Updating file properties...");
                Mp3.UpdateSongInformation(songFilePath, songInfo);

                // Remove backup files (.bak)
                UpdateStatusUI("Cleaning up files...");
                file.Cleanup(songFilePath);
                UpdateStatusUI("Finished. File location: " + songFilePath);
            }
            catch (Exception error)
            {
                MessageBox.Show("Failed to convert song: " + error);
            }
        }

        private void OnConvertClick(object sender, RoutedEventArgs e)
        {
            string apiKey = apiKeyTextBox.Text;
            string targetDir = targetDirTextBox.Text;
            string url = urlTextBox.Text;
            string keywords = keywordsTextBox.Text;
            new Thread(() => Start(apiKey, targetDir, url, keywords)).Start();
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            urlTextBox.Text = "";
            statusListBox.Items.Clear();
        }
    }
}