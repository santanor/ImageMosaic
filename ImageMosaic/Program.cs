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
            MosaicGenerator generator = new MosaicGenerator("Test2.jpg", "TestMosaic.png", imageParser);
            generator.GenerateImageMosaic(10);
        }
    }
}
