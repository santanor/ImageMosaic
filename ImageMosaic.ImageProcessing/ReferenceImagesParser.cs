using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMosaic.ImageProcessing
{
    public class ReferenceImagesParser
    {
        private string imagesPath;
        private string[] imagesNames;
        private string[] imagesCache;
        public IDictionary<string, Color> ColorSet { get; set; }

        public ReferenceImagesParser(string imagesPath)
        {
            this.imagesPath = imagesPath;
            imagesNames = Directory.GetFiles(imagesPath);//Loads the images name for future processing
            imagesCache = File.ReadAllLines("ReferenceImages/Cache/cache.txt");
            ColorSet = new Dictionary<string,Color>();
        }

        /// <summary>
        /// Parse all the images in the "imagesPath" field and gets the dominant Color
        /// </summary>
        public IDictionary<string, Color> ParseAllImages()
        {
            if (imagesNames.Length-1 == imagesCache.Length)
                _recoverImagesFromCache();
            else
                _parseImages();
           
            return ColorSet;
        }

        /// <summary>
        /// Recover each image color from the local cache to avoid startup load
        /// </summary>
        private void _recoverImagesFromCache()
        {
            Console.WriteLine("Recovering the images from the cache");
            foreach (string image in imagesCache)
            {
                string[] imageParams = image.Split(',');
                Color c = Color.FromArgb(int.Parse(imageParams[1]), int.Parse(imageParams[2]), int.Parse(imageParams[3]));
                if(!ColorSet.ContainsKey(imageParams[0]))
                    ColorSet.Add(imageParams[0],c);
            }
        }

        /// <summary>
        /// Parse all the images and stores a local copy in the cache for future reading
        /// </summary>
        private void _parseImages()
        {
            Console.WriteLine("Cache failed to load. Preprocessing Images");
            int counter = 1;
            IList<string> imagesCache = new List<string>();
            Parallel.For(0, imagesNames.Length, i =>
            {
                Color color = _getImageColor(imagesNames[i]);
                if (color != Color.Transparent)
                {
                    ColorSet.Add(imagesNames[i], color);
                    Console.Clear();
                    Console.WriteLine("Images Processed: " + counter + " tiles of " + imagesNames.Length);
                    imagesCache.Add(imagesNames[i]+","+color.R+","+color.G+","+color.B);
                    imagesNames[i] = "";
                }

                counter++;
                if (counter % 50 == 0)//Force the garbage collector to dispose of unmanaged resources
                    System.GC.Collect();
            });
            System.GC.Collect();
            File.WriteAllLines("ReferenceImages/Cache/cache.txt", imagesCache.ToArray());
        }

        /// <summary>
        /// Process a single image and returns the Dominant Color
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private Color _getImageColor(string name)
        {
            try
            {
                ImageProcessor processor = new ImageProcessor(name);
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
