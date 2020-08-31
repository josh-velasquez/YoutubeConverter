using Mp3Lib;

namespace YoutubeConverter
{
    internal class Mp3
    {
        public static void UpdateSongInformation(string filePath, Song song)
        {
            Mp3File file = new Mp3File(filePath);
            file.TagHandler.Album = song.Album;
            file.TagHandler.Artist = song.Artist;
            file.TagHandler.Title = song.Title;
            file.Update();
        }
    }
}