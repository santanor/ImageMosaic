using ImageMosaic.DatabaseWorkerService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMosaic.ImageDownloader
{
    public class Program
    {

        static void Main()
        {
            var downloader = new ImagesDownloader("C:\\Users\\jose_\\Downloads\\test\\images.csv");
            downloader.StartDownload();
        }

    }
}