namespace YoutubeConverter
{
    internal static class Config
    {
        /// <summary>
        /// Remembers if the user wants the api key remembered
        /// </summary>
        /// <returns></returns>
        public static bool RememberApiKey()
        {
            return Properties.Settings.Default.rememberApiKey;
        }

        /// <summary>
        /// Gets the api key from the app config
        /// </summary>
        /// <returns></returns>
        public static string GetApiKey()
        {
            return Properties.Settings.Default.apiKey;
        }

        /// <summary>
        /// Saves the api key to app config
        /// </summary>
        /// <param name="apiKey"></param>
        public static void SaveApiKey(string apiKey)
        {
            Properties.Settings.Default.apiKey = apiKey;
            Properties.Settings.Default.rememberApiKey = true;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Deletes the api key from app config
        /// </summary>
        public static void DeleteApiKey()
        {
            Properties.Settings.Default.apiKey = string.Empty;
            Properties.Settings.Default.rememberApiKey = false;
            Properties.Settings.Default.Save();
        }
    }
}