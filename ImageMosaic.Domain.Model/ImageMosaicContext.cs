using MySql.Data.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMosaic.Domain.Model
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class ImageMosaicContext : DbContext 
    {
        public DbSet<ImageBlob> ImageBlob { get; set; }
        public DbSet<ImageInfo> ImageInfo { get; set; }

        public ImageMosaicContext() :base("ImageMosaic")
        {
            this.Database.CreateIfNotExists();
        }
    }
}
