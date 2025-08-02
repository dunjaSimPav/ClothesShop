using ClothesShop.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ClothesShop.Models
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
            // Leaving empty on purpose
        }

        public DbSet<Article> Articles { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }

        public DbSet<UserProfile> Profiles { get; set; }

        public DbSet<ArticleGroup> ArticleGroups { get; set; }
        public DbSet<ArticleGroupItem> ArticleGroupItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ArticleGroupItem>()
                .HasIndex(x => x.Id);

            builder.Entity<ArticleGroupItem>()
                .HasOne(x => x.ArticleGroup)
                .WithMany(x => x.ArticleGroupItems)
                .HasForeignKey(x => x.ArticleGroupId);

            builder.Entity<ArticleGroupItem>()
                .HasOne(x => x.Article)
                .WithMany(x => x.ArticleGroupItems)
                .HasForeignKey(x => x.ArticleId);

            builder.Entity<ArticleGroup>()
                .HasMany(x => x.ArticleGroupItems)
                .WithOne(x => x.ArticleGroup)
                .HasForeignKey(x => x.ArticleGroupId);

            builder.Entity<Article>()
                .HasMany(x => x.ArticleGroupItems)
                .WithOne(x => x.Article)
                .HasForeignKey(x => x.ArticleId);

            builder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion(new EnumToStringConverter<PaymentStatus>());

            builder.Entity<OrderLine>()
                .HasIndex(x => x.OrderLineId);

            builder.Entity<OrderLine>()
                .HasOne(x => x.Article)
                .WithMany(x => x.OrderLines)
                .HasForeignKey(x => x.ArticleId);

            builder.Entity<OrderLine>()
                .HasOne(x => x.Order)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.OrderId);
        }
    }
}
