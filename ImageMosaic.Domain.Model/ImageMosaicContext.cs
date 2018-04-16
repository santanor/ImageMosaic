using System.Data.Entity;
using MySql.Data.Entity;

namespace ImageMosaic.Domain.Model
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class ImageMosaicContext : DbContext
    {
        public ImageMosaicContext() : base("ImageMosaic")
        {
            Database.CreateIfNotExists();
        }

        public DbSet<ImageInfo> ImageInfo {get; set;}
    }
}