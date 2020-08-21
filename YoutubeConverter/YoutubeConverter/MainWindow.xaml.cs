using System;
using System.Diagnostics;
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
            InitializeComponent();
        }

        private string ExtractUrl(string html)
        {
            return "url";
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
                string filePath = videoDownloader.Download(ExtractUrl(html));

            }
            catch (Exception error)
            {
                Debug.WriteLine("Failed to fetch HTML data: " + error);
            }
        }
    }

}