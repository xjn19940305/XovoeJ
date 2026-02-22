using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using XovoeJ.Abstractions.Services;
using XovoeJ.Contracts.Cart;
using XovoeJ.Entities;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Application.Services
{
    /// <summary>
    /// 购物车服务实现
    /// </summary>
    public class CartService : ICartService
    {
        private readonly XovoeJDbContext _dbContext;
        private readonly ILogger<CartService> _logger;

        public CartService(
            XovoeJDbContext dbContext,
            ILogger<CartService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 获取购物车列表
        /// </summary>
        public async Task<CartResponseDto> GetCartAsync(string userId)
        {
            var cartItems = await _dbContext.ShoppingCarts
                .Include(c => c.Product)
                    .ThenInclude(p => p.Skus)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var items = new List<CartItemDto>();

            foreach (var cartItem in cartItems)
            {
                var sku = cartItem.Product?.Skus.FirstOrDefault(s => s.Id == cartItem.SkuId);
                if (cartItem.Product == null || sku == null)
                {
                    // 商品或SKU不存在，删除该购物车项
                    _dbContext.ShoppingCarts.Remove(cartItem);
                    continue;
                }

                items.Add(new CartItemDto
                {
                    Id = cartItem.Id,
                    ProductId = cartItem.ProductId,
                    ProductName = cartItem.Product.Name,
                    ProductImage = cartItem.Product.MainImage,
                    SkuId = cartItem.SkuId,
                    SkuSpecs = !string.IsNullOrEmpty(sku.Specs) ? JsonSerializer.Deserialize<Dictionary<string, string>>(sku.Specs) : null,
                    Price = sku.Price,
                    Quantity = cartItem.Quantity,
                    Stock = sku.Stock
                });
            }

            await _dbContext.SaveChangesAsync();

            return new CartResponseDto
            {
                Items = items
            };
        }

        /// <summary>
        /// 添加到购物车
        /// </summary>
        public async Task AddToCartAsync(string userId, AddToCartRequestDto request)
        {
            // 验证商品和SKU是否存在
            var product = await _dbContext.Products
                .Include(p => p.Skus)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId);
            if (product == null)
            {
                throw new ArgumentException("商品不存在");
            }

            var sku = product.Skus.FirstOrDefault(s => s.Id == request.SkuId);
            if (sku == null)
            {
                throw new ArgumentException("商品规格不存在");
            }

            if (!product.IsEnabled || !sku.IsEnabled)
            {
                throw new ArgumentException("商品已下架");
            }

            if (sku.Stock < request.Quantity)
            {
                throw new ArgumentException("库存不足");
            }

            // 查找购物车中是否已存在该商品SKU
            var existingCartItem = await _dbContext.ShoppingCarts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.SkuId == request.SkuId);

            if (existingCartItem != null)
            {
                // 更新数量
                existingCartItem.Quantity += request.Quantity;
                existingCartItem.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // 添加新项
                var cartItem = new ShoppingCart
                {
                    UserId = userId,
                    ProductId = request.ProductId,
                    SkuId = request.SkuId,
                    Quantity = request.Quantity
                };
                _dbContext.ShoppingCarts.Add(cartItem);
            }

            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// 更新购物车商品数量
        /// </summary>
        public async Task UpdateQuantityAsync(string userId, string cartItemId, UpdateCartQuantityRequestDto request)
        {
            var cartItem = await _dbContext.ShoppingCarts
                .Include(c => c.Product)
                    .ThenInclude(p => p.Skus)
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

            if (cartItem == null)
            {
                throw new ArgumentException("购物车项不存在");
            }

            var sku = cartItem.Product?.Skus.FirstOrDefault(s => s.Id == cartItem.SkuId);
            if (sku == null)
            {
                throw new ArgumentException("商品规格不存在");
            }

            if (sku.Stock < request.Quantity)
            {
                throw new ArgumentException("库存不足");
            }

            cartItem.Quantity = request.Quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// 删除购物车商品
        /// </summary>
        public async Task RemoveFromCartAsync(string userId, string cartItemId)
        {
            var cartItem = await _dbContext.ShoppingCarts
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

            if (cartItem == null)
            {
                throw new ArgumentException("购物车项不存在");
            }

            _dbContext.ShoppingCarts.Remove(cartItem);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// 清空购物车
        /// </summary>
        public async Task ClearCartAsync(string userId)
        {
            var cartItems = await _dbContext.ShoppingCarts
                .Where(c => c.UserId == userId)
                .ToListAsync();

            _dbContext.ShoppingCarts.RemoveRange(cartItems);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// 获取购物车商品数量
        /// </summary>
        public async Task<int> GetCartCountAsync(string userId)
        {
            return await _dbContext.ShoppingCarts
                .Where(c => c.UserId == userId)
                .SumAsync(c => c.Quantity);
        }
    }
}
