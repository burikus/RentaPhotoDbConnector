using Microsoft.EntityFrameworkCore;
using RentaPhotoDbConnector.EFModels;

namespace RentaPhotoDbConnector.EFData
{
    public class DataContext : DbContext
    {
        public DbSet<ProductEntity> Goods { get; set; }
        public DbSet<OrderEntity> Orders { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ProductEntity>().HasKey(e => new { e.ProductArticle });
            builder.Entity<OrderEntity>().HasKey(e => new { e.OrderId });
            builder.Entity<GoodsEntity>().HasKey(e => new { e.ProductId });
        }
    }
}
