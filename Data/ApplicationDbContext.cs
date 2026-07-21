using Microsoft.EntityFrameworkCore;
using BRT.Models;

namespace BRT.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<SubCategory> SubCategories => Set<SubCategory>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<PackingType> PackingTypes => Set<PackingType>();

        public DbSet<MarketPrice> MarketPrices => Set<MarketPrice>();
        public DbSet<MarketHighlight> MarketHighlights => Set<MarketHighlight>();

        public DbSet<OrderRequest> OrderRequests => Set<OrderRequest>();
        public DbSet<OrderRequestItem> OrderRequestItems => Set<OrderRequestItem>();
        public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

        public DbSet<Banner> Banners => Set<Banner>();
        public DbSet<Testimonial> Testimonials => Set<Testimonial>();
        public DbSet<FAQ> FAQs => Set<FAQ>();
        public DbSet<GalleryImage> GalleryImages => Set<GalleryImage>();
        public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
        public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique constraints
            modelBuilder.Entity<AdminUser>()
                .HasIndex(a => a.Email).IsUnique();

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Slug).IsUnique();

            modelBuilder.Entity<SubCategory>()
                .HasIndex(s => s.Slug).IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Slug).IsUnique();

            modelBuilder.Entity<OrderRequest>()
                .HasIndex(o => o.OrderNumber).IsUnique();

            modelBuilder.Entity<SiteSetting>()
                .HasIndex(s => s.Key).IsUnique();

            // Composite index: price lookups per product per day
            modelBuilder.Entity<MarketPrice>()
                .HasIndex(m => new { m.ProductId, m.PriceDate });

            modelBuilder.Entity<OrderRequest>()
                .HasIndex(o => o.Status);

            // Relationships / delete behavior
            modelBuilder.Entity<SubCategory>()
                .HasOne(s => s.Category)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.SubCategory)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PackingType>()
                .HasOne(pt => pt.Product)
                .WithMany(p => p.PackingTypes)
                .HasForeignKey(pt => pt.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MarketPrice>()
                .HasOne(m => m.Product)
                .WithMany(p => p.MarketPrices)
                .HasForeignKey(m => m.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MarketPrice>()
                .HasOne(m => m.PackingType)
                .WithMany()
                .HasForeignKey(m => m.PackingTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderRequestItem>()
                .HasOne(i => i.OrderRequest)
                .WithMany(o => o.Items)
                .HasForeignKey(i => i.OrderRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderRequestItem>()
                .HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderStatusHistory>()
                .HasOne(h => h.OrderRequest)
                .WithMany(o => o.StatusHistory)
                .HasForeignKey(h => h.OrderRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // Global soft-delete filters
            modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<SubCategory>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<PackingType>().HasQueryFilter(pt => !pt.IsDeleted);
        }
    }
}
