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
            ReferenceImagesParser imageParser = new ReferenceImagesParser("ReferenceImages");
            imageParser.ParseAllImages();

            while (true)
            {
                Console.WriteLine("Write the name path of the source image and the name of the output image and the tilesize");
                string[] line = Console.ReadLine().Split(' ');
                if (line.Length == 3)
                {
                    try
                    {
                        generateMosaic(imageParser, line[0], line[1], line[2]);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.Message);
                    }
                }

            }
        }

        private static void generateMosaic(ReferenceImagesParser imageParser, string sourceImg, string dstImg, string tileSize)
        {
            MosaicGenerator generator = new MosaicGenerator(sourceImg, dstImg, imageParser);
            generator.GenerateImageMosaic(int.Parse(tileSize), 10);
        }
    }
}
