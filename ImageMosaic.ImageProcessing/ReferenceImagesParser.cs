using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImageMosaic.ImageProcessing
{
    public class ReferenceImageParser
    {
        private readonly Stream _imageStream;

        public ReferenceImageParser(Stream image)
        {
            _imageStream = image;
        }

        public Bitmap ImageToBitmap(ImageFormat format)
        {
            var b = new BinaryReader(_imageStream);
            _imageStream.Position = 0;
            var binData = b.ReadBytes((int)_imageStream.Length);

            var imageObject = new Bitmap(new MemoryStream(binData));

            var stream = new MemoryStream();
            imageObject.Save(stream, format);

            return new Bitmap(stream);
        }


        /// <summary>
        /// Process a single image and returns the Dominant Color
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Color GetDominantColor()
        {
            try
            {
                var bitmap = ImageToBitmap(ImageFormat.Png);
                var processor = new ImageProcessor(bitmap);
                var color = processor.GetDominantColor();
                return color;
            }
            catch (Exception)
            {
                return Color.Transparent;
                throw;
            }
        }
    }
}