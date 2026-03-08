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
        public DbSet<UserAddress> UserAddresses { get; set; }
        public DbSet<WalletAccount> WalletAccounts { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<PointsAccount> PointsAccounts { get; set; }
        public DbSet<PointsLog> PointsLogs { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<PaymentOrder> PaymentOrders { get; set; }
        public DbSet<AfterSaleOrder> AfterSaleOrders { get; set; }
        public DbSet<DictionaryGroup> DictionaryGroups { get; set; }
        public DbSet<DictionaryItem> DictionaryItems { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; }
        public DbSet<WorkflowInstance> WorkflowInstances { get; set; }
        public DbSet<WorkflowPendingItem> WorkflowPendingItems { get; set; }
        public DbSet<WorkflowApprovalRecord> WorkflowApprovalRecords { get; set; }
        public DbSet<MessageTemplate> MessageTemplates { get; set; }
        public DbSet<MessageTask> MessageTasks { get; set; }
        public DbSet<MessageSendRecord> MessageSendRecords { get; set; }
        public DbSet<InviteRelation> InviteRelations { get; set; }
        public DbSet<ReferralLink> ReferralLinks { get; set; }
        public DbSet<CommissionRecord> CommissionRecords { get; set; }
        public DbSet<CouponTemplate> CouponTemplates { get; set; }
        public DbSet<UserCoupon> UserCoupons { get; set; }
        public DbSet<CouponIssueBatch> CouponIssueBatches { get; set; }
        public DbSet<CouponIssueRecord> CouponIssueRecords { get; set; }
        public DbSet<MemberLevelRewardRule> MemberLevelRewardRules { get; set; }
        public DbSet<PromotionActivity> PromotionActivities { get; set; }
        public DbSet<SeckillActivity> SeckillActivities { get; set; }
        public DbSet<GroupBuyActivity> GroupBuyActivities { get; set; }
        public DbSet<BargainActivity> BargainActivities { get; set; }

        public XovoeJDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
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

            modelBuilder.Entity<UserAddress>(build =>
            {
                build.HasIndex(a => a.UserId);
                build.HasIndex(a => new { a.UserId, a.IsDefault });
                build.HasIndex(a => a.RegionCode);
                build.HasIndex(a => a.CreatedAt);
            });

            modelBuilder.Entity<WalletAccount>(build =>
            {
                build.HasIndex(a => a.UserId).IsUnique();
                build.HasIndex(a => a.LastChangedAt);
                build.HasIndex(a => a.CreatedAt);
            });

            modelBuilder.Entity<WalletTransaction>(build =>
            {
                build.HasIndex(t => t.WalletAccountId);
                build.HasIndex(t => t.UserId);
                build.HasIndex(t => t.BusinessType);
                build.HasIndex(t => t.BusinessNo);
                build.HasIndex(t => t.CreatedAt);
                build.HasIndex(t => t.IdempotencyKey).IsUnique();
            });

            modelBuilder.Entity<PointsAccount>(build =>
            {
                build.HasIndex(a => a.UserId).IsUnique();
                build.HasIndex(a => a.LastChangedAt);
                build.HasIndex(a => a.CreatedAt);
            });

            modelBuilder.Entity<PointsLog>(build =>
            {
                build.HasIndex(t => t.PointsAccountId);
                build.HasIndex(t => t.UserId);
                build.HasIndex(t => t.BusinessType);
                build.HasIndex(t => t.BusinessNo);
                build.HasIndex(t => t.CreatedAt);
                build.HasIndex(t => t.IdempotencyKey).IsUnique();
            });

            // Order 配置
            modelBuilder.Entity<Order>(build =>
            {
                build.HasIndex(o => o.UserId);
                build.HasIndex(o => o.OrderNo).IsUnique();
                build.HasIndex(o => o.PaymentOrderNo);
                build.HasIndex(o => o.Status);
                build.HasIndex(o => o.CreatedAt);
                build.HasIndex(o => o.IsDeleted);
                build.HasQueryFilter(o => !o.IsDeleted);
            });

            // OrderItem 配置
            modelBuilder.Entity<OrderItem>(build =>
            {
                build.HasIndex(o => o.OrderId);
                build.HasIndex(o => o.ProductId);
            });

            modelBuilder.Entity<PaymentOrder>(build =>
            {
                build.HasIndex(o => o.PaymentOrderNo).IsUnique();
                build.HasIndex(o => o.OrderId).IsUnique();
                build.HasIndex(o => o.OrderNo);
                build.HasIndex(o => o.UserId);
                build.HasIndex(o => o.Status);
                build.HasIndex(o => o.CreatedAt);
                build.HasIndex(o => o.ExpireAt);
            });

            modelBuilder.Entity<AfterSaleOrder>(build =>
            {
                build.HasIndex(a => a.AfterSaleNo).IsUnique();
                build.HasIndex(a => a.OrderId);
                build.HasIndex(a => a.OrderNo);
                build.HasIndex(a => a.UserId);
                build.HasIndex(a => a.Status);
                build.HasIndex(a => a.Type);
                build.HasIndex(a => a.CreatedAt);
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
                build.HasIndex(i => new { i.GroupId, i.Key }).IsUnique();
                build.HasIndex(i => i.IsEnabled);
            });

            modelBuilder.Entity<Banner>(build =>
            {
                build.HasIndex(i => i.IsEnabled);
                build.HasIndex(i => i.SortOrder);
                build.HasIndex(i => i.CreatedAt);
                build.HasIndex(i => i.StartTime);
                build.HasIndex(i => i.EndTime);
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

            modelBuilder.Entity<MessageTemplate>(build =>
            {
                build.HasIndex(t => t.Code).IsUnique();
                build.HasIndex(t => t.Channel);
                build.HasIndex(t => t.Status);
                build.HasIndex(t => t.CreatedAt);
            });

            modelBuilder.Entity<MessageTask>(build =>
            {
                build.HasIndex(t => t.TemplateId);
                build.HasIndex(t => t.Channel);
                build.HasIndex(t => t.Status);
                build.HasIndex(t => t.CreatedAt);
                build.HasIndex(t => t.ScheduledAt);
            });

            modelBuilder.Entity<MessageSendRecord>(build =>
            {
                build.HasIndex(r => r.TemplateId);
                build.HasIndex(r => r.TaskId);
                build.HasIndex(r => r.Channel);
                build.HasIndex(r => r.Status);
                build.HasIndex(r => r.TraceId);
                build.HasIndex(r => r.CreatedAt);
                build.HasIndex(r => r.SentAt);
            });

            modelBuilder.Entity<InviteRelation>(build =>
            {
                build.HasIndex(r => r.InviterId);
                build.HasIndex(r => r.InviteeId);
                build.HasIndex(r => r.ReferralCode);
                build.HasIndex(r => r.Channel);
                build.HasIndex(r => r.Status);
                build.HasIndex(r => r.CreatedAt);
            });

            modelBuilder.Entity<ReferralLink>(build =>
            {
                build.HasIndex(r => r.Code).IsUnique();
                build.HasIndex(r => r.OwnerId);
                build.HasIndex(r => r.Channel);
                build.HasIndex(r => r.Status);
                build.HasIndex(r => r.CampaignName);
                build.HasIndex(r => r.CreatedAt);
                build.HasIndex(r => r.ExpireAt);
            });

            modelBuilder.Entity<CommissionRecord>(build =>
            {
                build.HasIndex(r => r.PromoterId);
                build.HasIndex(r => r.OrderNo);
                build.HasIndex(r => r.Status);
                build.HasIndex(r => r.CreatedAt);
                build.HasIndex(r => r.SettledAt);
            });

            modelBuilder.Entity<CouponTemplate>(build =>
            {
                build.HasIndex(t => t.Code).IsUnique();
                build.HasIndex(t => t.CouponType);
                build.HasIndex(t => t.Status);
                build.HasIndex(t => t.CreatedAt);
                build.HasIndex(t => t.StartTime);
                build.HasIndex(t => t.EndTime);
            });

            modelBuilder.Entity<UserCoupon>(build =>
            {
                build.HasIndex(t => t.UserId);
                build.HasIndex(t => t.CouponTemplateId);
                build.HasIndex(t => t.Status);
                build.HasIndex(t => new { t.UserId, t.Status });
                build.HasIndex(t => t.OrderId);
                build.HasIndex(t => t.CreatedAt);
            });

            modelBuilder.Entity<CouponIssueBatch>(build =>
            {
                build.HasIndex(t => t.TargetType);
                build.HasIndex(t => t.Status);
                build.HasIndex(t => t.CreatedAt);
            });

            modelBuilder.Entity<CouponIssueRecord>(build =>
            {
                build.HasIndex(t => t.BatchId);
                build.HasIndex(t => t.UserId);
                build.HasIndex(t => t.CouponTemplateId);
                build.HasIndex(t => t.UserCouponId);
                build.HasIndex(t => t.CreatedAt);
            });

            modelBuilder.Entity<MemberLevelRewardRule>(build =>
            {
                build.HasIndex(t => t.LevelCode).IsUnique();
                build.HasIndex(t => t.Status);
                build.HasIndex(t => t.Sort);
            });

            modelBuilder.Entity<PromotionActivity>(build =>
            {
                build.HasIndex(t => t.Type);
                build.HasIndex(t => t.Priority);
                build.HasIndex(t => t.Status);
                build.HasIndex(t => t.CreatedAt);
                build.HasIndex(t => t.StartTime);
                build.HasIndex(t => t.EndTime);
            });

            modelBuilder.Entity<SeckillActivity>(build =>
            {
                build.HasIndex(t => t.Code).IsUnique();
                build.HasIndex(t => t.Status);
                build.HasIndex(t => t.StartTime);
                build.HasIndex(t => t.EndTime);
                build.HasIndex(t => t.CreatedAt);
            });

            modelBuilder.Entity<GroupBuyActivity>(build =>
            {
                build.HasIndex(t => t.Code).IsUnique();
                build.HasIndex(t => t.Status);
                build.HasIndex(t => t.StartTime);
                build.HasIndex(t => t.EndTime);
                build.HasIndex(t => t.CreatedAt);
            });

            modelBuilder.Entity<BargainActivity>(build =>
            {
                build.HasIndex(t => t.Code).IsUnique();
                build.HasIndex(t => t.Status);
                build.HasIndex(t => t.StartTime);
                build.HasIndex(t => t.EndTime);
                build.HasIndex(t => t.CreatedAt);
            });
        }
    }
}
