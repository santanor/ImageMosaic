using ImageMosaic.DatabaseWorkerService;

namespace ImageMosaic.ImageDownloader
{
    public class Program
    {
        private static void Main()
        {
            var downloader = new ImagesDownloader("C:\\Users\\jose_\\Downloads\\test\\images.csv");
            downloader.StartDownload();
        }
    }
}