using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMosaic
{
    public class ConcurrentImageTileModel
    {
        public Image Image { get; set; }
        public Rectangle SrcRectangle { get; set; }
        public Rectangle DstRectangle { get; set; }
    }
}
