using System;
using System.Diagnostics;
using System.IO;

namespace YoutubeConverter
{
    internal class Files
    {
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
    }
}