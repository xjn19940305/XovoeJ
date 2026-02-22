using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using XovoeJ.Entities;

namespace XovoeJ.Persistence.PostgreSql
{
    public class XovoeJDbContext : IdentityDbContext<User, Role, string, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductSku> ProductSkus { get; set; }
        public DbSet<SpecGroup> SpecGroups { get; set; }
        public DbSet<SpecValue> SpecValues { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }


        public XovoeJDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User 索引
            modelBuilder.Entity<User>(build =>
            {
                build.HasIndex(u => u.PhoneNumber);
            });

            // Category 配置
            modelBuilder.Entity<Category>(build =>
            {
                build.HasIndex(c => c.ParentId);
                build.HasIndex(c => c.Path);
            });

            // Product 配置
            modelBuilder.Entity<Product>(build =>
            {
                build.HasIndex(p => p.CategoryId);
                build.HasIndex(p => p.IsEnabled);
                build.HasIndex(p => p.IsHot);
                build.HasIndex(p => p.IsNew);
                build.HasIndex(p => p.IsRecommend);
                build.HasIndex(p => p.SalesCount);
            });

            // ProductSku 配置
            modelBuilder.Entity<ProductSku>(build =>
            {
                build.HasIndex(s => s.ProductId);
                build.HasIndex(s => s.SkuCode).IsUnique();
                build.HasIndex(s => s.Stock);
            });

            // SpecGroup 配置
            modelBuilder.Entity<SpecGroup>(build =>
            {
                build.HasIndex(s => s.SortOrder);
            });

            // SpecValue 配置
            modelBuilder.Entity<SpecValue>(build =>
            {
                build.HasIndex(s => s.SpecGroupId);
                build.HasIndex(s => s.SortOrder);
            });

            // ShoppingCart 配置
            modelBuilder.Entity<ShoppingCart>(build =>
            {
                build.HasIndex(c => c.UserId);
                build.HasIndex(c => c.ProductId);
                build.HasIndex(c => c.SkuId);
                build.HasIndex(c => new { c.UserId, c.SkuId }).IsUnique();
            });

            // Order 配置
            modelBuilder.Entity<Order>(build =>
            {
                build.HasIndex(o => o.UserId);
                build.HasIndex(o => o.OrderNo).IsUnique();
                build.HasIndex(o => o.Status);
                build.HasIndex(o => o.CreatedAt);
            });

            // OrderItem 配置
            modelBuilder.Entity<OrderItem>(build =>
            {
                build.HasIndex(o => o.OrderId);
                build.HasIndex(o => o.ProductId);
            });

        }
    }
}
