using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMosaic.Domain.Model
{
    public class ImageInfo
    {
        public int Id { get; set; }
        public Color Color { get; set; }
        public int ImageBlobId { get; set; }
        public ImageBlob ImageBlog { get; set; }
    }
}
