using System;
using System.Diagnostics;
using System.IO;

namespace YoutubeConverter
{
    internal class Files
    {
        public void CreateAndMoveToAlbum(string filePath, string song, string album)
        {
            string currentDir = Directory.GetCurrentDirectory();
            string newDir = currentDir + "\\" + album;
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
            Directory.Move(filePath, newDir + "\\" + song);
        }

        public void MoveToFolder(string path)
        {
        }
    }
}