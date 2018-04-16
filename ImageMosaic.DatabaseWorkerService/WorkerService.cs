using System;
using System.Drawing;
using System.IO;
using ImageMosaic.Domain.Model;
using ImageMosaic.ImageProcessing;

namespace ImageMosaic.DatabaseWorkerService
{
    public class WorkerService
    {
        private const string ImagesPath = "ReferenceImages";
        private readonly ImageIoController io;


        public WorkerService()
        {
            io = new ImageIoController(ImagesPath);
        }

        public void Run()
        {
            while (true)
            {
                var context = new ImageMosaicContext();
                var image = io.GetNextImage(out var imagePath);
                if (image == null)
                {
                    continue;
                }

                var stream = ImageToStream(image);
                var parser = new ReferenceImageParser(stream);
                var color = parser.GetDominantColor();
                var newImagePath = io.GetNewPath(imagePath);
                SaveImageToDb(color, newImagePath, context);
                image.Dispose();
                io.MoveLastImage(imagePath, newImagePath);
            }
        }

        private MemoryStream ImageToStream(Image image)
        {
            var ms = new MemoryStream();
            image.Save(ms, image.RawFormat);
            return ms;
        }

        private void SaveImageToDb(Color color, string path, ImageMosaicContext context)
        {
            try
            {
                var imageInfo = new ImageInfo
                {
                    ArgbColor = color.ToArgb(),
                    ImagePath = path
                };
                context.ImageInfo.Add(imageInfo);
                context.SaveChanges();
            }
            catch (Exception e)
            {
                // ignored
            }
        }
    }
}
