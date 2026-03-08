using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using XovoeJ.Api.Swaggers;
using XovoeJ.Contracts.Mall;
using XovoeJ.Entities;
using XovoeJ.Enum;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Api.Controllers
{
    [ApiController]
    [Route("api/mall/checkout")]
    [Produces("application/json")]
    [ApiGroup(ApiGroupNames.USER)]
    [Authorize]
    public class MallCheckoutController : ControllerBase
    {
        private readonly XovoeJDbContext _dbContext;
        private readonly ILogger<MallCheckoutController> _logger;

        public MallCheckoutController(XovoeJDbContext dbContext, ILogger<MallCheckoutController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpPost("preview")]
        public async Task<IActionResult> Preview([FromBody] CheckoutPreviewRequestDto request)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息。" });
                }

                var items = await BuildPreviewItemsAsync(userId, request);
                if (items.Count == 0)
                {
                    return BadRequest(new { message = "请先选择要结算的商品。" });
                }

                var productAmount = items.Sum(item => item.Subtotal);
                var address = await ResolveAddressAsync(userId, request.AddressId);
                var couponId = request.UserCouponId ?? request.CouponId;
                var coupon = await ResolveCouponAsync(userId, couponId, productAmount);
                var discountAmount = coupon?.AppliedDiscountAmount ?? 0m;
                var freightAmount = 0m;
                var payAmount = Math.Max(0m, productAmount - discountAmount + freightAmount);

                var invalidItem = items.FirstOrDefault(item => !item.IsValid);
                var unavailableReason = invalidItem != null ? $"商品“{invalidItem.ProductName}”库存不足或已失效。" : null;
                if (address == null)
                {
                    unavailableReason ??= "请先选择收货地址。";
                }

                var response = new CheckoutPreviewResponseDto
                {
                    Items = items,
                    Address = address,
                    Coupon = coupon,
                    ProductAmount = productAmount,
                    DiscountAmount = discountAmount,
                    FreightAmount = freightAmount,
                    PayAmount = payAmount,
                    CanSubmit = invalidItem == null && address != null,
                    UnavailableReason = unavailableReason,
                };

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载结算预览失败。");
                return BadRequest(new { message = "加载结算预览失败。" });
            }
        }

        private async Task<List<CheckoutPreviewItemDto>> BuildPreviewItemsAsync(string userId, CheckoutPreviewRequestDto request)
        {
            if (request.CartItemIds != null && request.CartItemIds.Count > 0)
            {
                var cartItems = await _dbContext.ShoppingCarts
                    .Include(item => item.Product)
                        .ThenInclude(product => product!.Skus)
                    .Where(item => item.UserId == userId && request.CartItemIds.Contains(item.Id))
                    .ToListAsync();

                return cartItems.Select(item =>
                {
                    var sku = item.Product?.Skus.FirstOrDefault(row => row.Id == item.SkuId);
                    return MapPreviewItem(item.ProductId, item.Product, sku, item.Quantity);
                }).ToList();
            }

            if (request.DirectItems != null && request.DirectItems.Count > 0)
            {
                var productIds = request.DirectItems.Select(item => item.ProductId).Distinct().ToList();
                var products = await _dbContext.Products
                    .Include(item => item.Skus)
                    .Where(item => productIds.Contains(item.Id))
                    .ToListAsync();

                return request.DirectItems.Select(item =>
                {
                    var product = products.FirstOrDefault(row => row.Id == item.ProductId);
                    var sku = product?.Skus.FirstOrDefault(row => row.Id == item.SkuId);
                    return MapPreviewItem(item.ProductId, product, sku, item.Quantity);
                }).ToList();
            }

            return new List<CheckoutPreviewItemDto>();
        }

        private async Task<CheckoutAddressDto?> ResolveAddressAsync(string userId, string? addressId)
        {
            var query = _dbContext.UserAddresses.AsNoTracking().Where(item => item.UserId == userId);
            var address = !string.IsNullOrWhiteSpace(addressId)
                ? await query.FirstOrDefaultAsync(item => item.Id == addressId)
                : await query.OrderByDescending(item => item.IsDefault).ThenByDescending(item => item.Sort).ThenByDescending(item => item.CreatedAt).FirstOrDefaultAsync();

            if (address == null)
            {
                return null;
            }

            return new CheckoutAddressDto
            {
                Id = address.Id,
                ConsigneeName = address.ConsigneeName,
                Mobile = address.Mobile,
                FullAddress = JoinAddress(address.Province, address.City, address.Area, address.DetailAddress),
                Label = address.Label,
                IsDefault = address.IsDefault,
            };
        }

        private async Task<CheckoutCouponDto?> ResolveCouponAsync(string userId, string? userCouponId, decimal productAmount)
        {
            if (string.IsNullOrWhiteSpace(userCouponId))
            {
                return null;
            }

            var now = DateTime.UtcNow;
            var userCoupon = await _dbContext.UserCoupons
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == userCouponId
                    && item.UserId == userId
                    && item.Status == CouponStatus.Unused);

            if (userCoupon == null)
            {
                throw new ArgumentException("优惠券不存在、不可用或不属于当前用户。");
            }

            if (userCoupon.ExpiredAt.HasValue && userCoupon.ExpiredAt.Value < now)
            {
                throw new ArgumentException("优惠券已过期。");
            }

            var appliedDiscountAmount = CalculateCouponDiscount(userCoupon, productAmount);
            return new CheckoutCouponDto
            {
                Id = userCoupon.CouponTemplateId,
                UserCouponId = userCoupon.Id,
                Name = userCoupon.SnapshotName,
                CouponType = userCoupon.SnapshotCouponType,
                DiscountType = userCoupon.SnapshotDiscountType,
                DiscountValue = userCoupon.SnapshotDiscountValue,
                MinOrderAmount = userCoupon.SnapshotMinOrderAmount,
                AppliedDiscountAmount = appliedDiscountAmount,
            };
        }

        private static CheckoutPreviewItemDto MapPreviewItem(string productId, Product? product, ProductSku? sku, int quantity)
        {
            var isValid = product != null && sku != null && product.IsEnabled && sku.IsEnabled && sku.Stock >= quantity;
            var price = sku?.Price ?? 0m;

            return new CheckoutPreviewItemDto
            {
                ProductId = productId,
                ProductName = product?.Name ?? "商品不存在",
                ProductImage = product?.MainImage ?? sku?.Image,
                SkuId = sku?.Id ?? string.Empty,
                SkuCode = sku?.SkuCode,
                SkuSpecs = !string.IsNullOrWhiteSpace(sku?.Specs) ? JsonSerializer.Deserialize<Dictionary<string, string>>(sku.Specs) : null,
                Price = price,
                Quantity = quantity,
                Stock = sku?.Stock ?? 0,
                Subtotal = price * quantity,
                IsValid = isValid,
            };
        }

        private static decimal CalculateCouponDiscount(UserCoupon coupon, decimal productAmount)
        {
            if (productAmount < coupon.SnapshotMinOrderAmount)
            {
                throw new ArgumentException("当前订单金额未达到优惠券使用门槛。");
            }

            if (coupon.SnapshotCouponType == 1)
            {
                var rate = Math.Clamp(coupon.SnapshotDiscountValue / 10m, 0m, 1m);
                return Math.Round(productAmount * (1 - rate), 2, MidpointRounding.AwayFromZero);
            }

            return Math.Min(productAmount, coupon.SnapshotDiscountValue);
        }

        private static string JoinAddress(string? province, string? city, string? area, string detailAddress)
        {
            return string.Join(" ", new[] { province, city, area, detailAddress }.Where(item => !string.IsNullOrWhiteSpace(item)));
        }
    }
}
