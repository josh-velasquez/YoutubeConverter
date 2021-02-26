using MediaToolkit;
using MediaToolkit.Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace YoutubeConverter
{
    internal static class Files
    {
        /// <summary>
        /// Creates a new folder based on the album name and moves the song to there
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="targetDir"></param>
        /// <param name="song"></param>
        /// <param name="album"></param>
        /// <returns></returns>
        public static string CreateAndMoveToAlbum(string filePath, string targetDir, Song song)
        {
            string[] invalidSymbols = { "*", ".", "\"", "/", "\\", "[", "]", ":", ";", "|", "," };
            string album = song.Album;
            foreach (var symbol in invalidSymbols)
            {
                album = album.Replace(symbol, string.Empty);
            }
            string newDir = targetDir + "\\" + album;
            if (!Directory.Exists(newDir))
            {
                try
                {
                    Directory.CreateDirectory(newDir);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error creating directory: " + e);
                }
            }
            // Move file to newly created directory
            string newFilePath = newDir + "\\" + song.Title + ".mp3";
            Directory.Move(filePath, newFilePath);
            return newFilePath;
        }

        /// <summary>
        /// Converts file (mp4) to mp3
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string ConvertFileToMp3(string fileName)
        {
            var newFileName = fileName.Split('.')[0];
            var mp3File = $"{newFileName}.mp3";
            var inputFile = new MediaFile { Filename = fileName };
            var outputFile = new MediaFile { Filename = mp3File };
            using (var engine = new Engine())
            {
                engine.GetMetadata(inputFile);
                engine.Convert(inputFile, outputFile);
            }
            return mp3File;
        }

        /// <summary>
        /// Removes any .bak files that are created when moving the song file
        /// </summary>
        /// <param name="filePath">File path to remove files from</param>
        public static void Cleanup(string filePath, string fileExt)
        {
            string parentDir = Directory.GetParent(filePath).FullName;
            string[] files = Directory.GetFiles(parentDir);
            foreach (string file in files)
            {
                if (Path.GetExtension(file) == fileExt)
                {
                    File.Delete(file);
                }
            }
        }

        /// <summary>
        /// Extracts all the urls from the file name
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string[] ExtractUrls(string file)
        {
            return File.ReadLines(file).ToArray();
        }
    }
}