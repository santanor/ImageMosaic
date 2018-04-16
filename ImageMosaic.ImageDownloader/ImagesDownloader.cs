using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;

namespace ImageMosaic.DatabaseWorkerService
{
    public class ImagesDownloader
    {
        private readonly string path;
        private string[] lines;

        public ImagesDownloader(string path)
        {
            this.path = path;
            lines = File.ReadAllLines(path);
        }

        public void StartDownload()
        {
            try
            {
                foreach (var x in lines)
                {
                    var url = x.Split(',')[2];
                    SaveImage(url);
                }

                ;
                lines = null;
                File.Delete(path);
            }
            catch (Exception)
            {
            }
        }

        private void SaveImage(string imageUrl)
        {
            try
            {
                var client = new WebClient();
                var stream = client.OpenRead(imageUrl);
                var bitmap = new Bitmap(stream ?? throw new InvalidOperationException());

                bitmap?.Save($"ReferenceImages\\{imageUrl.Split('/')[4]}.png", ImageFormat.Png);

                stream.Flush();
                stream.Close();
                client.Dispose();
            }
            catch (Exception e)
            {
                // ignored
            }
        }
    }
}
