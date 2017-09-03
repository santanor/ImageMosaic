using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMosaic.DatabaseWorkerService
{
    public class ImageIOController
    {
        private DirectoryInfo _imagesDirectory;
        public string CurrentImagePath { get; set; }
        public string ImageParsedPath { get; set; }

        public string GetNewPath(string currentPath)
        {
            var guid = Guid.NewGuid().ToString();
            return $"{ImageParsedPath}\\{guid}.png";
        }

        public ImageIOController(string path)
        {
            this._imagesDirectory = new DirectoryInfo(path);
            this.ImageParsedPath = $"{path}\\Parsed";
        }

        public Image GetNextImage(out string path)
        {
            try
            {
                var imagePath = _imagesDirectory.GetFiles().FirstOrDefault()?.FullName;
                path = imagePath;
                if (imagePath == null)
                    return null;

                this.CurrentImagePath = imagePath;
                return Image.FromFile(imagePath);
            }catch(Exception)
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
