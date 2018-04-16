using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ImageMosaic.DatabaseWorkerService
{
    public class ImageIoController
    {
        private readonly DirectoryInfo imagesDirectory;

        public ImageIoController(string path)
        {
            imagesDirectory = new DirectoryInfo(path);
            ImageParsedPath = $"{path}\\Parsed";
        }

        public string CurrentImagePath {get; set;}
        public string ImageParsedPath {get; set;}

        public string GetNewPath(string currentPath)
        {
            var guid = Guid.NewGuid().ToString();
            return $"{ImageParsedPath}\\{guid}.png";
        }

        public Image GetNextImage(out string path)
        {
            try
            {
                var imagePath = imagesDirectory.GetFiles().FirstOrDefault()?.FullName;
                path = imagePath;
                if (imagePath == null)
                {
                    return null;
                }

                CurrentImagePath = imagePath;
                return Image.FromFile(imagePath);
            }
            catch (Exception)
            {
                path = null;
                return null;
            }
        }

        public void MoveLastImage(string currentImage, string newImage)
        {
            try
            {
                File.Copy(currentImage, newImage);
                File.Delete(currentImage);
            }
            catch (Exception e)
            {
            }
        }
    }
}
