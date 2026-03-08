using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using XovoeJ.Api.Swaggers;
using XovoeJ.Entities;
using XovoeJ.Enum;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Api.Controllers
{
    [ApiController]
    [Route("api/mall")]
    [Produces("application/json")]
    [ApiGroup(ApiGroupNames.USER)]
    [Authorize]
    public class MallAccountController : ControllerBase
    {
        private readonly XovoeJDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<MallAccountController> _logger;

        public MallAccountController(
            XovoeJDbContext dbContext,
            UserManager<User> userManager,
            ILogger<MallAccountController> logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("account/summary")]
        public async Task<IActionResult> GetAccountSummary()
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息。" });
                }

                var user = await _userManager.Users.AsNoTracking().FirstOrDefaultAsync(item => item.Id == userId);
                if (user == null)
                {
                    return NotFound(new { message = "用户不存在。" });
                }

                var orderQuery = _dbContext.Orders.AsNoTracking().Where(item => item.UserId == userId);
                var totalOrders = await orderQuery.CountAsync();
                var pendingOrders = await orderQuery.CountAsync(item => item.Status == OrderStatus.Pending);
                var shippedOrders = await orderQuery.CountAsync(item => item.Status == OrderStatus.Shipped);
                var completedOrders = await orderQuery.CountAsync(item => item.Status == OrderStatus.Completed || item.Status == OrderStatus.Received);
                var totalSpent = await orderQuery
                    .Where(item => item.Status == OrderStatus.Paid || item.Status == OrderStatus.Shipped || item.Status == OrderStatus.Received || item.Status == OrderStatus.Completed)
                    .SumAsync(item => (decimal?)item.PayAmount) ?? 0m;

                var defaultAddress = await _dbContext.UserAddresses
                    .AsNoTracking()
                    .Where(item => item.UserId == userId)
                    .OrderByDescending(item => item.IsDefault)
                    .ThenByDescending(item => item.Sort)
                    .ThenByDescending(item => item.CreatedAt)
                    .Select(MapAddress)
                    .FirstOrDefaultAsync();

                var availableCouponCount = await _dbContext.CouponTemplates
                    .AsNoTracking()
                    .CountAsync(item => item.Status == 1);

                return Ok(new MallAccountSummaryDto
                {
                    UserId = user.Id,
                    NickName = user.NickName ?? user.UserName,
                    RealName = user.RealName,
                    PhoneNumber = user.PhoneNumber,
                    Picture = user.Picture,
                    MemberLevel = ResolveMemberLevel(totalSpent),
                    TotalSpent = totalSpent,
                    TotalOrders = totalOrders,
                    PendingOrders = pendingOrders,
                    ShippedOrders = shippedOrders,
                    CompletedOrders = completedOrders,
                    AvailableCouponCount = availableCouponCount,
                    DefaultAddress = defaultAddress,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载商城用户摘要失败。");
                return BadRequest(new { message = "加载商城用户摘要失败。" });
            }
        }

        [HttpGet("addresses")]
        public async Task<IActionResult> GetAddresses()
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息。" });
                }

                var items = await _dbContext.UserAddresses
                    .AsNoTracking()
                    .Where(item => item.UserId == userId)
                    .OrderByDescending(item => item.IsDefault)
                    .ThenByDescending(item => item.Sort)
                    .ThenByDescending(item => item.CreatedAt)
                    .Select(MapAddress)
                    .ToListAsync();

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载地址列表失败。");
                return BadRequest(new { message = "加载地址列表失败。" });
            }
        }

        [HttpGet("addresses/{addressId}")]
        public async Task<IActionResult> GetAddress(string addressId)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息。" });
                }

                var item = await _dbContext.UserAddresses
                    .AsNoTracking()
                    .Where(row => row.UserId == userId && row.Id == addressId)
                    .Select(MapAddress)
                    .FirstOrDefaultAsync();

                if (item == null)
                {
                    return NotFound(new { message = "地址不存在。" });
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载地址详情失败：{AddressId}", addressId);
                return BadRequest(new { message = "加载地址详情失败。" });
            }
        }

        [HttpPost("addresses")]
        public async Task<IActionResult> CreateAddress([FromBody] SaveUserAddressRequest request)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息。" });
                }

                if (request.IsDefault)
                {
                    await ClearDefaultAddressAsync(userId);
                }

                var item = new UserAddress
                {
                    UserId = userId,
                    ConsigneeName = request.ConsigneeName.Trim(),
                    Mobile = request.Mobile.Trim(),
                    Province = request.Province?.Trim(),
                    City = request.City?.Trim(),
                    Area = request.Area?.Trim(),
                    RegionCode = request.RegionCode?.Trim(),
                    DetailAddress = request.DetailAddress.Trim(),
                    PostalCode = request.PostalCode?.Trim(),
                    Label = request.Label?.Trim(),
                    IsDefault = request.IsDefault,
                    Sort = request.Sort,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                _dbContext.UserAddresses.Add(item);
                await _dbContext.SaveChangesAsync();

                return Ok(MapAddress.Compile().Invoke(item));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "新增地址失败。");
                return BadRequest(new { message = "新增地址失败。" });
            }
        }

        [HttpPut("addresses/{addressId}")]
        public async Task<IActionResult> UpdateAddress(string addressId, [FromBody] SaveUserAddressRequest request)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息。" });
                }

                var item = await _dbContext.UserAddresses.FirstOrDefaultAsync(row => row.UserId == userId && row.Id == addressId);
                if (item == null)
                {
                    return NotFound(new { message = "地址不存在。" });
                }

                if (request.IsDefault)
                {
                    await ClearDefaultAddressAsync(userId, addressId);
                }

                item.ConsigneeName = request.ConsigneeName.Trim();
                item.Mobile = request.Mobile.Trim();
                item.Province = request.Province?.Trim();
                item.City = request.City?.Trim();
                item.Area = request.Area?.Trim();
                item.RegionCode = request.RegionCode?.Trim();
                item.DetailAddress = request.DetailAddress.Trim();
                item.PostalCode = request.PostalCode?.Trim();
                item.Label = request.Label?.Trim();
                item.IsDefault = request.IsDefault;
                item.Sort = request.Sort;
                item.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();
                return Ok(MapAddress.Compile().Invoke(item));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新地址失败：{AddressId}", addressId);
                return BadRequest(new { message = "更新地址失败。" });
            }
        }

        [HttpPost("addresses/{addressId}/default")]
        public async Task<IActionResult> SetDefaultAddress(string addressId)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息。" });
                }

                var item = await _dbContext.UserAddresses.FirstOrDefaultAsync(row => row.UserId == userId && row.Id == addressId);
                if (item == null)
                {
                    return NotFound(new { message = "地址不存在。" });
                }

                await ClearDefaultAddressAsync(userId, addressId);
                item.IsDefault = true;
                item.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "默认地址设置成功。" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置默认地址失败：{AddressId}", addressId);
                return BadRequest(new { message = "设置默认地址失败。" });
            }
        }

        [HttpDelete("addresses/{addressId}")]
        public async Task<IActionResult> DeleteAddress(string addressId)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息。" });
                }

                var item = await _dbContext.UserAddresses.FirstOrDefaultAsync(row => row.UserId == userId && row.Id == addressId);
                if (item == null)
                {
                    return NotFound(new { message = "地址不存在。" });
                }

                _dbContext.UserAddresses.Remove(item);
                await _dbContext.SaveChangesAsync();

                if (item.IsDefault)
                {
                    var nextDefault = await _dbContext.UserAddresses
                        .Where(row => row.UserId == userId)
                        .OrderByDescending(row => row.Sort)
                        .ThenByDescending(row => row.CreatedAt)
                        .FirstOrDefaultAsync();

                    if (nextDefault != null)
                    {
                        nextDefault.IsDefault = true;
                        nextDefault.UpdatedAt = DateTime.UtcNow;
                        await _dbContext.SaveChangesAsync();
                    }
                }

                return Ok(new { message = "地址删除成功。" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除地址失败：{AddressId}", addressId);
                return BadRequest(new { message = "删除地址失败。" });
            }
        }

        private async Task ClearDefaultAddressAsync(string userId, string? excludeAddressId = null)
        {
            var addresses = await _dbContext.UserAddresses
                .Where(item => item.UserId == userId && item.IsDefault && (excludeAddressId == null || item.Id != excludeAddressId))
                .ToListAsync();

            foreach (var item in addresses)
            {
                item.IsDefault = false;
                item.UpdatedAt = DateTime.UtcNow;
            }
        }

        private static string ResolveMemberLevel(decimal totalSpent)
        {
            if (totalSpent >= 50000m)
            {
                return "钻石会员";
            }

            if (totalSpent >= 10000m)
            {
                return "铂金会员";
            }

            if (totalSpent >= 5000m)
            {
                return "金卡会员";
            }

            if (totalSpent >= 1000m)
            {
                return "银卡会员";
            }

            return "普通会员";
        }

        private static readonly System.Linq.Expressions.Expression<Func<UserAddress, UserAddressDto>> MapAddress = item => new UserAddressDto
        {
            Id = item.Id,
            ConsigneeName = item.ConsigneeName,
            Mobile = item.Mobile,
            Province = item.Province,
            City = item.City,
            Area = item.Area,
            RegionCode = item.RegionCode,
            DetailAddress = item.DetailAddress,
            PostalCode = item.PostalCode,
            Label = item.Label,
            IsDefault = item.IsDefault,
            Sort = item.Sort,
            FullAddress = (item.Province ?? "")
                + (((item.Province ?? "") != "" && (item.City ?? "") != "") ? " " : "")
                + (item.City ?? "")
                + ((((item.Province ?? "") != "" || (item.City ?? "") != "") && (item.Area ?? "") != "") ? " " : "")
                + (item.Area ?? "")
                + (((((item.Province ?? "") != "" || (item.City ?? "") != "") || (item.Area ?? "") != "") && item.DetailAddress != "") ? " " : "")
                + item.DetailAddress,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
        };
    }

    public sealed class MallAccountSummaryDto
    {
        public string UserId { get; set; } = string.Empty;
        public string? NickName { get; set; }
        public string? RealName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Picture { get; set; }
        public string MemberLevel { get; set; } = "普通会员";
        public decimal TotalSpent { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int AvailableCouponCount { get; set; }
        public UserAddressDto? DefaultAddress { get; set; }
    }

    public sealed class UserAddressDto
    {
        public string Id { get; set; } = string.Empty;
        public string ConsigneeName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string? Province { get; set; }
        public string? City { get; set; }
        public string? Area { get; set; }
        public string? RegionCode { get; set; }
        public string DetailAddress { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        public string? Label { get; set; }
        public bool IsDefault { get; set; }
        public int Sort { get; set; }
        public string FullAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public sealed class SaveUserAddressRequest
    {
        public string ConsigneeName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string? Province { get; set; }
        public string? City { get; set; }
        public string? Area { get; set; }
        public string? RegionCode { get; set; }
        public string DetailAddress { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        public string? Label { get; set; }
        public bool IsDefault { get; set; }
        public int Sort { get; set; } = 100;
    }
}
