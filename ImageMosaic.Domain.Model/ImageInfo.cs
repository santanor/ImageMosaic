using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMosaic.Domain.Model
{
    public class ImageInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Color Color { get; set; }
        
        public virtual ImageBlob ImageBlob { get; set; }
    }
}
