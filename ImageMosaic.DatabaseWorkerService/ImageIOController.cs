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

        public ImageIOController(string path)
        {
            this._imagesDirectory = new DirectoryInfo(path);
        }

        public Image GetNextImage()
        {
            try
            {
                var imagePath = _imagesDirectory.GetFiles().FirstOrDefault()?.FullName;
                if (imagePath == null)
                    return null;

                this.CurrentImagePath = imagePath;
                return Image.FromFile(imagePath);
            }catch(Exception)
            {
                return null;
            }
        }

        public void DeleteLastImage()
        {
            try
            {
                File.Delete(CurrentImagePath);
            }
            catch (Exception e)
            {

            }
        }


    }
}
