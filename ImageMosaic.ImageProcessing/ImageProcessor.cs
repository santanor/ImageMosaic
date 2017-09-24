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
        /// https://codereview.stackexchange.com/questions/157667/getting-the-dominant-rgb-color-of-a-bitmap
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private Color _processRegion(int x, int y, int width, int height)
        {
            uint uWidth = (uint)width;
            uint uHeight = (uint)height;
            uint pixelCount = uWidth * uHeight;
            BitmapData srcData = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            uint[] totals = new uint[] { 0, 0, 0 };
            unsafe
            {
                uint* p = (uint*)(void*)srcData.Scan0;

                uint idx = 0;
                while (idx < (pixelCount & ~0xff))
                {
                    uint sumRR00BB = 0;
                    uint sum00GG00 = 0;
                    for (int j = 0; j < 0x100; j++)
                    {
                        sumRR00BB += p[idx] & 0xff00ff;
                        sum00GG00 += p[idx] & 0x00ff00;
                        idx++;
                    }

                    totals[0] += sumRR00BB >> 16;
                    totals[1] += sum00GG00 >> 8;
                    totals[2] += sumRR00BB & 0xffff;
                }

                // And the final partial block of fewer than 0x100 pixels.
                {
                    uint sumRR00BB = 0;
                    uint sum00GG00 = 0;
                    while (idx < pixelCount)
                    {
                        sumRR00BB += p[idx] & 0xff00ff;
                        sum00GG00 += p[idx] & 0x00ff00;
                        idx++;
                    }

                    totals[0] += sumRR00BB >> 16;
                    totals[1] += sum00GG00 >> 8;
                    totals[2] += sumRR00BB & 0xffff;
                }
            }

            uint avgB = totals[0] / (uWidth * uHeight);
            uint avgG = totals[1] / (uWidth * uHeight);
            uint avgR = totals[2] / (uWidth * uHeight);

            image.UnlockBits(srcData);

            return Color.FromArgb((int)avgR, (int)avgG, (int)avgB);
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
