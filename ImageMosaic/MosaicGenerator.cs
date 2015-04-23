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
            Bitmap exportImage = new Bitmap(processor.image.Width * dstImageMultiplicator, processor.image.Height * dstImageMultiplicator);
            Graphics graphics = Graphics.FromImage(exportImage);
            int tilesWidth = processor.image.Width / tileSize;
            int tilesHeight = processor.image.Height / tileSize;
            int tileSizeWidth = tileSize * dstImageMultiplicator;
            int tileSizeHeight = tileSize * dstImageMultiplicator;
            int tileCounter = 1;
            IDictionary<string, Image> imagesCache = new Dictionary<string, Image>();
            for (int i = 0; i < tilesHeight; i++)
            {
                for (int j = 0; j < tilesWidth; j++)
                {
                    string imageName = _getTileImageName(imageColors[j, i]);
                    if(!imagesCache.ContainsKey(imageName))
                        imagesCache.Add(imageName, Image.FromFile(imageName));
                    Rectangle srcRect = new Rectangle(0, 0, imagesCache[imageName].Width, imagesCache[imageName].Height);
                    Rectangle dstRect = new Rectangle(j*tileSizeWidth, i* tileSizeHeight, tileSizeWidth, tileSizeHeight);
                    graphics.DrawImage(imagesCache[imageName], dstRect, srcRect, GraphicsUnit.Pixel);
                    Console.Clear();
                    Console.WriteLine("Processed " + tileCounter + " tiles of " + tilesHeight * tilesWidth);
                    tileCounter++;
                }
            }
            exportImage.Save(outputImagePath, ImageFormat.Png);
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
            
            foreach (string imageName in imagesParser.ColorSet.Keys)
            {
                ColorComparer targetColor = new ColorComparer(color.R,color.G, color.B);
                ColorComparer currenColor = new ColorComparer(imagesParser.ColorSet[imageName].R, imagesParser.ColorSet[imageName].G, imagesParser.ColorSet[imageName].B);
                int colorMatch = currenColor.CompareTo(targetColor);
                if (colorMatch < minColorMatch)
                {
                    minColorMatch = colorMatch;
                    currentImageMatch = imageName;
                }
            }
            return currentImageMatch;
        }
    }
}
