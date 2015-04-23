using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMosaic
{
    class ReferenceImagesParser
    {
        private string imagesPath;
        private string[] imagesNames;
        public IDictionary<string, Color> ColorSet { get; set; }

        public ReferenceImagesParser(string imagesPath)
        {
            this.imagesPath = imagesPath;
            imagesNames = Directory.GetFiles(imagesPath);//Loads the images name for future processing
            ColorSet = new Dictionary<string,Color>();
        }

        /// <summary>
        /// Parse all the images in the "imagesPath" field and gets the dominant Color
        /// </summary>
        public IDictionary<string, Color> ParseAllImages()
        {
            int counter = 1;

            Parallel.For(0, imagesNames.Length, i=>
            {
                Color color = _getImageColor(imagesNames[i]);
                if (color != Color.Transparent)
                {
                    ColorSet.Add(imagesNames[i], color);
                    imagesNames[i] = "";
                    Console.Clear();
                    Console.WriteLine("Images Processed: " + counter + " tiles of "+ imagesNames.Length);
                }
                
                counter++;
                if(counter % 100 == 0)//Force the garbage collector to dispose of unmanaged resources
                    System.GC.Collect();
            });
            System.GC.Collect();
            return ColorSet;
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
