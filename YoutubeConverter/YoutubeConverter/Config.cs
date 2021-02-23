using System.Diagnostics;

namespace YoutubeConverter
{
    internal static class Config
    {
        public static bool RememberApiKey()
        {
            return Properties.Settings.Default.rememberApiKey;
        }

        public static string GetApiKey()
        {
            return Properties.Settings.Default.apiKey;
        }

        public static void SaveApiKey(string apiKey)
        {
            Debug.WriteLine("SAVED KEY: " + Properties.Settings.Default.apiKey);
            Properties.Settings.Default.apiKey = apiKey;
            Properties.Settings.Default.rememberApiKey = true;
            Properties.Settings.Default.Save();
        }

        public static void DeleteApiKey()
        {
            Debug.WriteLine("REMEMBER KEY: " + Properties.Settings.Default.rememberApiKey);
            Properties.Settings.Default.apiKey = string.Empty;
            Properties.Settings.Default.rememberApiKey = false;
            Properties.Settings.Default.Save();
        }
    }
}