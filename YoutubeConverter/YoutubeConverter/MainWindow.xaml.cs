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
        private Song songInfo = null;
        private string downloadUrl = null;
        private string targetDir = null;

        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            SetDefaultDirectory();

            // DEBUG
            apiKeyTextBox.Text = "320d5e38afmsh4303ea9c1b8f26dp1ba115jsnee49a5333256";
            targetDirTextBox.Text = "C:\\Users\\joshv\\Desktop\\Music";
            urlTextBox.Text = "https://www.youtube.com/watch?v=jf0vrsP1i5g&ab_channel=TheGoodVibe";
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
        /// <param name = "html" ></ param >
        /// <returns></returns>
        private string ExtractUrl(string html)
        {
            //Regex regex = new Regex("href=\"(https:\\/\\/s03.ytapivmp3.*?)\"");
            //Regex regex = new Regex("(https:\\/\\/s02.ytapivmp3.*?)\"");
            Regex regex = new Regex("href=\"(.*?)\"");
            return regex.Matches(html)[7].Groups[1].Value;
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
            Dispatcher.Invoke(() =>
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
        private void Start(string apikey, string url)
        {
            string youtube320 = "https://www.320youtube.com/watch?v=";
            API api = new API();
            string songTitle = "";
            songInfo = new Song();
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
                ShowError(status, error.Message.ToString());
                return;
            }
            try
            {
                // Hit an api to get the song information
                UpdateStatusUI("Getting song info...");
                songInfo = api.GetSongInfo(songTitle, apikey);
            }
            catch (Exception error)
            {
                string status = "Failed to get song information.";
                UpdateStatusUI(status, true);
                ShowError(status, error.Message.ToString());
                return;
            }

            UpdateSongInfo();
            UpdateStatusUI("Verify song information and press Download to continue.");
            EnableFields(true);
        }

        /// <summary>
        /// Downloads the song once the information is verified
        /// </summary>
        private void DownloadSong()
        {
            EnableFields(false);

            Files file = new Files();
            Downloader videoDownloader = new Downloader();
            string songFilePath;
            string filePath;
            if (songInfo == null || targetDir == null || downloadUrl == null)
            {
                UpdateStatusUI("Song info error", true);
                return;
            }
            try
            {
                UpdateStatusUI("Downloading song " + songInfo.Title + "...");
                filePath = videoDownloader.Download(downloadUrl, songInfo.Title);
            }
            catch (Exception error)
            {
                string status = "Failed to download song";
                UpdateStatusUI(status, true);
                ShowError(status, error.Message.ToString());
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
                ShowError(status, error.Message.ToString());
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
                ShowError(status, error.Message.ToString());
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
                ShowError(status, error.Message.ToString());
            }

            songInfo = null;
            downloadUrl = null;
            targetDir = null;
        }

        /// <summary>
        /// Enable or disable fields
        /// </summary>
        /// <param name="enable"></param>
        private void EnableFields(bool enable)
        {
            Dispatcher.Invoke(() =>
            {
                DownloadButton.IsEnabled = enable;
                SongTitleTextBox.IsEnabled = enable;
                ArtistTextBox.IsEnabled = enable;
                AlbumTextBox.IsEnabled = enable;
            });
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

        /// <summary>
        /// Updates the textbox for the song information received from api
        /// </summary>
        /// <param name="song"></param>
        private void UpdateSongInfo()
        {
            Dispatcher.Invoke(() =>
            {
                SongTitleTextBox.Text = songInfo.Title;
                ArtistTextBox.Text = songInfo.Artist;
                AlbumTextBox.Text = songInfo.Album;
            });
        }

        private void OnConvertClick(object sender, RoutedEventArgs e)
        {
            if (apiKeyTextBox.Text == "" || targetDirTextBox.Text == "" || urlTextBox.Text == "")
            {
                ShowError("Field Value Missing", "Enter a value into the appropriate field");
                return;
            }
            string apiKey = apiKeyTextBox.Text;
            targetDir = targetDirTextBox.Text;
            string url = urlTextBox.Text;
            new Thread(() => Start(apiKey, url)).Start();
        }

        /// <summary>
        /// Clears fields
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            urlTextBox.Text = "";
            statusListBox.Items.Clear();
            SongTitleTextBox.Text = "";
            ArtistTextBox.Text = "";
            AlbumTextBox.Text = "";
        }

        /// <summary>
        /// Starts the download process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDownloadClick(object sender, RoutedEventArgs e)
        {
            new Thread(() => DownloadSong()).Start();
        }

        /// <summary>
        /// Allows the user to import a text file containing urls
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnImportClick(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Shows the help window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHelpClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Help help = new Help();
            help.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            help.Show();
        }
    }
}