using Microsoft.EntityFrameworkCore;
using StorageService.Model;

namespace StorageService
{
    public class AppDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);  // сохранение OrderItem при удалении Product для сохранения истории заказов

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // ограничение уникальности артикула товара
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Article)
                .IsUnique();

            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>();
        }
    }
}
