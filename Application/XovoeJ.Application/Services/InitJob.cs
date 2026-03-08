using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using XovoeJ.Entities;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Application.Services
{
    public class InitJob
    {
        private const string SuperAdminRoleName = "\u8d85\u7ea7\u7ba1\u7406\u5458";
        private const string FrontendUserRoleName = "\u524d\u7aef\u7528\u6237";
        private const string BackendUserRoleName = "\u540e\u7aef\u7528\u6237";
        private const string DefaultAdminDisplayName = "\u7ba1\u7406\u5458\u8d26\u53f7";

        private static readonly string[] BackendUserDefaultPermissions =
        [
            "admin.system",
            "admin.user",
            "admin.user.read",
            "admin.role",
            "admin.role.read",
            "admin.mall",
            "admin.product",
            "admin.product.read",
            "admin.category",
            "admin.category.read",
            "admin.order",
            "admin.order.read",
            "admin.aftersale",
            "admin.aftersale.read",
            "admin.content",
            "admin.banner",
            "admin.banner.read",
            "admin.dictionary",
            "admin.dictionary.read",
            "admin.workflow",
            "admin.workflow.read",
            "admin.marketing",
            "admin.coupon",
            "admin.coupon.read",
            "admin.promotion",
            "admin.promotion.read",
            "admin.marketing.seckill",
            "admin.marketing.seckill.manage",
            "admin.marketing.group-buy",
            "admin.marketing.group-buy.manage",
            "admin.marketing.bargain",
            "admin.marketing.bargain.manage",
            "admin.growth",
            "admin.distribution",
            "admin.distribution.read",
            "admin.referral-link",
            "admin.referral-link.read",
            "admin.commission",
            "admin.commission.read",
            "admin.message",
            "admin.message.template",
            "admin.message.template.read",
            "admin.message.task",
            "admin.message.task.read",
            "admin.message.record",
            "admin.message.record.read",
        ];

        private UserManager<User> userManage = null!;
        private XovoeJDbContext dbContext = null!;

        public async Task Init(IServiceScope scope)
        {
            userManage = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            dbContext = scope.ServiceProvider.GetRequiredService<XovoeJDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            var canConnect = await dbContext.Database.CanConnectAsync();

            if (!canConnect)
            {
                await dbContext.Database.MigrateAsync();
                await ImportMasterData();
            }
            else if (pendingMigrations.Any())
            {
                await dbContext.Database.MigrateAsync();
            }

            await CreateRole(roleManager);
            await CreateUser();
            await SeedMallUserData();
            await SeedMarketingCenterData();
            await SeedMessageCenterData();
            await SeedGrowthCenterData();
        }

        private async Task ImportMasterData()
        {
            dbContext.Database.SetCommandTimeout(1800);
            var sqlScriptsDir = Path.Combine(AppContext.BaseDirectory, "sqlscripts");

            if (!Directory.Exists(sqlScriptsDir))
            {
                sqlScriptsDir = "sqlscripts";
                if (!Directory.Exists(sqlScriptsDir))
                {
                    return;
                }
            }

            foreach (var file in Directory.GetFiles(sqlScriptsDir, "*.sql"))
            {
                try
                {
                    var sql = await File.ReadAllTextAsync(file);
                    var statements = sql.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s)
                            && !s.StartsWith("--")
                            && !s.StartsWith("/*")
                            && !s.StartsWith("DROP TABLE", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    foreach (var statement in statements)
                    {
                        if (!string.IsNullOrWhiteSpace(statement))
                        {
                            await dbContext.Database.ExecuteSqlRawAsync(statement);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to execute SQL script {Path.GetFileName(file)}: {ex.Message}");
                }
            }
        }

        private async Task CreateUser()
        {
            await AddSysAccount("milo", [SuperAdminRoleName], DefaultAdminDisplayName);
        }

        private async Task AddSysAccount(string userName, string[] roleNames, string? realName = null)
        {
            var user = await userManage.FindByNameAsync(userName);
            if (user == null)
            {
                user = new User
                {
                    NickName = realName ?? userName,
                    RealName = realName ?? userName,
                };

                if (!string.IsNullOrEmpty(userName))
                {
                    await userManage.SetPhoneNumberAsync(user, userName);
                    await userManage.SetUserNameAsync(user, userName);
                }

                await userManage.AddPasswordAsync(user, "123456");

                var result = await userManage.CreateAsync(user);
                if (!result.Succeeded)
                {
                    throw new Exception(JsonConvert.SerializeObject(result.Errors));
                }

                await userManage.SetLockoutEnabledAsync(user, false);
                await dbContext.SaveChangesAsync();
            }

            foreach (var roleName in roleNames)
            {
                if (!await userManage.IsInRoleAsync(user, roleName))
                {
                    await userManage.AddToRoleAsync(user, roleName);
                }
            }
        }

        private async Task CreateRole(RoleManager<Role> roleManager)
        {
            var role = await EnsureRoleExistsAsync(
                roleManager,
                SuperAdminRoleName,
                10,
                "\u7cfb\u7edf\u5185\u7f6e\u7ba1\u7406\u5458\uff0c\u62e5\u6709\u6240\u6709\u6743\u9650");
            await EnsureRolePermissionsAsync(roleManager, role, ["*"]);

            await EnsureRoleExistsAsync(roleManager, FrontendUserRoleName, 30, FrontendUserRoleName);

            role = await EnsureRoleExistsAsync(roleManager, BackendUserRoleName, 40, BackendUserRoleName);
            await EnsureRolePermissionsAsync(roleManager, role, BackendUserDefaultPermissions);
        }

        private static async Task<Role> EnsureRoleExistsAsync(RoleManager<Role> roleManager, string roleName, int sort, string description)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                role = new Role
                {
                    Name = roleName,
                    Sort = sort,
                    Description = description,
                };
                await roleManager.CreateAsync(role);
            }

            return role;
        }

        private static async Task EnsureRolePermissionsAsync(RoleManager<Role> roleManager, Role role, IEnumerable<string> permissions)
        {
            var existingClaims = await roleManager.GetClaimsAsync(role);
            foreach (var permission in permissions.Distinct())
            {
                if (!existingClaims.Any(c => c.Type == "permission" && c.Value == permission))
                {
                    await roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("permission", permission));
                }
            }
        }

        private async Task SeedMessageCenterData()
        {
            if (await dbContext.MessageTemplates.AnyAsync())
            {
                return;
            }

            var now = DateTime.UtcNow;

            var orderPaidTemplate = new MessageTemplate
            {
                Name = "\u8ba2\u5355\u652f\u4ed8\u6210\u529f\u901a\u77e5",
                Code = "order_paid_email",
                Channel = "email",
                BusinessType = "order",
                Subject = "\u8ba2\u5355\u652f\u4ed8\u6210\u529f",
                ContentPreview = "\u60a8\u597d\uff0c\u8ba2\u5355 {{orderNo}} \u5df2\u652f\u4ed8\u6210\u529f\uff0c\u8bf7\u7559\u610f\u540e\u7eed\u53d1\u8d27\u901a\u77e5\u3002",
                Description = "\u652f\u4ed8\u6210\u529f\u540e\u7684\u9ed8\u8ba4\u90ae\u4ef6\u6a21\u677f\u3002",
                Status = 1,
                CreatedAt = now.AddDays(-7),
                UpdatedAt = now.AddDays(-2),
            };

            var shipmentTemplate = new MessageTemplate
            {
                Name = "\u53d1\u8d27\u901a\u77e5",
                Code = "shipment_sms",
                Channel = "sms",
                BusinessType = "shipping",
                Subject = "\u8ba2\u5355\u5df2\u53d1\u8d27",
                ContentPreview = "\u8ba2\u5355 {{orderNo}} \u5df2\u53d1\u8d27\uff0c\u7269\u6d41\u5355\u53f7\uff1a{{trackingNo}}\u3002",
                Description = "\u53d1\u8d27\u901a\u77e5\u7684\u9ed8\u8ba4\u77ed\u4fe1\u6a21\u677f\u3002",
                Status = 1,
                CreatedAt = now.AddDays(-6),
                UpdatedAt = now.AddDays(-1),
            };

            var promotionTemplate = new MessageTemplate
            {
                Name = "\u6d3b\u52a8\u5e7f\u64ad\u901a\u77e5",
                Code = "campaign_in_app",
                Channel = "inApp",
                BusinessType = "marketing",
                Subject = "\u9650\u65f6\u6d3b\u52a8\u4e0a\u7ebf",
                ContentPreview = "\u65b0\u7684\u6d3b\u52a8\u5df2\u7ecf\u4e0a\u7ebf\uff0c\u5feb\u8fdb\u5165\u5e94\u7528\u9886\u53d6\u4f60\u7684\u4e13\u5c5e\u4f18\u60e0\u3002",
                Description = "\u8425\u9500\u6d3b\u52a8\u5e7f\u64ad\u7684\u9ed8\u8ba4\u7ad9\u5185\u4fe1\u6a21\u677f\u3002",
                Status = 0,
                CreatedAt = now.AddDays(-5),
                UpdatedAt = now.AddDays(-5),
            };

            dbContext.MessageTemplates.AddRange(orderPaidTemplate, shipmentTemplate, promotionTemplate);

            var paymentTask = new MessageTask
            {
                Name = "\u652f\u4ed8\u6210\u529f\u6279\u91cf\u901a\u77e5",
                Template = orderPaidTemplate,
                Channel = orderPaidTemplate.Channel,
                TriggerType = "order_paid",
                RecipientCount = 42,
                SuccessCount = 41,
                FailedCount = 1,
                Status = 2,
                ScheduledAt = now.AddDays(-2).AddMinutes(-10),
                SentAt = now.AddDays(-2),
                CreatedAt = now.AddDays(-2).AddHours(-1),
                UpdatedAt = now.AddDays(-2),
            };

            var shipmentTask = new MessageTask
            {
                Name = "\u53d1\u8d27\u77ed\u4fe1\u4efb\u52a1",
                Template = shipmentTemplate,
                Channel = shipmentTemplate.Channel,
                TriggerType = "order_shipped",
                RecipientCount = 18,
                SuccessCount = 16,
                FailedCount = 2,
                Status = 1,
                ScheduledAt = now.AddHours(1),
                SentAt = null,
                CreatedAt = now.AddMinutes(-30),
                UpdatedAt = now.AddMinutes(-10),
            };

            dbContext.MessageTasks.AddRange(paymentTask, shipmentTask);

            dbContext.MessageSendRecords.AddRange(
                new MessageSendRecord
                {
                    Template = orderPaidTemplate,
                    Task = paymentTask,
                    Channel = orderPaidTemplate.Channel,
                    Recipient = "milo@example.com",
                    BusinessType = orderPaidTemplate.BusinessType,
                    TraceId = $"msg-{Guid.NewGuid():N}"[..20],
                    Status = 1,
                    SentAt = now.AddDays(-2),
                    CreatedAt = now.AddDays(-2).AddMinutes(-5),
                    UpdatedAt = now.AddDays(-2),
                },
                new MessageSendRecord
                {
                    Template = shipmentTemplate,
                    Task = shipmentTask,
                    Channel = shipmentTemplate.Channel,
                    Recipient = "13800000000",
                    BusinessType = shipmentTemplate.BusinessType,
                    TraceId = $"msg-{Guid.NewGuid():N}"[..20],
                    ErrorMessage = "\u6e20\u9053\u670d\u52a1\u9996\u6b21\u8c03\u7528\u8d85\u65f6\u3002",
                    Status = 2,
                    SentAt = now.AddMinutes(-20),
                    CreatedAt = now.AddMinutes(-25),
                    UpdatedAt = now.AddMinutes(-20),
                },
                new MessageSendRecord
                {
                    Template = promotionTemplate,
                    Channel = promotionTemplate.Channel,
                    Recipient = "user-feed",
                    BusinessType = promotionTemplate.BusinessType,
                    TraceId = $"msg-{Guid.NewGuid():N}"[..20],
                    Status = 0,
                    SentAt = null,
                    CreatedAt = now.AddHours(-6),
                    UpdatedAt = now.AddHours(-6),
                });

            await dbContext.SaveChangesAsync();
        }

        private async Task SeedMarketingCenterData()
        {
            var now = DateTime.UtcNow;

            if (!await dbContext.CouponTemplates.AnyAsync())
            {
                dbContext.CouponTemplates.AddRange(
                    new CouponTemplate
                {
                    Name = "\u65b0\u4eba\u9996\u5355\u6ee1\u51cf\u5238",
                    Code = "NEW_USER_FULL_REDUCTION",
                    CouponType = 0,
                    DiscountType = 1,
                    DiscountValue = 20.00m,
                    MinOrderAmount = 99.00m,
                    TotalQuantity = 5000,
                    IssuedQuantity = 1260,
                    UsedQuantity = 486,
                    Status = 1,
                    ReceiveLimit = 1,
                    Description = "\u65b0\u4eba\u9996\u5355\u6ee1 99 \u51cf 20\uff0c\u7528\u4e8e\u63d0\u5347\u9996\u5355\u8f6c\u5316\u3002",
                    StartTime = now.AddDays(-30),
                    EndTime = now.AddDays(30),
                    CreatedAt = now.AddDays(-35),
                    UpdatedAt = now.AddDays(-2),
                },
                new CouponTemplate
                {
                    Name = "\u4f1a\u5458\u65e5 9 \u6298\u5238",
                    Code = "MEMBER_DAY_DISCOUNT",
                    CouponType = 1,
                    DiscountType = 0,
                    DiscountValue = 9.00m,
                    MinOrderAmount = 0m,
                    TotalQuantity = 3000,
                    IssuedQuantity = 980,
                    UsedQuantity = 410,
                    Status = 1,
                    ReceiveLimit = 2,
                    Description = "\u4f1a\u5458\u65e5\u6d3b\u52a8\u4e13\u5c5e\u6298\u6263\u5238\uff0c\u9002\u7528\u4e8e\u6307\u5b9a\u4f1a\u5458\u5546\u54c1\u3002",
                    StartTime = now.AddDays(-7),
                    EndTime = now.AddDays(10),
                    CreatedAt = now.AddDays(-10),
                    UpdatedAt = now.AddDays(-1),
                },
                new CouponTemplate
                {
                    Name = "\u56de\u8d2d\u6fc0\u6d3b\u65e0\u95e8\u69db\u5238",
                    Code = "REBUY_DIRECT_COUPON",
                    CouponType = 2,
                    DiscountType = 1,
                    DiscountValue = 8.00m,
                    MinOrderAmount = 0m,
                    TotalQuantity = 8000,
                    IssuedQuantity = 2400,
                    UsedQuantity = 1160,
                    Status = 2,
                    ReceiveLimit = 1,
                    Description = "\u9488\u5bf9 30 \u5929\u672a\u4e0b\u5355\u7528\u6237\u7684\u53ec\u56de\u4f18\u60e0\u5238\u3002",
                    StartTime = now.AddDays(-45),
                    EndTime = now.AddDays(-5),
                    CreatedAt = now.AddDays(-50),
                    UpdatedAt = now.AddDays(-5),
                });
            }

            if (!await dbContext.PromotionActivities.AnyAsync())
            {
                dbContext.PromotionActivities.AddRange(
                    new PromotionActivity
                {
                    Name = "\u6625\u5b63\u5927\u4fc3\u6ee1\u51cf",
                    Type = 0,
                    ScopeText = "\u5168\u573a\u5b9e\u7269\u5546\u54c1\uff08\u7279\u4ef7\u5546\u54c1\u9664\u5916\uff09",
                    Priority = 100,
                    Stackable = false,
                    OrderCount = 356,
                    ParticipantCount = 282,
                    Status = 1,
                    Description = "\u5168\u573a\u6ee1 199 \u51cf 30\uff0c\u7ed3\u7b97\u65f6\u6309\u8ba2\u5355\u7ef4\u5ea6\u751f\u6548\u3002",
                    StartTime = now.AddDays(-5),
                    EndTime = now.AddDays(15),
                    CreatedAt = now.AddDays(-10),
                    UpdatedAt = now.AddDays(-1),
                },
                new PromotionActivity
                {
                    Name = "\u7206\u6b3e\u7c7b\u76ee\u9650\u65f6\u6298\u6263",
                    Type = 1,
                    ScopeText = "\u5bb6\u5c45\u4e0e\u65e5\u7528\u7c7b\u76ee\u6307\u5b9a SKU",
                    Priority = 90,
                    Stackable = true,
                    OrderCount = 198,
                    ParticipantCount = 164,
                    Status = 1,
                    Description = "\u6307\u5b9a\u5546\u54c1\u5728\u6d3b\u52a8\u671f\u95f4\u6309\u4fc3\u9500\u4ef7\u7ed3\u7b97\uff0c\u53ef\u4e0e\u90e8\u5206\u4f1a\u5458\u6298\u6263\u53e0\u52a0\u3002",
                    StartTime = now.AddDays(-2),
                    EndTime = now.AddDays(5),
                    CreatedAt = now.AddDays(-6),
                    UpdatedAt = now.AddHours(-8),
                },
                new PromotionActivity
                {
                    Name = "\u62a4\u80a4\u5957\u88c5\u4e70\u8d60",
                    Type = 2,
                    ScopeText = "\u62a4\u80a4\u5957\u88c5\u7ec4\u5408\u6ee1\u8d2d\u573a\u666f",
                    Priority = 80,
                    Stackable = false,
                    OrderCount = 87,
                    ParticipantCount = 79,
                    Status = 0,
                    Description = "\u8d2d\u4e70\u6307\u5b9a\u62a4\u80a4\u5957\u88c5\u5373\u53ef\u8d60\u9001\u8bd5\u7528\u88c5\uff0c\u5f85\u6d3b\u52a8\u5f00\u59cb\u540e\u751f\u6548\u3002",
                    StartTime = now.AddDays(3),
                    EndTime = now.AddDays(20),
                    CreatedAt = now.AddDays(-1),
                    UpdatedAt = now.AddDays(-1),
                });
            }

            if (!await dbContext.SeckillActivities.AnyAsync())
            {
                dbContext.SeckillActivities.AddRange(
                    new SeckillActivity
                    {
                        Name = "春日焕新秒杀专场",
                        Code = "SECKILL-202603-001",
                        ProductName = "樱花保湿精华礼盒",
                        SkuName = "樱花限定装 50ml",
                        OriginalPrice = 299.00m,
                        SeckillPrice = 199.00m,
                        TotalStock = 300,
                        LockedStock = 18,
                        SoldStock = 164,
                        PurchaseLimit = 1,
                        ParticipantCount = 241,
                        OrderCount = 158,
                        Status = 1,
                        WarmupStartTime = now.AddHours(-6),
                        StartTime = now.AddHours(-2),
                        EndTime = now.AddHours(10),
                        Description = "春日焕新主题秒杀场，主推护肤爆品单品拉新。",
                        CreatedAt = now.AddDays(-3),
                        UpdatedAt = now.AddMinutes(-20),
                    },
                    new SeckillActivity
                    {
                        Name = "夜间家清限量秒杀",
                        Code = "SECKILL-202603-002",
                        ProductName = "居家抑菌清洁套装",
                        SkuName = "三件组合装",
                        OriginalPrice = 159.00m,
                        SeckillPrice = 99.00m,
                        TotalStock = 500,
                        LockedStock = 0,
                        SoldStock = 0,
                        PurchaseLimit = 2,
                        ParticipantCount = 0,
                        OrderCount = 0,
                        Status = 0,
                        WarmupStartTime = now.AddHours(8),
                        StartTime = now.AddHours(12),
                        EndTime = now.AddHours(18),
                        Description = "面向晚高峰流量的日用家清秒杀活动，预热阶段展示中。",
                        CreatedAt = now.AddDays(-1),
                        UpdatedAt = now.AddHours(-1),
                    },
                    new SeckillActivity
                    {
                        Name = "会员专场母婴秒杀",
                        Code = "SECKILL-202602-007",
                        ProductName = "婴幼儿柔护洗护套装",
                        SkuName = "会员专享款",
                        OriginalPrice = 229.00m,
                        SeckillPrice = 149.00m,
                        TotalStock = 200,
                        LockedStock = 0,
                        SoldStock = 200,
                        PurchaseLimit = 1,
                        ParticipantCount = 312,
                        OrderCount = 198,
                        Status = 2,
                        WarmupStartTime = now.AddDays(-10),
                        StartTime = now.AddDays(-9),
                        EndTime = now.AddDays(-8),
                        Description = "会员专场秒杀已结束，用于沉淀复盘数据。",
                        CreatedAt = now.AddDays(-12),
                        UpdatedAt = now.AddDays(-8),
                    });
            }

            if (!await dbContext.GroupBuyActivities.AnyAsync())
            {
                dbContext.GroupBuyActivities.AddRange(
                    new GroupBuyActivity
                    {
                        Name = "春季护肤两人成团",
                        Code = "GROUPBUY-202603-001",
                        ProductName = "樱花保湿精华礼盒",
                        SkuName = "双件组合装",
                        OriginalPrice = 299.00m,
                        GroupPrice = 219.00m,
                        GroupSize = 2,
                        PurchaseLimit = 1,
                        VirtualGroupCount = 12,
                        ParticipantCount = 186,
                        SuccessGroupCount = 78,
                        FailedGroupCount = 9,
                        GroupLeaderReward = 12.00m,
                        Status = 1,
                        StartTime = now.AddDays(-2),
                        EndTime = now.AddDays(5),
                        Description = "主打拉新与复购的双人成团活动，团长可额外获得返利。",
                        CreatedAt = now.AddDays(-4),
                        UpdatedAt = now.AddHours(-2),
                    },
                    new GroupBuyActivity
                    {
                        Name = "家清囤货三人成团",
                        Code = "GROUPBUY-202603-002",
                        ProductName = "居家抑菌清洁套装",
                        SkuName = "三件组合装",
                        OriginalPrice = 159.00m,
                        GroupPrice = 109.00m,
                        GroupSize = 3,
                        PurchaseLimit = 2,
                        VirtualGroupCount = 6,
                        ParticipantCount = 0,
                        SuccessGroupCount = 0,
                        FailedGroupCount = 0,
                        GroupLeaderReward = 8.00m,
                        Status = 0,
                        StartTime = now.AddDays(1),
                        EndTime = now.AddDays(10),
                        Description = "面向家庭囤货场景的三人成团活动，预热阶段用于提前蓄水。",
                        CreatedAt = now.AddDays(-1),
                        UpdatedAt = now.AddHours(-5),
                    },
                    new GroupBuyActivity
                    {
                        Name = "母婴会员四人成团",
                        Code = "GROUPBUY-202602-006",
                        ProductName = "婴幼儿柔护洗护套装",
                        SkuName = "会员专享款",
                        OriginalPrice = 229.00m,
                        GroupPrice = 169.00m,
                        GroupSize = 4,
                        PurchaseLimit = 1,
                        VirtualGroupCount = 0,
                        ParticipantCount = 240,
                        SuccessGroupCount = 52,
                        FailedGroupCount = 14,
                        GroupLeaderReward = 15.00m,
                        Status = 2,
                        StartTime = now.AddDays(-12),
                        EndTime = now.AddDays(-6),
                        Description = "会员裂变场景的四人成团活动，已结束，保留用于后续复盘。",
                        CreatedAt = now.AddDays(-15),
                        UpdatedAt = now.AddDays(-6),
                    });
            }

            if (!await dbContext.BargainActivities.AnyAsync())
            {
                dbContext.BargainActivities.AddRange(
                    new BargainActivity
                    {
                        Name = "春日焕肤好友砍价",
                        Code = "BARGAIN-202603-001",
                        ProductName = "樱花保湿精华礼盒",
                        SkuName = "限定版 50ml",
                        OriginalPrice = 299.00m,
                        FloorPrice = 189.00m,
                        CurrentLowestPrice = 196.00m,
                        PurchaseLimit = 1,
                        ParticipantCount = 268,
                        HelperCount = 1342,
                        SuccessCount = 112,
                        Status = 1,
                        StartTime = now.AddDays(-1),
                        EndTime = now.AddDays(6),
                        Description = "面向春季拉新与老客唤醒的好友助力砍价活动，主打高转化单品。",
                        CreatedAt = now.AddDays(-3),
                        UpdatedAt = now.AddMinutes(-35),
                    },
                    new BargainActivity
                    {
                        Name = "家清爆品限时帮砍",
                        Code = "BARGAIN-202603-002",
                        ProductName = "居家抑菌清洁套装",
                        SkuName = "三件组合装",
                        OriginalPrice = 159.00m,
                        FloorPrice = 99.00m,
                        CurrentLowestPrice = 99.00m,
                        PurchaseLimit = 2,
                        ParticipantCount = 0,
                        HelperCount = 0,
                        SuccessCount = 0,
                        Status = 0,
                        StartTime = now.AddHours(10),
                        EndTime = now.AddDays(5),
                        Description = "针对家庭囤货场景的预热砍价活动，提前积累助力关系和待转化用户。",
                        CreatedAt = now.AddDays(-1),
                        UpdatedAt = now.AddHours(-2),
                    },
                    new BargainActivity
                    {
                        Name = "母婴会员专场砍到底",
                        Code = "BARGAIN-202602-009",
                        ProductName = "婴幼儿柔护洗护套装",
                        SkuName = "会员专享款",
                        OriginalPrice = 229.00m,
                        FloorPrice = 149.00m,
                        CurrentLowestPrice = 149.00m,
                        PurchaseLimit = 1,
                        ParticipantCount = 196,
                        HelperCount = 1184,
                        SuccessCount = 84,
                        Status = 2,
                        StartTime = now.AddDays(-14),
                        EndTime = now.AddDays(-9),
                        Description = "会员社群裂变场景的历史砍价活动，用于复盘助力链路和成单效率。",
                        CreatedAt = now.AddDays(-16),
                        UpdatedAt = now.AddDays(-9),
                    });
            }

            await dbContext.SaveChangesAsync();
        }

        private async Task SeedMallUserData()
        {
            if (await dbContext.UserAddresses.AnyAsync())
            {
                return;
            }

            var admin = await userManage.FindByNameAsync("milo");
            if (admin == null)
            {
                return;
            }

            dbContext.UserAddresses.Add(
                new UserAddress
                {
                    UserId = admin.Id,
                    ConsigneeName = "\u7ba1\u7406\u5458",
                    Mobile = "13800138000",
                    Province = "\u4e0a\u6d77\u5e02",
                    City = "\u4e0a\u6d77\u5e02",
                    Area = "\u6d66\u4e1c\u65b0\u533a",
                    RegionCode = "310115",
                    DetailAddress = "\u5f20\u6c5f\u8def 88 \u53f7 XovoeJ \u7535\u5546\u4e2d\u5fc3",
                    PostalCode = "200120",
                    Label = "\u516c\u53f8",
                    IsDefault = true,
                    Sort = 100,
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                });

            await dbContext.SaveChangesAsync();
        }

        private async Task SeedGrowthCenterData()
        {
            var now = DateTime.UtcNow;

            if (!await dbContext.ReferralLinks.AnyAsync())
            {
                dbContext.ReferralLinks.AddRange(
                    new ReferralLink
                    {
                        Name = "首页分享推广链路",
                        Code = "GROWTH-HOME-001",
                        OwnerName = "米洛",
                        Channel = "shareLink",
                        LandingPath = "/pages/home",
                        FullUrl = "https://mall.xovoej.com/pages/home?ref=GROWTH-HOME-001",
                        AttributionSource = "首页首屏横幅",
                        CampaignName = "春季拉新",
                        ClickCount = 826,
                        SignupCount = 132,
                        FirstOrderCount = 46,
                        RewardAmount = 368.00m,
                        Status = 1,
                        ExpireAt = now.AddDays(30),
                        LastVisitAt = now.AddMinutes(-42),
                        CreatedAt = now.AddDays(-12),
                        UpdatedAt = now.AddHours(-3),
                    },
                    new ReferralLink
                    {
                        Name = "结算页邀请返利链接",
                        Code = "GROWTH-CHECKOUT-008",
                        OwnerName = "艾拉",
                        Channel = "campaign",
                        LandingPath = "/pages/checkout/invite",
                        FullUrl = "https://mall.xovoej.com/pages/checkout/invite?ref=GROWTH-CHECKOUT-008",
                        AttributionSource = "结算页浮层",
                        CampaignName = "复购激励",
                        ClickCount = 214,
                        SignupCount = 28,
                        FirstOrderCount = 11,
                        RewardAmount = 88.00m,
                        Status = 1,
                        ExpireAt = now.AddDays(7),
                        LastVisitAt = now.AddHours(-6),
                        CreatedAt = now.AddDays(-5),
                        UpdatedAt = now.AddHours(-6),
                    },
                    new ReferralLink
                    {
                        Name = "社群分销备用链接",
                        Code = "GROWTH-COMMUNITY-013",
                        OwnerName = "诺亚",
                        Channel = "inviteCode",
                        LandingPath = "/pages/invite/landing",
                        FullUrl = "https://mall.xovoej.com/pages/invite/landing?ref=GROWTH-COMMUNITY-013",
                        AttributionSource = "企微社群分享",
                        CampaignName = "社群裂变",
                        ClickCount = 93,
                        SignupCount = 9,
                        FirstOrderCount = 2,
                        RewardAmount = 16.00m,
                        Status = 0,
                        ExpireAt = now.AddDays(3),
                        LastVisitAt = now.AddDays(-1),
                        CreatedAt = now.AddDays(-2),
                        UpdatedAt = now.AddDays(-1),
                    });
            }

            if (!await dbContext.InviteRelations.AnyAsync())
            {
                dbContext.InviteRelations.AddRange(
                    new InviteRelation
                {
                    InviterName = "\u7c73\u6d1b",
                    InviteeName = "\u5c0f\u827e",
                    ReferralCode = "MILO-AVA",
                    Channel = "shareLink",
                    AttributionSource = "\u9996\u9875\u5206\u4eab\u6a2a\u5e45",
                    TotalOrders = 3,
                    TotalRewardAmount = 88.00m,
                    Status = 1,
                    InvitedAt = now.AddDays(-12),
                    FirstOrderAt = now.AddDays(-10),
                    CreatedAt = now.AddDays(-12),
                    UpdatedAt = now.AddDays(-2),
                },
                new InviteRelation
                {
                    InviterName = "\u7c73\u6d1b",
                    InviteeName = "\u8bfa\u4e9a",
                    ReferralCode = "MILO-NOAH",
                    Channel = "inviteCode",
                    AttributionSource = "\u7ed3\u7b97\u9875\u9080\u8bf7\u5165\u53e3",
                    TotalOrders = 0,
                    TotalRewardAmount = 0m,
                    Status = 0,
                    InvitedAt = now.AddDays(-1),
                    FirstOrderAt = null,
                    CreatedAt = now.AddDays(-1),
                    UpdatedAt = now.AddHours(-6),
                },
                new InviteRelation
                {
                    InviterName = "\u827e\u62c9",
                    InviteeName = "\u5362\u5361\u65af",
                    ReferralCode = "ELLA-LUCAS",
                    Channel = "campaign",
                    AttributionSource = "\u6625\u5b63\u589e\u957f\u6d3b\u52a8",
                    TotalOrders = 1,
                    TotalRewardAmount = 20.00m,
                    Status = 2,
                    InvitedAt = now.AddDays(-20),
                    FirstOrderAt = now.AddDays(-19),
                    CreatedAt = now.AddDays(-20),
                    UpdatedAt = now.AddDays(-18),
                });
            }

            if (!await dbContext.CommissionRecords.AnyAsync())
            {
                dbContext.CommissionRecords.AddRange(
                    new CommissionRecord
                {
                    PromoterName = "\u7c73\u6d1b",
                    OrderNo = "ORD-20260301-1001",
                    RuleName = "\u9996\u5355\u5956\u52b1",
                    SourceType = "\u9080\u8bf7\u9996\u5355",
                    CommissionRate = 10.00m,
                    EstimatedAmount = 36.80m,
                    SettledAmount = 36.80m,
                    Status = 2,
                    CreatedAt = now.AddDays(-7),
                    SettledAt = now.AddDays(-4),
                    UpdatedAt = now.AddDays(-4),
                },
                new CommissionRecord
                {
                    PromoterName = "\u827e\u62c9",
                    OrderNo = "ORD-20260305-2008",
                    RuleName = "\u6d3b\u52a8\u5956\u52b1",
                    SourceType = "\u6d3b\u52a8\u8ba2\u5355",
                    CommissionRate = 8.00m,
                    EstimatedAmount = 25.60m,
                    SettledAmount = 0m,
                    Status = 1,
                    CreatedAt = now.AddDays(-2),
                    SettledAt = null,
                    UpdatedAt = now.AddHours(-12),
                },
                new CommissionRecord
                {
                    PromoterName = "\u8bfa\u4e9a",
                    OrderNo = "ORD-20260306-3002",
                    RuleName = "\u9000\u6b3e\u56de\u9000",
                    SourceType = "\u9000\u6b3e\u51b2\u56de",
                    CommissionRate = 10.00m,
                    EstimatedAmount = 18.00m,
                    SettledAmount = 0m,
                    Status = 3,
                    CreatedAt = now.AddDays(-1),
                    SettledAt = null,
                    UpdatedAt = now.AddHours(-4),
                });
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
