using System;
using System.Collections.Generic;
using System.Drawing;

namespace ImageMosaic
{
    public class ConcurrentImageTileModel : IDisposable
    {
        public IList<ImageRect> Rectangles = new List<ImageRect>();
        public Image Image {get; set;}
        public string ImageName {get; set;}

        public void Dispose()
        {
            Image.Dispose();
            GC.SuppressFinalize(this);
        }

        public struct ImageRect
        {
            public Rectangle SrcRectangle {get; set;}
            public Rectangle DstRectangle {get; set;}
        }
    }
}