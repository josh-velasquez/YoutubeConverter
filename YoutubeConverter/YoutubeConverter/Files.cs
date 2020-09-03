using System;
using System.Diagnostics;
using System.IO;

namespace YoutubeConverter
{
    internal class Files
    {
        /// <summary>
        /// Creates a new folder based on the album name and moves the song to there
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="targetDir"></param>
        /// <param name="song"></param>
        /// <param name="album"></param>
        /// <returns></returns>
        public string CreateAndMoveToAlbum(string filePath, string targetDir, string song, string album)
        {
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
            string newFilePath = newDir + "\\" + song + ".mp3";
            Directory.Move(filePath, newFilePath);
            return newFilePath;
        }

        /// <summary>
        /// Removes any .bak files that are created when moving the song file
        /// </summary>
        /// <param name="filePath">File path to remove the .bak files from</param>
        public void Cleanup(string filePath)
        {
            string parentDir = Directory.GetParent(filePath).FullName;
            string[] files = Directory.GetFiles(parentDir);
            foreach (string file in files)
            {
                if (Path.GetExtension(file) == ".bak")
                {
                    File.Delete(file);
                }
            }
        }
    }
}