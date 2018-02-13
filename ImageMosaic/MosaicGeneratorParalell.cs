 using ImageMosaic.Domain.Model;
using ImageMosaic.ImageProcessing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        public delegate void TilePlacedStreamEvent(Image bitmap);
        public delegate void GeneratorStart();
        public delegate void GeneratorStop();

        public TilePlacedEvent OnTilePlaced { get; set; }
        public TilePlacedStreamEvent OnTilePlacedStream { get; set; }
        public GeneratorStart OnGeneratorStarted { get; set; }
        public GeneratorStop OnGeneratorStop { get; set; }
        private string inputImagePath;
        private string outputImagePath;
        private Object thisLock = new Object();
        private ImageMosaicContext context;
        private int _maxImagesCached;
        private IList<ConcurrentImageTileModel> imagesCached;
        private int _totalTiles;
        private int _currentTileCount;
        private bool loadingFinished;

        public MosaicGeneratorParalell(string inputImagePath, string outputImagePath, int maxImagesCached)
        {
            this.inputImagePath = inputImagePath;
            this.outputImagePath = outputImagePath;
            this.context = new ImageMosaicContext();
            this._maxImagesCached = maxImagesCached;
            this.imagesCached = new List<ConcurrentImageTileModel>();
        }

        public void GenerateImageMosaic(int tileSize, int dstImageMultiplicator)
        {
            if (OnGeneratorStarted != null)
                OnGeneratorStarted();

            ImageProcessor processor = new ImageProcessor(inputImagePath, tileSize);
            Color[,] imageColors = processor.GetColorMatrix();
            while (processor.image.Width * dstImageMultiplicator > 10000 ||
                   processor.image.Height * dstImageMultiplicator > 10000)
                dstImageMultiplicator--;

            this._totalTiles = imageColors.GetLength(0) * imageColors.GetLength(1);


            //Thread thread = new Thread(() => _generateExportImage(tileSize, dstImageMultiplicator));
            //thread.Start();

            StartLoadImages(processor, tileSize, dstImageMultiplicator, imageColors);
            _generateExportImage(tileSize, dstImageMultiplicator);
            loadingFinished = true;

            if (OnGeneratorStop != null)
                OnGeneratorStop();
        }

        private void _generateExportImage(int tileSize, int dstImageMultiplicator)
        {
            ImageProcessor processor = new ImageProcessor(inputImagePath, tileSize);
            int[,] exportImage = new int[processor.image.Width * dstImageMultiplicator, processor.image.Height * dstImageMultiplicator];
            //Graphics graphics = Graphics.FromImage(exportImage);
            int tilesProcessed = 1;
            foreach(var image in imagesCached)
            {
                var resizedImage = ResizeImage(image.Image, tileSize * dstImageMultiplicator, tileSize * dstImageMultiplicator);
                var imgColorsMatrix = new ImageProcessor(resizedImage as Bitmap, 1).GetArgbMatrix();
                foreach(var rect in image.Rectangles)
                {
                    CopyArgbImageToDst(ref imgColorsMatrix, ref exportImage, rect.DstRectangle);
                    tilesProcessed++;                    
                    if (tilesProcessed % 1000 == 0 && OnTilePlaced != null && OnTilePlacedStream != null)
                    {
                        OnTilePlaced(tilesProcessed, _totalTiles);
                        var img = GetBitmapFromArgbMatrix(ref exportImage);
                        OnTilePlacedStream(img);
                    }
                }
                image.Dispose();                
            }
            var btmp = GetBitmapFromArgbMatrix(ref exportImage);
            btmp.Save(outputImagePath + ".jpg", ImageFormat.Jpeg);
        }

        /// <summary>
        /// returns the name of the image that matches with the given color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>s
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
            for (int i = 0; i < tilesWidth; i++)
            {
                for(int j = 0; j < tilesHeight; j++)
                {
                    if (i < tilesHeight && j < tilesWidth)
                    {
                        var fileName = imagePaths[i, j];
                        //Check if the image exists in the collection already, if it does then just add the rect to the collection
                        var imgExists = imagesCached.FirstOrDefault(x => x.ImageName == fileName);
                        if(imgExists != null)
                        {
                            Rectangle srcRect = new Rectangle(0, 0, imgExists.Image.Width, imgExists.Image.Height);
                            Rectangle dstRect = new Rectangle(j * tileSizeWidth, i * tileSizeHeight, tileSizeWidth, tileSizeHeight);
                            imgExists.Rectangles.Add(new ConcurrentImageTileModel.ImageRect { SrcRectangle = srcRect, DstRectangle = dstRect });
                        }
                        else
                        {
                            var file = new FileInfo(fileName);
                            //while (IsFileLocked(file) || this.imagesCached.Count >= this._maxImagesCached) { }
                            var image = Image.FromFile(fileName);
                            Rectangle srcRect = new Rectangle(0, 0, image.Width, image.Height);
                            Rectangle dstRect = new Rectangle(j * tileSizeWidth, i * tileSizeHeight, tileSizeWidth, tileSizeHeight);
                            imagesCached.Add(new ConcurrentImageTileModel
                            {
                                Rectangles = new List<ConcurrentImageTileModel.ImageRect>(new []{ new ConcurrentImageTileModel.ImageRect { SrcRectangle = srcRect, DstRectangle = dstRect } }),
                                Image = image,
                                ImageName = fileName
                            });
                            

                        }
                    }
                    _currentTileCount++; 
                }
            }
        }

       
        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Image ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        private void CopyArgbImageToDst(ref int[,] srcArgb, ref int[,] dstArgb, Rectangle dstRect)
        {
            for(int i = 0; i< srcArgb.GetLength(0); i++)
            {
                for(int j = 0; j < srcArgb.GetLength(1); j++)
                {
                    dstArgb[i + dstRect.X, j + dstRect.Y] = srcArgb[i, j];
                }
            }
        }

        private Bitmap GetBitmapFromArgbMatrix(ref int[,] argbMatrix)
        {
            Bitmap bitmap;
            unsafe
            {
                fixed (int* intPtr = &argbMatrix[0, 0])
                {
                    bitmap = new Bitmap(argbMatrix.GetLength(0), argbMatrix.GetLength(1), argbMatrix.GetLength(0)*4, PixelFormat.Format16bppArgb1555, new IntPtr(intPtr));
                }
            }
            return bitmap;
        }
        
    }
}

