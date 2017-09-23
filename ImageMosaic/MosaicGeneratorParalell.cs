using ImageMosaic.Domain.Model;
using ImageMosaic.ImageProcessing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageMosaic
{
    public class MosaicGeneratorParalell
    {
        public delegate void TilePlacedEvent(int tileCount, int totalTiles);

        public TilePlacedEvent OnTilePlaced { get; set; }
        private string inputImagePath;
        private string outputImagePath;
        private Object thisLock = new Object();
        private ImageMosaicContext context;
        private int _maxImagesCached;
        private ConcurrentQueue<ConcurrentImageTileModel> imagesCached;
        private int _totalTiles;
        private int _currentTileCount;
        private bool loadingFinished;

        public MosaicGeneratorParalell(string inputImagePath, string outputImagePath, int maxImagesCached)
        {
            this.inputImagePath = inputImagePath;
            this.outputImagePath = outputImagePath;
            this.context = new ImageMosaicContext();
            this._maxImagesCached = maxImagesCached;
            this.imagesCached = new ConcurrentQueue<ConcurrentImageTileModel>();
        }

        public void GenerateImageMosaic(int tileSize, int dstImageMultiplicator)
        {
            ImageProcessor processor = new ImageProcessor(inputImagePath, tileSize);
            Color[,] imageColors = processor.GetColorMatrix();
            while (processor.image.Width * dstImageMultiplicator > 10000 ||
                   processor.image.Height * dstImageMultiplicator > 10000)
                dstImageMultiplicator--;

            this._totalTiles = imageColors.GetLength(0) * imageColors.GetLength(1);


            Thread thread = new Thread(() => _generateExportImage(tileSize, dstImageMultiplicator));
            thread.Start();

            StartLoadImages(processor, tileSize, dstImageMultiplicator, imageColors);
            loadingFinished = true;
        }

        private void _generateExportImage(int tileSize, int dstImageMultiplicator)
        {
            ImageProcessor processor = new ImageProcessor(inputImagePath, tileSize);
            Bitmap exportImage = new Bitmap(processor.image.Width * dstImageMultiplicator, processor.image.Height * dstImageMultiplicator);
            Graphics graphics = Graphics.FromImage(exportImage);
            int tilesProcessed = 1;
            while (!loadingFinished || imagesCached.Count > 0)
            {
                ConcurrentImageTileModel image = null;
                if (imagesCached.TryDequeue(out image))
                {
                    graphics.DrawImage(image.Image, image.DstRectangle, image.SrcRectangle, GraphicsUnit.Pixel);
                    tilesProcessed++;
                    image.Image.Dispose();
                    image = null;
                    if(tilesProcessed%100 == 0 && OnTilePlaced != null)
                    {
                        OnTilePlaced(tilesProcessed, _totalTiles);
                    }
                }

            }

            exportImage.Save(outputImagePath + ".jpg", ImageFormat.Jpeg);
        }

        /// <summary>
        /// returns the name of the image that matches with the given color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private string _getTileImageName(Color color)
        {
            var argbColor = color.ToArgb();
            var closestMatch = context.ImageInfo.OrderBy(x => Math.Abs(Math.Abs(argbColor) - Math.Abs(x.ArgbColor))).FirstOrDefault();
            return closestMatch.ImagePath;
        }

        private string[,] _getAllImageColors(Color[,] colors)
        {
            var allImageColors = new string[colors.GetLength(0), colors.GetLength(1)];
            var imagesCache = context.ImageInfo.Select(x => new { x.ArgbColor, x.ImagePath }).ToList();
            for (int i = 0; i < allImageColors.GetLength(0); i++)
            {
                Parallel.For(0, allImageColors.GetLength(1), j =>
                {
                    var argbColor = colors[i, j].ToArgb();
                    allImageColors[i, j] = imagesCache.OrderBy(x => Math.Abs(Math.Abs(argbColor) - Math.Abs(x.ArgbColor))).FirstOrDefault()?.ImagePath;
                });
            }

            return allImageColors;
        }

        private void StartLoadImages(ImageProcessor processor, int tileSize, int dstImageMultiplicator, Color[,] colors)
        {
            int tilesWidth = processor.image.Width / tileSize;
            int tilesHeight = processor.image.Height / tileSize;
            int tileSizeWidth = tileSize * dstImageMultiplicator;
            int tileSizeHeight = tileSize * dstImageMultiplicator;
            var imagePaths = this._getAllImageColors(colors);
            var memorySize = Process.GetCurrentProcess();
            memorySize.MaxWorkingSet = new IntPtr(2000000000);            
            for (int i = 0; i < tilesWidth; i++)
            {
                Parallel.For(0, tilesHeight, j => {
                    var currentSuccess = false;
                    while (!currentSuccess)
                    {
                        try
                        { 
                            if (i < tilesHeight && j < tilesWidth)
                            {
                                var fileName = imagePaths[i, j];
                                var file = new FileInfo(fileName);                                
                                while (IsFileLocked(file) || this.imagesCached.Count >= this._maxImagesCached) {}
                                Image image = Image.FromFile(fileName);
                                Rectangle srcRect = new Rectangle(0, 0, image.Width, image.Height);
                                Rectangle dstRect = new Rectangle(j * tileSizeWidth, i * tileSizeHeight, tileSizeWidth, tileSizeHeight);
                                this.imagesCached.Enqueue(new ConcurrentImageTileModel
                                {
                                    SrcRectangle = srcRect,
                                    DstRectangle = dstRect,
                                    Image = image
                                });

                            }
                            _currentTileCount++;
                            currentSuccess = true;
                        }catch (OutOfMemoryException e)
                        {
                            currentSuccess = false;
                        }
                }

                });
            }
        }

        private bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
    }
}

