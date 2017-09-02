using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMosaic.ImageProcessing
{
    public class ReferenceImageParser
    {
        private Stream _imageStream;

        public ReferenceImageParser(Stream image)
        {
            this._imageStream = image;
        }

        public Bitmap ImageToBitmap(ImageFormat format)
        {
            BinaryReader b = new BinaryReader(this._imageStream);
            this._imageStream.Position = 0;
            byte[] binData = b.ReadBytes((int)this._imageStream.Length);

            Bitmap imageObject = new Bitmap(new MemoryStream(binData));

            MemoryStream stream = new MemoryStream();
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
                ImageProcessor processor = new ImageProcessor(bitmap);
                Color color = processor.GetDominantColor();
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
