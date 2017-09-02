using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Security.Principal;

namespace ImageMosaic.ImageProcessing
{
    public class ImageProcessor
    {
        public Bitmap image { get; private set; }
        private int squareTam;

        private int tilesWidth;
        private int tilesHeight;
        private int squaresWidth;
        private int squaresHeight;
        
        /// <summary>
        /// Process the image with a given tile size
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="squareTam"></param>
        public ImageProcessor(string imagePath, int squareTam)
        {
            image = new Bitmap(Image.FromFile(imagePath));
            this.squareTam = squareTam;
            tilesWidth = image.Width/squareTam;
            tilesHeight = image.Height/squareTam;
            squaresWidth = squareTam;
            squaresHeight = squareTam;
        }

        /// <summary>
        /// Tries to process the image in a whole tile
        /// </summary>
        /// <param name="imagePath"></param>
        public ImageProcessor(string imagePath)
        {
            image = new Bitmap(Image.FromFile(imagePath));
            this.squareTam = (image.Width < image.Height )? image.Width : image.Height;
            tilesWidth = image.Width / squareTam;
            tilesHeight = image.Height / squareTam;
            squaresWidth = squareTam;
            squaresHeight = squareTam;
        }

        public ImageProcessor(Bitmap bitmap)
        {
            image = bitmap;
            this.squareTam = (image.Width < image.Height) ? image.Width : image.Height;
            tilesWidth = image.Width / squareTam;
            tilesHeight = image.Height / squareTam;
            squaresWidth = squareTam;
            squaresHeight = squareTam;
        }

        /// <summary>
        /// Returns the dominant color of the image
        /// </summary>
        /// <returns></returns>
        public Color GetDominantColor()
        {
            return _processRegion(0, 0, squaresWidth, squaresHeight);
        }

        /// <summary>
        /// Store the image as a mosaic of plain colors
        /// </summary>
        /// <param name="path"></param>
        public void SaveHamaTemplate(string path)
        {
            Bitmap exportImage = new Bitmap(this.image.Width, this.image.Height);
            Graphics graphics = Graphics.FromImage(exportImage);
            SolidBrush[,] brushes = _getBrushes(GetColorMatrix());

            for (int i = 0; i < tilesHeight; i++)
            {
                for (int j = 0; j < tilesWidth; j++)
                {
                    graphics.FillRectangle(new SolidBrush(Color.Black), j * squaresWidth, i * squaresHeight, squaresWidth, squaresHeight);
                    graphics.FillRectangle(brushes[j, i], j * squaresWidth-3, i * squaresHeight-3, squaresWidth-3, squaresHeight-3);
                }
            }
               
            exportImage.Save(path, ImageFormat.Png);
        }

        /// <summary>
        /// Transforms an image to a matrix of the predominant color of each region/tile
        /// </summary>
        /// <returns></returns>
        public Color[,] GetColorMatrix()
        {
            Color[,] colorSquares = new Color[tilesWidth, tilesHeight];

            for (int i = 0; i < tilesWidth - 1; i++)
                for (int j = 0; j < tilesHeight-1; j++)
                    colorSquares[i, j] = _processRegion(squaresWidth*i, squaresHeight*j, squaresWidth, squaresHeight);

            return colorSquares;
        }

        /// <summary>
        /// Process a certain region of the image and returns the average of the color
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private Color _processRegion(int x, int y, int width, int height)
        {
            int r = 0;
            int g = 0;
            int b = 0;
            int a = 0;
            int numberOfPixels = width*height;

            for (int i = x; i < width + x; i++)
            {
                for(int j = y; j < height+y;j++)
                {
                    if (i < image.Width && j < image.Height)
                    {
                        Color pixelColor = image.GetPixel(i, j);
                        r += pixelColor.R;
                        g += pixelColor.G;
                        b += pixelColor.B;
                        a += pixelColor.A;
                    }
                }
            }

            Color finalColor = Color.FromArgb(a/numberOfPixels, r/numberOfPixels, g/numberOfPixels, b/numberOfPixels);
            return finalColor;
        }

       
        /// <summary>
        /// Gets a matrix of brushes to pain plain color in a image
        /// </summary>
        /// <param name="colors"></param>
        /// <returns></returns>
        private SolidBrush[,] _getBrushes(Color[,] colors)
        {
            SolidBrush[,] brushes = new SolidBrush[tilesWidth, tilesHeight];

            for (int i = 0; i < tilesWidth; i++)
                for (int j = 0; j < tilesHeight; j++)
                    brushes[i, j] = new SolidBrush(colors[i, j]);

            return brushes;
        }



    }
}
