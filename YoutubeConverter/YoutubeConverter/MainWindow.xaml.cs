using Microsoft.Win32;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VideoLibrary;

namespace YoutubeConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Song song = null;
        private YouTubeVideo video = null;
        private string targetDir = null;

        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            SetDefaultDirectory();

#if DEBUG
            apiKeyTextBox.Text = "";
            urlTextBox.Text = "https://www.youtube.com/watch?v=iqp7oiesrwI&ab_channel=LYRIX";
#endif
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
        /// Gets the song information using an api
        /// </summary>
        /// <param name="songTitle"></param>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        private Song getSongInfo(string songTitle, string apiKey)
        {
            Song songInfo = null;
            try
            {
                // Hit an api to get the song information
                UpdateStatusUI("Getting song info...");
                songInfo = API.GetSongInfo(songTitle, apiKey);
            }
            catch (Exception error)
            {
                string status = "Failed to get song information.";
                UpdateStatusUI(status, true);
                ShowError(status, error.Message.ToString());
            }
            return songInfo;
        }

        /// <summary>
        /// Main start of the program
        /// </summary>
        /// <param name="apikey">Key provided by the user (Shazam api key)</param>
        /// <param name="url">YouTube url of the song</param>
        private bool Start(string apikey, string url)
        {
            EnableFields(false);
            YouTube youtube = YouTube.Default;
            video = youtube.GetVideo(url);
            song = getSongInfo(video.FullName, apikey);
            if (song == null)
            {
                return false;
            }
            UpdateSongInfoDisplay();
            UpdateStatusUI("Verify song information and press Download to continue.");
            EnableFields(true);
            return true;
        }

        /// <summary>
        /// Proceeds with the download of the mp4 file
        /// </summary>
        private void ContinueDownload()
        {
            EnableFields(false);
            string songLocation = DownloadSong();
            if (songLocation == "")
            {
                return;
            }
            string mp3File = ConvertFile(songLocation);
            if (mp3File == "")
            {
                return;
            }
            string newSongLocation = MoveSongLocation(mp3File);
            if (newSongLocation == "")
            {
                return;
            }
            UpdateSongInfo(newSongLocation);
            UpdateStatusUI("Finished. File location: " + newSongLocation);
        }

        /// <summary>
        /// Converts mp4 file to mp3
        /// </summary>
        /// <param name="songLocation"></param>
        /// <returns></returns>
        private string ConvertFile(string songLocation)
        {
            string songFilePath = "";
            try
            {
                // Conver the song to mp3 file
                songFilePath = Files.ConvertFileToMp3(songLocation);
            }
            catch (Exception error)
            {
                string status = "Failed to convert song to mp3";
                UpdateStatusUI(status, true);
                ShowError(status, error.Message.ToString());
            }
            try
            {
                // Remove backup files (.mp4)
                UpdateStatusUI("Cleaning up files...");
                Files.Cleanup(songLocation, ".mp4");
            }
            catch (Exception error)
            {
                string status = "Failed to remove mp4 file";
                UpdateStatusUI(status, true);
                ShowError(status, error.Message.ToString());
            }
            return songFilePath;
        }

        /// <summary>
        /// Moves the song location to the album folder
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string MoveSongLocation(string filePath)
        {
            string songFilePath = "";
            try
            {
                // Move the song to its album folder
                songFilePath = Files.CreateAndMoveToAlbum(filePath, targetDir, song);
            }
            catch (Exception error)
            {
                string status = "Failed to move song";
                UpdateStatusUI(status, true);
                ShowError(status, error.Message.ToString());
            }
            return songFilePath;
        }

        /// <summary>
        /// Updates the file information based on the received song information
        /// </summary>
        /// <param name="songLocation"></param>
        private void UpdateSongInfo(string songLocation)
        {
            try
            {
                // Update the file information
                UpdateStatusUI("Updating file properties...");
                Mp3.UpdateSongInformation(songLocation, song);
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
                Files.Cleanup(songLocation, ".bak");
            }
            catch (Exception error)
            {
                string status = "Failed to remove backup file";
                UpdateStatusUI(status, true);
                ShowError(status, error.Message.ToString());
            }
        }

        /// <summary>
        /// Downloads the song once the information is verified
        /// </summary>
        private string DownloadSong()
        {
            string filePath = "";
            if (song == null || targetDir == null || video == null)
            {
                UpdateStatusUI("Song info error", true);
            }
            try
            {
                UpdateStatusUI("Downloading song " + song.Title + "...");
                filePath = Downloader.Download(video.GetBytes(), song.Title, "mp4");
            }
            catch (Exception error)
            {
                string status = "Failed to download song";
                UpdateStatusUI(status, true);
                ShowError(status, error.Message.ToString());
            }
            return filePath;
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
        private void UpdateSongInfoDisplay()
        {
            Dispatcher.Invoke(() =>
            {
                SongTitleTextBox.Text = song.Title;
                ArtistTextBox.Text = song.Artist;
                AlbumTextBox.Text = song.Album;
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
            EnableFields(false);
        }

        /// <summary>
        /// Starts the download process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDownloadClick(object sender, RoutedEventArgs e)
        {
            new Thread(() => ContinueDownload()).Start();
        }

        /// <summary>
        /// Allows the user to import a text file containing urls
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnImportClick(object sender, RoutedEventArgs e)
        {
            if (apiKeyTextBox.Text == "" || targetDirTextBox.Text == "")
            {
                ShowError("Field Value Missing", "Enter a value into the appropriate field");
                return;
            }
            EnableFields(false);
            ReadFile();
        }

        /// <summary>
        /// Reads the file input of urls and automatically downloads them all
        /// </summary>
        private void ReadFile()
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog
                {
                    DefaultExt = "txt",
                    Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
                };
                string fileName = "";
                if (ofd.ShowDialog() != true)
                {
                    return;
                }
                UpdateStatusUI("Reading file urls...");
                fileName = ofd.FileName;
                string apiKey = apiKeyTextBox.Text;
                string dir = targetDirTextBox.Text;
                new Thread(() =>
                {
                    foreach (string url in Files.ExtractUrls(ofd.FileName))
                    {
                        targetDir = dir;
                        if (!Start(apiKey, url))
                        {
                            UpdateStatusUI("Failed to get song information for: " + url);
                        }
                        else
                        {
                            DownloadSong();
                        }
                    }
                }).Start();
                EnableFields(true);
                UpdateStatusUI("Finished downloading imported urls.");
            }
            catch (Exception error)
            {
                string status = "Failed to read file urls";
                UpdateStatusUI(status, true);
                ShowError(status, error.Message.ToString());
            }
        }

        /// <summary>
        /// Shows the help window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHelpClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Help help = new Help
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            help.Show();
        }
    }
}