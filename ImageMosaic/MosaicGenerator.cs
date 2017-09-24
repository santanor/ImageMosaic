using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageMosaic.ImageProcessing;
using ImageMosaic.Domain.Model;
using System.Collections.Concurrent;

namespace ImageMosaic
{
    public class MosaicGenerator
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

        public MosaicGenerator(string inputImagePath, string outputImagePath)
        {
            this.inputImagePath = inputImagePath;
            this.outputImagePath = outputImagePath;
            this.context = new ImageMosaicContext();
        }

        public void GenerateImageMosaic(int tileSize, int dstImageMultiplicator)
        {
            if (OnGeneratorStarted != null)
                OnGeneratorStarted();


            ImageProcessor processor = new ImageProcessor(inputImagePath, tileSize);
            Color[,] imageColors = processor.GetColorMatrix();
            while (processor.image.Width*dstImageMultiplicator > 10000 ||
                   processor.image.Height*dstImageMultiplicator > 10000)
                dstImageMultiplicator--;

            var _totalTiles = imageColors.GetLength(0) * imageColors.GetLength(1);
            Bitmap exportImage = new Bitmap(processor.image.Width * dstImageMultiplicator, processor.image.Height * dstImageMultiplicator);
            Graphics graphics = Graphics.FromImage(exportImage);
            int tilesWidth = processor.image.Width / tileSize;
            int tilesHeight = processor.image.Height / tileSize;
            int tileSizeWidth = tileSize * dstImageMultiplicator;
            int tileSizeHeight = tileSize * dstImageMultiplicator;
            int tileCounter = 1;
            string[,] imagePaths = _getAllImageColors(imageColors);
            //var dic = GetCachedImages(imagePaths);
            for (int i = 0; i < tilesHeight; i++)
            {
                for (int j = 0; j < tilesWidth; j++)
                {
                    string imageName = imagePaths[j, i];
                    Image image =  Image.FromFile(imageName);
                    Rectangle srcRect = new Rectangle(0, 0, image.Width, image.Height);
                    Rectangle dstRect = new Rectangle(j*tileSizeWidth, i* tileSizeHeight, tileSizeWidth, tileSizeHeight);
                    graphics.DrawImage(image, dstRect, srcRect, GraphicsUnit.Pixel);                    
                    tileCounter++;          
                    if (OnTilePlaced != null && OnTilePlacedStream != null)
                    {
                        OnTilePlaced(tileCounter, _totalTiles);
                        var img = exportImage.Clone() as Image;
                        OnTilePlacedStream(img);
                    }
                    image.Dispose();
                }                
            }
            exportImage.Save(outputImagePath+".jpg", ImageFormat.Jpeg);

            if (OnGeneratorStop != null)
                OnGeneratorStop();
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
            for(int i = 0; i < allImageColors.GetLength(0); i++)
            {
                for (int j = 0; j < allImageColors.GetLength(1); j++)
                {
                    var argbColor = colors[i, j].ToArgb();
                    allImageColors[i, j] = imagesCache.OrderBy(x => Math.Abs(Math.Abs(argbColor) - Math.Abs(x.ArgbColor))).FirstOrDefault()?.ImagePath;
                }
            }

            return allImageColors;
        }

        private ConcurrentDictionary<string, Image> GetCachedImages(string[,] paths)
        {
            var files = new List<string>();
            for (int i = 0; i < paths.GetLength(0); i++)
            {
                for (int j = 0; j < paths.GetLength(1); j++)
                {
                    files.Add(paths[i, j]);
                }
            }
            var dict = new ConcurrentDictionary<string, Image>();
            Parallel.ForEach(files.Distinct(), row =>
            {
                dict.TryAdd(row, Image.FromFile(row));
            });

            return dict;
        }
    }
}
