using Microsoft.EntityFrameworkCore;
using ShopDongY.Models;

namespace ShopDongY.Data
{
    public class ShopDongYContext : DbContext
    {
        public ShopDongYContext(DbContextOptions<ShopDongYContext> options): base(options) {}
        public DbSet<ProductModel> Products { get; set; }
        public DbSet<BrandModel> Brands { get; set; }
        public DbSet<OrderModel> Orders { get; set; }
        public DbSet<OrderDetailsModel> OrderDetails { get; set; }
        public DbSet<UserModel> Users { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<RoleModel> Roles { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔹 Product - Category (N:1)
            modelBuilder.Entity<ProductModel>()
                .HasOne(p => p.Categorys)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Product - Brand (N:1)
            modelBuilder.Entity<ProductModel>()
                .HasOne(p => p.Brands)
                .WithMany()
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Product: unique Code
            modelBuilder.Entity<ProductModel>()
                .HasIndex(p => p.Code)
                .IsUnique();

            // 🔹 User - Role (N:1)
            modelBuilder.Entity<UserModel>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Order - User (N:1)
            modelBuilder.Entity<OrderModel>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 OrderDetail - Order (N:1)
            modelBuilder.Entity<OrderDetailsModel>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔹 OrderDetail - Product (N:1)
            modelBuilder.Entity<OrderDetailsModel>()
                .HasOne(od => od.Product)
                .WithMany()
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }



    }
}
