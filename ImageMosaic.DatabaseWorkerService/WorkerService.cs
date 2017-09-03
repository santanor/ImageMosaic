using ImageMosaic.Domain.Model;
using ImageMosaic.ImageProcessing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageMosaic.DatabaseWorkerService
{
    public class WorkerService
    {
        private ImageIOController _io;
        private const string _imagesPath = "D:\\ReferenceImages";
        

        public WorkerService()
        {
            this._io = new ImageIOController(_imagesPath);
        }

        public void Run()
        {
            while (true)
            {
                var context = new ImageMosaicContext();
                var imagePath = "";
                var newImagePath = "";
                var image = _io.GetNextImage(out imagePath);
                if (image == null) continue;
                var stream = ImageToStream(image);
                var parser = new ReferenceImageParser(stream);
                var color = parser.GetDominantColor();
                newImagePath = _io.GetNewPath(imagePath);
                SaveImageToDb(color, newImagePath, context);
                image.Dispose();
                _io.MoveLastImage(imagePath, newImagePath);
            }
        }
               
        private MemoryStream ImageToStream(Image image)
        {
            var ms = new MemoryStream();
            image.Save(ms, image.RawFormat);
            return ms;
        }

        private bool SaveImageToDb(Color color, string path, ImageMosaicContext context)
        { 
            try{
                bool success = true;
                var imageInfo = new ImageInfo
                {
                    ArgbColor = color.ToArgb(),
                    ImagePath = path
                };
                context.ImageInfo.Add(imageInfo);
                context.SaveChanges();
                return success;
            }
            catch (Exception e)
            {
                return false;
            }
            
        }
    }
}
