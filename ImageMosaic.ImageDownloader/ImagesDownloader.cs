using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ImageMosaic.DatabaseWorkerService
{
    public class ImagesDownloader
    {
        private string path;
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
                };
                lines = null;
                File.Delete(path);
            }
            catch (Exception) { }
        }

        private void SaveImage(string imageUrl)
        {
            WebClient client = new WebClient();
            Stream stream = client.OpenRead(imageUrl);
            Bitmap bitmap; bitmap = new Bitmap(stream);

            if (bitmap != null)
                bitmap.Save($"D:\\ReferenceImages\\{imageUrl.Split('/')[4]}.png", ImageFormat.Png);

            stream.Flush();
            stream.Close();
            client.Dispose();
        }


    }
}
