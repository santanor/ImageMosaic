using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMosaic.Domain.Model
{
    public class ImageBlob
    {
        public int Id { get; set; }
        public byte[] Image { get; set; }
        public int ImageInfoId { get; set; }
        public virtual ImageInfo ImageInfo { get; set; }
    }
}
