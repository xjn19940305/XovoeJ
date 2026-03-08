using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using XovoeJ.Abstractions.Services;
using XovoeJ.Contracts.Order;
using XovoeJ.Entities;
using XovoeJ.Enum;
using XovoeJ.EventBus.Abstractions;
using XovoeJ.EventBus.Events;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly XovoeJDbContext _dbContext;
        private readonly ILogger<OrderService> _logger;
        private readonly IEventBus _eventBus;
        private readonly IAssetLedgerService _assetLedgerService;

        public OrderService(
            XovoeJDbContext dbContext,
            ILogger<OrderService> logger,
            IEventBus eventBus,
            IAssetLedgerService assetLedgerService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _eventBus = eventBus;
            _assetLedgerService = assetLedgerService;
        }

        public async Task<OrderDto> SubmitOrderAsync(string userId, SubmitOrderRequestDto request)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var orderItems = await BuildOrderItemsAsync(userId, request);
                if (orderItems.Count == 0)
                {
                    throw new ArgumentException("请选择要购买的商品");
                }

                ValidateInvoiceRequest(request);

                var totalAmount = orderItems.Sum(i => i.Subtotal);
                var resolvedCoupon = await ResolveOrderCouponAsync(userId, request.UserCouponId ?? request.CouponId, totalAmount);
                var discountAmount = resolvedCoupon?.DiscountAmount ?? 0m;
                var freightAmount = 0m;
                var payAmount = Math.Max(0m, totalAmount - discountAmount + freightAmount);
                var orderNo = GenerateOrderNo();

                var coupon = resolvedCoupon?.Coupon;

                var order = new Order
                {
                    OrderNo = orderNo,
                    UserId = userId,
                    TotalAmount = totalAmount,
                    DiscountAmount = discountAmount,
                    UserCouponId = coupon?.Id,
                    CouponTemplateId = coupon?.CouponTemplateId,
                    CouponName = coupon?.SnapshotName,
                    FreightAmount = freightAmount,
                    PayAmount = payAmount,
                    Status = OrderStatus.Pending,
                    PayStatus = 0,
                    ShipStatus = 0,
                    ConsigneeName = request.ConsigneeName,
                    ConsigneeMobile = request.ConsigneeMobile,
                    ConsigneeAddress = request.ConsigneeAddress,
                    Remark = request.Remark,
                    NeedInvoice = request.NeedInvoice,
                    InvoiceType = request.NeedInvoice ? request.InvoiceType : null,
                    InvoiceTitle = request.NeedInvoice ? NormalizeOptionalText(request.InvoiceTitle) : null,
                    InvoiceTaxNo = request.NeedInvoice ? NormalizeOptionalText(request.InvoiceTaxNo) : null,
                    InvoiceEmail = request.NeedInvoice ? NormalizeOptionalText(request.InvoiceEmail) : null,
                };

                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();

                var paymentOrder = CreatePaymentOrderEntity(order);
                _dbContext.PaymentOrders.Add(paymentOrder);
                await _dbContext.SaveChangesAsync();

                order.PaymentOrderId = paymentOrder.Id;
                order.PaymentOrderNo = paymentOrder.PaymentOrderNo;
                order.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                foreach (var item in orderItems)
                {
                    item.OrderId = order.Id;
                    _dbContext.OrderItems.Add(item);
                }

                foreach (var item in orderItems)
                {
                    var rowsAffected = await _dbContext.ProductSkus
                        .Where(s => s.Id == item.SkuId && s.Stock >= item.Quantity)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(p => p.Stock, p => p.Stock - item.Quantity)
                            .SetProperty(p => p.SalesCount, p => p.SalesCount + item.Quantity));

                    if (rowsAffected == 0)
                    {
                        throw new ArgumentException($"商品 {item.ProductName} 库存不足，请重新下单");
                    }
                }

                if (resolvedCoupon != null)
                {
                    coupon!.Status = CouponStatus.Used;
                    coupon.UsedAt = DateTime.UtcNow;
                    coupon.OrderId = order.Id;
                    coupon.OrderNo = order.OrderNo;
                    coupon.UpdatedAt = DateTime.UtcNow;

                    var template = await _dbContext.CouponTemplates
                        .AsTracking()
                        .FirstOrDefaultAsync(item => item.Id == coupon!.CouponTemplateId);
                    if (template != null)
                    {
                        template.UsedQuantity += 1;
                        template.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                await _eventBus.PublishAsync(new OrderCreatedEvent
                {
                    OrderId = order.Id,
                    UserId = userId,
                    OrderNo = order.OrderNo,
                    TotalAmount = order.TotalAmount,
                });

                return await GetOrderByIdAsync(userId, order.Id) ?? throw new InvalidOperationException("订单创建成功但加载失败");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderListResponseDto> GetOrdersAsync(string userId, OrderListQueryDto query)
        {
            var queryable = _dbContext.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .AsQueryable();

            if (query.Status.HasValue)
            {
                queryable = queryable.Where(o => (int)o.Status == query.Status.Value);
            }

            var total = await queryable.CountAsync();
            var orders = await queryable
                .OrderByDescending(o => o.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new OrderListResponseDto
            {
                Items = orders.Select(MapToOrderDto).ToList(),
                Total = total,
                Page = query.Page,
                PageSize = query.PageSize,
            };
        }

        public async Task<OrderDto?> GetOrderByIdAsync(string userId, string orderId)
        {
            var order = await _dbContext.Orders
                .AsTracking()
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            return order != null ? MapToOrderDto(order) : null;
        }

        public async Task<OrderDto?> GetOrderByNoAsync(string userId, string orderNo)
        {
            var order = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderNo == orderNo && o.UserId == userId);

            return order != null ? MapToOrderDto(order) : null;
        }

        public async Task<bool> CancelOrderAsync(string userId, string orderId)
        {
            var order = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                return false;
            }

            if (order.Status != OrderStatus.Pending)
            {
                throw new ArgumentException("只有待支付订单可以取消");
            }

            foreach (var item in order.OrderItems)
            {
                await _dbContext.ProductSkus
                    .Where(s => s.Id == item.SkuId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(p => p.Stock, p => p.Stock + item.Quantity)
                        .SetProperty(p => p.SalesCount, p => p.SalesCount - item.Quantity));
            }

            if (!string.IsNullOrWhiteSpace(order.UserCouponId))
            {
                var userCoupon = await _dbContext.UserCoupons
                    .AsTracking()
                    .FirstOrDefaultAsync(item => item.Id == order.UserCouponId && item.UserId == userId);
                if (userCoupon != null && userCoupon.Status == CouponStatus.Used)
                {
                    userCoupon.Status = CouponStatus.Unused;
                    userCoupon.UsedAt = null;
                    userCoupon.OrderId = null;
                    userCoupon.OrderNo = null;
                    userCoupon.UpdatedAt = DateTime.UtcNow;

                    var template = await _dbContext.CouponTemplates
                        .AsTracking()
                        .FirstOrDefaultAsync(item => item.Id == userCoupon.CouponTemplateId);
                    if (template != null && template.UsedQuantity > 0)
                    {
                        template.UsedQuantity -= 1;
                        template.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(order.PaymentOrderId))
            {
                var paymentOrder = await _dbContext.PaymentOrders
                    .AsTracking()
                    .FirstOrDefaultAsync(item => item.Id == order.PaymentOrderId);

                if (paymentOrder != null && paymentOrder.Status == PaymentOrderStatus.Pending)
                {
                    paymentOrder.Status = PaymentOrderStatus.Closed;
                    paymentOrder.ClosedAt = DateTime.UtcNow;
                    paymentOrder.UpdatedAt = DateTime.UtcNow;
                }
            }

            order.Status = OrderStatus.Cancelled;
            order.CancelTime = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            await _eventBus.PublishAsync(new OrderCancelledEvent
            {
                OrderId = order.Id,
                OrderNo = order.OrderNo,
                Reason = "用户主动取消",
                CancelledTime = DateTime.UtcNow,
            });

            return true;
        }

        public async Task<bool> ConfirmReceiptAsync(string userId, string orderId)
        {
            var order = await _dbContext.Orders
                .AsTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
            if (order == null)
            {
                return false;
            }

            if (order.Status != OrderStatus.Shipped)
            {
                throw new ArgumentException("只有已发货订单可以确认收货");
            }

            order.Status = OrderStatus.Received;
            order.ReceiveTime = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteOrderAsync(string userId, string orderId)
        {
            var order = await _dbContext.Orders
                .AsTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
            if (order == null)
            {
                return false;
            }

            if (order.Status != OrderStatus.Completed && order.Status != OrderStatus.Cancelled)
            {
                throw new ArgumentException("只能删除已完成或已取消的订单");
            }

            order.IsDeleted = true;
            order.DeletedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PayOrderAsync(string orderId, string paymentMethod, string? paymentOrderId = null, string? paymentOrderNo = null, decimal? paidAmount = null)
        {
            var order = await _dbContext.Orders
                .AsTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return false;
            }

            if (order.Status != OrderStatus.Pending)
            {
                throw new ArgumentException("只有待支付订单可以支付");
            }

            order.Status = OrderStatus.Paid;
            order.PayStatus = 1;
            order.PayTime = DateTime.UtcNow;
            order.PaymentMethod = paymentMethod;
            order.PaymentOrderId = paymentOrderId ?? order.PaymentOrderId;
            order.PaymentOrderNo = paymentOrderNo ?? order.PaymentOrderNo;
            order.WalletPayAmount = string.Equals(paymentMethod, "wallet", StringComparison.OrdinalIgnoreCase)
                ? paidAmount ?? order.PayAmount
                : order.WalletPayAmount;
            order.RewardPoints = order.RewardPoints <= 0 ? CalculateRewardPoints(order.PayAmount) : order.RewardPoints;
            order.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            if (order.RewardPoints > 0)
            {
                await _assetLedgerService.AddPointsAsync(
                    order.UserId,
                    order.RewardPoints,
                    "order_reward",
                    order.OrderNo,
                    $"points-reward-{order.OrderNo}",
                    "订单支付奖励积分");
            }

            await UpdateUserMemberLevelAsync(order.UserId);

            await _eventBus.PublishAsync(new OrderPaidEvent
            {
                OrderId = order.Id,
                OrderNo = order.OrderNo,
                PaidAmount = paidAmount ?? order.PayAmount,
                PaymentMethod = paymentMethod,
                PaymentTime = DateTime.UtcNow,
            });

            return true;
        }

        public async Task<bool> ShipOrderAsync(string orderId, ShipOrderRequestDto request)
        {
            var order = await _dbContext.Orders
                .AsTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return false;
            }

            if (order.Status != OrderStatus.Paid)
            {
                throw new ArgumentException("只有已支付订单可以发货");
            }

            order.Status = OrderStatus.Shipped;
            order.ShipStatus = 1;
            order.ShipTime = DateTime.UtcNow;
            order.ShippingCompany = request.ShippingCompany.Trim();
            order.TrackingNo = request.TrackingNo.Trim();
            order.ShippingRemark = string.IsNullOrWhiteSpace(request.ShippingRemark) ? null : request.ShippingRemark.Trim();
            order.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<OrderTrackingDto?> GetOrderTrackingAsync(string userId, string orderId)
        {
            var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
            if (order == null)
            {
                return null;
            }

            return new OrderTrackingDto
            {
                OrderId = order.Id,
                OrderNo = order.OrderNo,
                ShippingCompany = order.ShippingCompany,
                TrackingNo = order.TrackingNo,
                ShippingRemark = order.ShippingRemark,
                ShipTime = order.ShipTime,
                TrackingStatus = order.Status switch
                {
                    OrderStatus.Paid => "待发货",
                    OrderStatus.Shipped => "运输中",
                    OrderStatus.Received => "已签收",
                    OrderStatus.Completed => "已完成",
                    OrderStatus.Cancelled => "已取消",
                    _ => "待支付",
                },
            };
        }

        public async Task<bool> CompleteOrderAsync(string userId, string orderId)
        {
            var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
            if (order == null)
            {
                return false;
            }

            if (order.Status != OrderStatus.Received)
            {
                throw new ArgumentException("只有已收货订单可以完成");
            }

            order.Status = OrderStatus.Completed;
            order.FinishTime = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task<List<OrderItem>> BuildOrderItemsAsync(string userId, SubmitOrderRequestDto request)
        {
            var orderItems = new List<OrderItem>();

            if (request.CartItemIds != null && request.CartItemIds.Any())
            {
                var cartItems = await _dbContext.ShoppingCarts
                    .Include(c => c.Product)
                        .ThenInclude(p => p.Skus)
                    .Where(c => c.UserId == userId && request.CartItemIds.Contains(c.Id))
                    .ToListAsync();

                foreach (var cartItem in cartItems)
                {
                    var sku = cartItem.Product?.Skus.FirstOrDefault(s => s.Id == cartItem.SkuId);
                    if (cartItem.Product == null || sku == null)
                    {
                        throw new ArgumentException($"商品 {cartItem.Product?.Name} 不存在或已下架");
                    }

                    if (!cartItem.Product.IsEnabled)
                    {
                        throw new ArgumentException($"商品 {cartItem.Product.Name} 已下架");
                    }

                    if (!sku.IsEnabled || sku.Stock < cartItem.Quantity)
                    {
                        throw new ArgumentException($"商品 {cartItem.Product.Name} 库存不足");
                    }

                    orderItems.Add(new OrderItem
                    {
                        ProductId = cartItem.ProductId,
                        ProductName = cartItem.Product.Name,
                        ProductImage = cartItem.Product.MainImage,
                        SkuId = cartItem.SkuId,
                        SkuSpecs = sku.Specs,
                        Price = sku.Price,
                        Quantity = cartItem.Quantity,
                        Subtotal = sku.Price * cartItem.Quantity,
                    });
                }

                _dbContext.ShoppingCarts.RemoveRange(cartItems);
                return orderItems;
            }

            if (request.DirectItems == null || !request.DirectItems.Any())
            {
                return orderItems;
            }

            var productIds = request.DirectItems.Select(i => i.ProductId).Distinct().ToList();
            var products = await _dbContext.Products
                .Include(p => p.Skus)
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            foreach (var item in request.DirectItems)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null)
                {
                    throw new ArgumentException("商品不存在");
                }

                if (!product.IsEnabled)
                {
                    throw new ArgumentException($"商品 {product.Name} 已下架");
                }

                var sku = product.Skus.FirstOrDefault(s => s.Id == item.SkuId);
                if (sku == null || !sku.IsEnabled)
                {
                    throw new ArgumentException($"商品 {product.Name} 规格不存在或已下架");
                }

                if (sku.Stock < item.Quantity)
                {
                    throw new ArgumentException($"商品 {product.Name} 库存不足");
                }

                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = product.Name,
                    ProductImage = product.MainImage,
                    SkuId = item.SkuId,
                    SkuSpecs = sku.Specs,
                    Price = sku.Price,
                    Quantity = item.Quantity,
                    Subtotal = sku.Price * item.Quantity,
                });
            }

            return orderItems;
        }

        private async Task<(UserCoupon Coupon, decimal DiscountAmount)?> ResolveOrderCouponAsync(string userId, string? userCouponId, decimal totalAmount)
        {
            if (string.IsNullOrWhiteSpace(userCouponId))
            {
                return null;
            }

            var now = DateTime.UtcNow;
            var userCoupon = await _dbContext.UserCoupons
                .AsTracking()
                .FirstOrDefaultAsync(item => item.Id == userCouponId && item.UserId == userId);
            if (userCoupon == null)
            {
                throw new ArgumentException("优惠券不存在或不属于当前用户。");
            }

            if (userCoupon.Status != CouponStatus.Unused)
            {
                throw new ArgumentException("当前优惠券不可用。");
            }

            if (userCoupon.ExpiredAt.HasValue && userCoupon.ExpiredAt.Value < now)
            {
                throw new ArgumentException("优惠券已过期。");
            }

            if (totalAmount < userCoupon.SnapshotMinOrderAmount)
            {
                throw new ArgumentException("当前订单金额未达到优惠券使用门槛。");
            }

            decimal discountAmount;
            if (userCoupon.SnapshotCouponType == 1)
            {
                var rate = Math.Clamp(userCoupon.SnapshotDiscountValue / 10m, 0m, 1m);
                discountAmount = Math.Round(totalAmount * (1 - rate), 2, MidpointRounding.AwayFromZero);
            }
            else
            {
                discountAmount = Math.Min(totalAmount, userCoupon.SnapshotDiscountValue);
            }

            return (userCoupon, discountAmount);
        }

        private async Task UpdateUserMemberLevelAsync(string userId)
        {
            var user = await _dbContext.Users
                .AsTracking()
                .FirstOrDefaultAsync(item => item.Id == userId);
            if (user == null)
            {
                return;
            }

            var totalSpent = await _dbContext.Orders
                .Where(item => item.UserId == userId
                    && (item.Status == OrderStatus.Paid
                        || item.Status == OrderStatus.Shipped
                        || item.Status == OrderStatus.Received
                        || item.Status == OrderStatus.Completed))
                .SumAsync(item => (decimal?)item.PayAmount) ?? 0m;

            var previousLevelCode = user.CurrentMemberLevelCode ?? "normal";
            var nextLevelCode = ResolveMemberLevelCode(totalSpent);

            user.TotalSpentAmount = totalSpent;
            user.CurrentMemberLevelCode = nextLevelCode;
            user.UpdateAt = DateTime.UtcNow;

            if (GetMemberLevelRank(nextLevelCode) > GetMemberLevelRank(previousLevelCode)
                && !string.Equals(user.RewardedMemberLevelCode, nextLevelCode, StringComparison.OrdinalIgnoreCase))
            {
                await IssueMemberUpgradeCouponsAsync(user, nextLevelCode);
            }

            await _dbContext.SaveChangesAsync();
        }

        private static string ResolveMemberLevelCode(decimal totalSpent)
        {
            if (totalSpent >= 50000m)
            {
                return "diamond";
            }

            if (totalSpent >= 10000m)
            {
                return "platinum";
            }

            if (totalSpent >= 5000m)
            {
                return "gold";
            }

            if (totalSpent >= 1000m)
            {
                return "silver";
            }

            return "normal";
        }

        private async Task IssueMemberUpgradeCouponsAsync(User user, string levelCode)
        {
            var rule = await _dbContext.MemberLevelRewardRules.FirstOrDefaultAsync(item => item.LevelCode == levelCode && item.Status == 1);
            if (rule == null || string.IsNullOrWhiteSpace(rule.CouponTemplateIdsJson))
            {
                user.RewardedMemberLevelCode = levelCode;
                return;
            }

            List<string>? templateIds;
            try
            {
                templateIds = JsonSerializer.Deserialize<List<string>>(rule.CouponTemplateIdsJson);
            }
            catch
            {
                templateIds = null;
            }

            if (templateIds == null || templateIds.Count == 0)
            {
                user.RewardedMemberLevelCode = levelCode;
                return;
            }

            var templates = await _dbContext.CouponTemplates
                .AsTracking()
                .Where(item => templateIds.Contains(item.Id) && item.Status == 1)
                .ToListAsync();

            var now = DateTime.UtcNow;
            foreach (var template in templates)
            {
                if (template.TotalQuantity > 0 && template.IssuedQuantity >= template.TotalQuantity)
                {
                    continue;
                }

                _dbContext.UserCoupons.Add(new UserCoupon
                {
                    UserId = user.Id,
                    CouponTemplateId = template.Id,
                    Status = CouponStatus.Unused,
                    SourceType = "member_upgrade",
                    SourceReference = levelCode,
                    SnapshotName = template.Name,
                    SnapshotCouponType = template.CouponType,
                    SnapshotDiscountType = template.DiscountType,
                    SnapshotDiscountValue = template.DiscountValue,
                    SnapshotMinOrderAmount = template.MinOrderAmount,
                    IssuedAt = now,
                    ClaimedAt = now,
                    ExpiredAt = template.EndTime,
                    CreatedAt = now,
                    UpdatedAt = now,
                });

                template.IssuedQuantity += 1;
                template.UpdatedAt = now;
            }

            user.RewardedMemberLevelCode = levelCode;
        }

        private static int GetMemberLevelRank(string levelCode)
        {
            return levelCode switch
            {
                "diamond" => 5,
                "platinum" => 4,
                "gold" => 3,
                "silver" => 2,
                _ => 1,
            };
        }

        private static string GenerateOrderNo()
        {
            return $"ORD{DateTime.UtcNow:yyyyMMddHHmmssfff}{Guid.NewGuid().ToString("N")[..8]}";
        }

        private static PaymentOrder CreatePaymentOrderEntity(Order order)
        {
            return new PaymentOrder
            {
                OrderId = order.Id,
                OrderNo = order.OrderNo,
                UserId = order.UserId,
                PaymentOrderNo = GeneratePaymentOrderNo(),
                PayableAmount = order.PayAmount,
                PaidAmount = 0m,
                RefundedAmount = 0m,
                Status = PaymentOrderStatus.Pending,
                ExpireAt = DateTime.UtcNow.AddMinutes(30),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
        }

        private static string GeneratePaymentOrderNo()
        {
            return $"PAY{DateTime.UtcNow:yyyyMMddHHmmssfff}{Guid.NewGuid().ToString("N")[..8]}";
        }

        private static int CalculateRewardPoints(decimal payAmount)
        {
            if (payAmount <= 0)
            {
                return 0;
            }

            return Math.Max(0, (int)Math.Floor(payAmount));
        }

        private static OrderDto MapToOrderDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                OrderNo = order.OrderNo,
                UserId = order.UserId,
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                UserCouponId = order.UserCouponId,
                CouponTemplateId = order.CouponTemplateId,
                CouponName = order.CouponName,
                FreightAmount = order.FreightAmount,
                PayAmount = order.PayAmount,
                PaymentOrderId = order.PaymentOrderId,
                PaymentOrderNo = order.PaymentOrderNo,
                PaymentMethod = order.PaymentMethod,
                WalletPayAmount = order.WalletPayAmount,
                PointsUsed = order.PointsUsed,
                PointsDeductionAmount = order.PointsDeductionAmount,
                RewardPoints = order.RewardPoints,
                RefundedAmount = order.RefundedAmount,
                Status = order.Status,
                PayStatus = order.PayStatus,
                PayTime = order.PayTime,
                ShipStatus = order.ShipStatus,
                ShipTime = order.ShipTime,
                ShippingCompany = order.ShippingCompany,
                TrackingNo = order.TrackingNo,
                ShippingRemark = order.ShippingRemark,
                ReceiveTime = order.ReceiveTime,
                FinishTime = order.FinishTime,
                ConsigneeName = order.ConsigneeName,
                ConsigneeMobile = order.ConsigneeMobile,
                ConsigneeAddress = order.ConsigneeAddress,
                Remark = order.Remark,
                NeedInvoice = order.NeedInvoice,
                InvoiceType = order.InvoiceType,
                InvoiceTitle = order.InvoiceTitle,
                InvoiceTaxNo = order.InvoiceTaxNo,
                InvoiceEmail = order.InvoiceEmail,
                CreatedAt = order.CreatedAt,
                OrderItems = order.OrderItems.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    ProductImage = i.ProductImage,
                    SkuId = i.SkuId,
                    SkuSpecs = !string.IsNullOrEmpty(i.SkuSpecs) ? JsonSerializer.Deserialize<Dictionary<string, string>>(i.SkuSpecs) : null,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    Subtotal = i.Subtotal,
                }).ToList(),
            };
        }

        private static void ValidateInvoiceRequest(SubmitOrderRequestDto request)
        {
            if (!request.NeedInvoice)
            {
                return;
            }

            if (request.InvoiceType is not (1 or 2))
            {
                throw new ArgumentException("发票类型不正确");
            }

            if (string.IsNullOrWhiteSpace(request.InvoiceTitle))
            {
                throw new ArgumentException("请填写发票抬头");
            }

            if (request.InvoiceType == 2 && string.IsNullOrWhiteSpace(request.InvoiceTaxNo))
            {
                throw new ArgumentException("企业发票请填写税号");
            }
        }

        private static string? NormalizeOptionalText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
