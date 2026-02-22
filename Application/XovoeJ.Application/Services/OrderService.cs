using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using XovoeJ.Abstractions.Services;
using XovoeJ.Contracts.Order;
using XovoeJ.Entities;
using XovoeJ.Enum;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Application.Services
{
    /// <summary>
    /// 订单服务实现
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly XovoeJDbContext _dbContext;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            XovoeJDbContext dbContext,
            ILogger<OrderService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 提交订单
        /// </summary>
        public async Task<OrderDto> SubmitOrderAsync(string userId, SubmitOrderRequestDto request)
        {
            // 获取商品信息
            var orderItems = new List<OrderItem>();

            if (request.CartItemIds != null && request.CartItemIds.Any())
            {
                // 从购物车下单
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
                        Subtotal = sku.Price * cartItem.Quantity
                    });
                }

                // 清除购物车中已下单的商品
                _dbContext.ShoppingCarts.RemoveRange(cartItems);
            }
            else if (request.DirectItems != null && request.DirectItems.Any())
            {
                // 直接购买
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
                        throw new ArgumentException($"商品不存在");
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
                        Subtotal = sku.Price * item.Quantity
                    });
                }
            }
            else
            {
                throw new ArgumentException("请选择要购买的商品");
            }

            // 计算金额
            var totalAmount = orderItems.Sum(i => i.Subtotal);
            var discountAmount = 0m;
            var freightAmount = 0m; // TODO: 计算运费
            var payAmount = totalAmount - discountAmount + freightAmount;

            // 生成订单号
            var orderNo = GenerateOrderNo();

            // 创建订单
            var order = new Order
            {
                OrderNo = orderNo,
                UserId = userId,
                TotalAmount = totalAmount,
                DiscountAmount = discountAmount,
                FreightAmount = freightAmount,
                PayAmount = payAmount,
                Status = OrderStatus.Pending,
                PayStatus = 0,
                ShipStatus = 0,
                ConsigneeName = request.ConsigneeName,
                ConsigneeMobile = request.ConsigneeMobile,
                ConsigneeAddress = request.ConsigneeAddress,
                Remark = request.Remark
            };

            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            // 添加订单项
            foreach (var item in orderItems)
            {
                item.OrderId = order.Id;
                _dbContext.OrderItems.Add(item);
            }

            await _dbContext.SaveChangesAsync();

            // 扣减库存
            foreach (var item in orderItems)
            {
                var sku = await _dbContext.ProductSkus.FirstAsync(s => s.Id == item.SkuId);
                sku.Stock -= item.Quantity;
                sku.SalesCount += item.Quantity;
            }

            await _dbContext.SaveChangesAsync();

            return await GetOrderByIdAsync(userId, order.Id);
        }

        /// <summary>
        /// 获取订单列表
        /// </summary>
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
                PageSize = query.PageSize
            };
        }

        /// <summary>
        /// 根据ID获取订单
        /// </summary>
        public async Task<OrderDto?> GetOrderByIdAsync(string userId, string orderId)
        {
            var order = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            return order != null ? MapToOrderDto(order) : null;
        }

        /// <summary>
        /// 根据订单号获取订单
        /// </summary>
        public async Task<OrderDto?> GetOrderByNoAsync(string userId, string orderNo)
        {
            var order = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderNo == orderNo && o.UserId == userId);

            return order != null ? MapToOrderDto(order) : null;
        }

        /// <summary>
        /// 取消订单
        /// </summary>
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

            // 恢复库存
            foreach (var item in order.OrderItems)
            {
                var sku = await _dbContext.ProductSkus.FirstAsync(s => s.Id == item.SkuId);
                sku.Stock += item.Quantity;
            }

            order.Status = OrderStatus.Cancelled;
            order.CancelTime = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 确认收货
        /// </summary>
        public async Task<bool> ConfirmReceiptAsync(string userId, string orderId)
        {
            var order = await _dbContext.Orders
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

        /// <summary>
        /// 删除订单
        /// </summary>
        public async Task<bool> DeleteOrderAsync(string userId, string orderId)
        {
            var order = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                return false;
            }

            // 只有已完成或已取消的订单可以删除
            if (order.Status != OrderStatus.Completed && order.Status != OrderStatus.Cancelled)
            {
                throw new ArgumentException("只能删除已完成或已取消的订单");
            }

            _dbContext.OrderItems.RemoveRange(order.OrderItems);
            _dbContext.Orders.Remove(order);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        #region 私有方法

        /// <summary>
        /// 生成订单号
        /// </summary>
        private static string GenerateOrderNo()
        {
            return $"ORD{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
        }

        /// <summary>
        /// 映射订单实体到DTO
        /// </summary>
        private static OrderDto MapToOrderDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                OrderNo = order.OrderNo,
                UserId = order.UserId,
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                FreightAmount = order.FreightAmount,
                PayAmount = order.PayAmount,
                Status = order.Status,
                PayStatus = order.PayStatus,
                PayTime = order.PayTime,
                ShipStatus = order.ShipStatus,
                ShipTime = order.ShipTime,
                ReceiveTime = order.ReceiveTime,
                FinishTime = order.FinishTime,
                ConsigneeName = order.ConsigneeName,
                ConsigneeMobile = order.ConsigneeMobile,
                ConsigneeAddress = order.ConsigneeAddress,
                Remark = order.Remark,
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
                    Subtotal = i.Subtotal
                }).ToList()
            };
        }

        #endregion
    }
}
