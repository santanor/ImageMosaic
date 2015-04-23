using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMosaic
{
    class MosaicGenerator
    {

        private string inputImagePath;
        private string outputImagePath;
        private ReferenceImagesParser imagesParser;
        private Object thisLock = new Object();

        public MosaicGenerator(string inputImagePath, string outputImagePath, ReferenceImagesParser imagesParser)
        {
            this.inputImagePath = inputImagePath;
            this.outputImagePath = outputImagePath;
            this.imagesParser = imagesParser;
        }

        public void GenerateImageMosaic(int tileSize, int dstImageMultiplicator)
        {
            ImageProcessor processor = new ImageProcessor(inputImagePath, tileSize);
            Color[,] imageColors = processor.GetColorMatrix();
            while (processor.image.Width*dstImageMultiplicator > 10000 ||
                   processor.image.Height*dstImageMultiplicator > 10000)
                dstImageMultiplicator--;
            Bitmap exportImage = new Bitmap(processor.image.Width * dstImageMultiplicator, processor.image.Height * dstImageMultiplicator);
            Graphics graphics = Graphics.FromImage(exportImage);
            int tilesWidth = processor.image.Width / tileSize;
            int tilesHeight = processor.image.Height / tileSize;
            int tileSizeWidth = tileSize * dstImageMultiplicator;
            int tileSizeHeight = tileSize * dstImageMultiplicator;
            int tileCounter = 1;
            for (int i = 0; i < tilesHeight; i++)
            {
                for (int j = 0; j < tilesWidth; j++)
                {
                    string imageName = _getTileImageName(imageColors[j, i]);
                    Image image =  Image.FromFile(imageName);
                    Rectangle srcRect = new Rectangle(0, 0, image.Width, image.Height);
                    Rectangle dstRect = new Rectangle(j*tileSizeWidth, i* tileSizeHeight, tileSizeWidth, tileSizeHeight);
                    graphics.DrawImage(image, dstRect, srcRect, GraphicsUnit.Pixel);
                    Console.Clear();
                    Console.WriteLine("Processed " + tileCounter + " tiles of " + tilesHeight * tilesWidth);
                    tileCounter++;
                }
                System.GC.Collect();
            }
            exportImage.Save(outputImagePath+".jpg", ImageFormat.Jpeg);
        }

        /// <summary>
        /// returns the name of the image that matches with the given color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private string _getTileImageName(Color color)
        {
            string currentImageMatch = "";
            int minColorMatch = Int32.MaxValue;

            Parallel.ForEach(imagesParser.ColorSet.Keys, (imageName) =>
            {
                ColorComparer targetColor = new ColorComparer(color.R, color.G, color.B);
                ColorComparer currenColor = new ColorComparer(imagesParser.ColorSet[imageName].R, imagesParser.ColorSet[imageName].G, imagesParser.ColorSet[imageName].B);

                lock (thisLock)
                {
                    int colorMatch = currenColor.CompareTo(targetColor);

                    if (colorMatch < minColorMatch)
                    {
                        minColorMatch = colorMatch;
                        currentImageMatch = imageName;
                    }
                }
                
            });
            return currentImageMatch;
        }
    }
}
