using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMosaic.Domain.Model
{
    public class ImageBlob
    {
        public byte[] Image { get; set; }
        
        [Key, ForeignKey("ImageInfo")]
        public int ImageInfoId { get; set; }
        public virtual ImageInfo ImageInfo { get; set; }
    }
}
