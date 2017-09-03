using ImageMosaic.ImageProcessing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMosaic
{
    class Program
    {
        static void Main(string[] args)
        {

            while (true)
            {
                Console.WriteLine("Write the name path of the source image and the name of the output image and the tilesize");
                string[] line = Console.ReadLine().Split(' ');
                if (line.Length == 3)
                {
                    try
                    {
                        generateMosaic(line[0], line[1], line[2]);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.Message);
                    }
                }

            }
        }


        private static void generateMosaic(string sourceImg, string dstImg, string tileSize)
        {
            //ImageProcessor processor = new ImageProcessor(sourceImg, Int32.Parse(tileSize));
            //processor.SaveHamaTemplate(dstImg);
            MosaicGenerator generator = new MosaicGenerator(sourceImg, dstImg);
            generator.GenerateImageMosaic(int.Parse(tileSize), 1);
        }
    }
}
