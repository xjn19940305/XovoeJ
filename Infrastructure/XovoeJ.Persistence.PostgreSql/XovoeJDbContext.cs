using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using XovoeJ.Entities;

namespace XovoeJ.Persistence.PostgreSql
{
    public class XovoeJDbContext : IdentityDbContext<User, Role, string, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductSku> ProductSkus { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<DictionaryGroup> DictionaryGroups { get; set; }
        public DbSet<DictionaryItem> DictionaryItems { get; set; }
        public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; }
        public DbSet<WorkflowInstance> WorkflowInstances { get; set; }
        public DbSet<WorkflowPendingItem> WorkflowPendingItems { get; set; }
        public DbSet<WorkflowApprovalRecord> WorkflowApprovalRecords { get; set; }

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

            // DictionaryGroup 配置
            modelBuilder.Entity<DictionaryGroup>(build =>
            {
                build.HasIndex(g => g.Code).IsUnique();
                build.HasIndex(g => g.ParentId);
                build.HasIndex(g => g.Path);
                build.HasIndex(g => g.Type);
            });

            // DictionaryItem 配置
            modelBuilder.Entity<DictionaryItem>(build =>
            {
                build.HasIndex(i => i.GroupId);
                build.HasIndex(i => i.Key).IsUnique();
                build.HasIndex(i => i.IsEnabled);
            });

            // WorkflowDefinition 配置
            modelBuilder.Entity<WorkflowDefinition>(build =>
            {
                build.HasIndex(d => d.Code).IsUnique();
                build.HasIndex(d => d.Type);
                build.HasIndex(d => d.IsEnabled);
            });

            // WorkflowInstance 配置
            modelBuilder.Entity<WorkflowInstance>(build =>
            {
                build.HasIndex(i => i.WorkflowCode);
                build.HasIndex(i => i.InitiatorId);
                build.HasIndex(i => i.BusinessKey);
                build.HasIndex(i => i.Status);
                build.HasIndex(i => i.CreatedAt);
            });

            // WorkflowPendingItem 配置
            modelBuilder.Entity<WorkflowPendingItem>(build =>
            {
                build.HasIndex(p => p.InstanceId);
                build.HasIndex(p => p.ApproverId);
                build.HasIndex(p => new { p.InstanceId, p.ApproverId });
            });

            // WorkflowApprovalRecord 配置
            modelBuilder.Entity<WorkflowApprovalRecord>(build =>
            {
                build.HasIndex(r => r.InstanceId);
                build.HasIndex(r => r.StepId);
                build.HasIndex(r => r.ApproverId);
                build.HasIndex(r => r.ActionTime);
            });
        }
    }
}
