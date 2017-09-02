using ImageMosaic.Domain.Model;
using ImageMosaic.ImageProcessing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMosaic.DatabaseWorkerService
{
    public class WorkerService
    {
        private ImageIOController _io;
        private const string _imagesPath = "D:\\ReferenceImages";
        private ImageMosaicContext context;

        public WorkerService()
        {
            this._io = new ImageIOController(_imagesPath);
            context = new ImageMosaicContext();
        }

        public void Run()
        {
            while (true)
            {
                Console.WriteLine("Fetching next image");
                var image =_io.GetNextImage();
                if (image == null) continue;
                Console.WriteLine($"{_io.CurrentImagePath} fetched");
                var stream = ImageToStream(image);
                var parser = new ReferenceImageParser(stream);
                var color = parser.GetDominantColor();
                var byteArray = StreamToByte(stream);
                Console.WriteLine("Saving to the database");
                SaveImageToDb(color, byteArray);                
                _io.DeleteLastImage();
                Console.WriteLine("Image deleted");
            }
        }

       
        private MemoryStream ImageToStream(Image image)
        {
            var ms = new MemoryStream();
            image.Save(ms, image.RawFormat);
            return ms;
        }

        private byte[] StreamToByte(MemoryStream stream)
        {
            stream.Position = 0;
            return stream.ToArray();
        }

        private bool SaveImageToDb(Color color, byte[] bytes)
        { 
            try{
                bool success = true;
                var imageInfo = new ImageInfo
                {
                    Color = color,
                    ImageBlob = new ImageBlob
                    {
                        Image = bytes
                    }
                };
                context.ImageInfo.Attach(imageInfo);
                context.SaveChanges();
                return success;
            }
            catch (Exception)
            {
                return false;
            }
            
        }
    }
}
