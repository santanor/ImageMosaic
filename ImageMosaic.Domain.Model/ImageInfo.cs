using System.ComponentModel.DataAnnotations.Schema;

namespace ImageMosaic.Domain.Model
{
    public class ImageInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id {get; set;}

        public int ArgbColor {get; set;}
        public string ImagePath {get; set;}
    }
}