using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        /// <summary>
        /// Gets the current directory and shows it to the user
        /// </summary>
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

        /// <summary>
        /// Updates the status box in the UI thread
        /// </summary>
        /// <param name="message"></param>
        private void UpdateStatusUI(string message, bool failed = false)
        {
            this.Dispatcher.Invoke(() =>
            {
                statusListBox.Items.Add(new ListBoxItem { Content = message, Background = failed ? Brushes.Red : Brushes.LightGreen });
            });
        }

        /// <summary>
        /// Main start of the program
        /// </summary>
        /// <param name="apikey">Key provided by the user (Shazam api key)</param>
        /// <param name="targetDir">Target directory that the song will be downloaded to</param>
        /// <param name="url">YouTube url of the song</param>
        /// <param name="keywords">Keywords that the user can enter for better searching</param>
        private void Start(string apikey, string targetDir, string url, string keywords)
        {
            string youtube320 = "https://www.320youtube.com/watch?v=";
            API api = new API();
            Downloader videoDownloader = new Downloader();
            Files file = new Files();
            string songTitle = "", downloadUrl = "", filePath = "", songFilePath = "";
            Song songInfo = new Song();

            try
            {
                //Extract youtube ID from url
                string id = GetYoutubeId(url);

                // Get the html page of the resulting api call to youtube 320
                UpdateStatusUI("Sending request to YouTube320 Api...");
                string html = api.GetHtml(youtube320 + id);
                // Grab title of the song
                songTitle = ExtractTitle(html);

                // Get the download url from the html page
                downloadUrl = ExtractUrl(html);
            }
            catch (Exception error)
            {
                string status = "Failed to get download url.";
                UpdateStatusUI(status, true);
                ShowError(status, error.ToString());
                return;
            }
            try
            {
                // Hit an api to get the song information
                UpdateStatusUI("Getting song info...");
                songInfo = api.GetSongInfo(keywords != String.Empty ? keywords : songTitle, apikey);
            }
            catch (Exception error)
            {
                string status = "Failed to get song information.";
                UpdateStatusUI(status, true);
                ShowError(status, error.ToString());
                return;
            }
            try
            {
                // Download the mp3 file form the
                UpdateStatusUI("Downloading song " + songInfo.Title + "...");
                filePath = videoDownloader.Download(downloadUrl, songInfo.Title);
            }
            catch (Exception error)
            {
                string status = "Failed to download song";
                UpdateStatusUI(status, true);
                ShowError(status, error.ToString());
                return;
            }
            try
            {
                // Move the song to its album folder
                songFilePath = file.CreateAndMoveToAlbum(filePath, targetDir, songInfo.Title, songInfo.Album);
            }
            catch (Exception error)
            {
                string status = "Failed to move song";
                UpdateStatusUI(status, true);
                ShowError(status, error.ToString());
                return;
            }
            try
            {
                // Update the file information
                UpdateStatusUI("Updating file properties...");
                Mp3.UpdateSongInformation(songFilePath, songInfo);
            }
            catch (Exception error)
            {
                string status = "Failed to update song properties";
                UpdateStatusUI(status, true);
                ShowError(status, error.ToString());
            }
            try
            {
                // Remove backup files (.bak)
                UpdateStatusUI("Cleaning up files...");
                file.Cleanup(songFilePath);
                UpdateStatusUI("Finished. File location: " + songFilePath);
            }
            catch (Exception error)
            {
                string status = "Failed to remove backup file";
                UpdateStatusUI(status, true);
                ShowError(status, error.ToString());
            }
        }

        /// <summary>
        /// Displays a pop up error message
        /// </summary>
        /// <param name="message">Message to show the user</param>
        /// <param name="error">Error that caused the message to show</param>
        /// <param name="critical">true if the program should stop and not continue; false otherwise</param>
        private void ShowError(string message, string error)
        {
            MessageBox.Show(message + ": " + error);
        }

        private void OnConvertClick(object sender, RoutedEventArgs e)
        {
            string apiKey = apiKeyTextBox.Text;
            string targetDir = targetDirTextBox.Text;
            string url = urlTextBox.Text;
            string keywords = keywordsTextBox.Text;
            keywords = keywords.Replace("+", " ");
            new Thread(() => Start(apiKey, targetDir, url, keywords)).Start();
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            urlTextBox.Text = "";
            statusListBox.Items.Clear();
            keywordsTextBox.Text = "";
        }
    }
}