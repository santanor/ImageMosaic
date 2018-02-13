using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMosaic
{
    public class ConcurrentImageTileModel : System.IDisposable
    {
        public Image Image { get; set; }
        public string ImageName { get; set; }
        public IList<ImageRect> Rectangles = new List<ImageRect>();

        public void Dispose()
        {
            this.Image.Dispose();
            System.GC.SuppressFinalize(this);
        }

        public struct ImageRect
        {
            public Rectangle SrcRectangle { get; set; }
            public Rectangle DstRectangle { get; set; }
        }
    }
}
