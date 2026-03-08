using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XovoeJ.Api.Swaggers;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Api.Managements
{
    [ApiController]
    [Route("api/admin/assets")]
    [Produces("application/json")]
    [Authorize]
    [ApiGroup(ApiGroupNames.MANAGEMENT)]
    public class AdminAssetController : ControllerBase
    {
        private readonly XovoeJDbContext _dbContext;
        private readonly ILogger<AdminAssetController> _logger;

        public AdminAssetController(XovoeJDbContext dbContext, ILogger<AdminAssetController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet("wallets")]
        public async Task<IActionResult> GetWallets([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? keyword = null)
        {
            try
            {
                var query =
                    from wallet in _dbContext.WalletAccounts.AsNoTracking()
                    join user in _dbContext.Users.AsNoTracking() on wallet.UserId equals user.Id into users
                    from user in users.DefaultIfEmpty()
                    select new { wallet, user };

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(item =>
                        item.wallet.UserId.Contains(keyword)
                        || (item.user != null && (
                            (item.user.UserName != null && item.user.UserName.Contains(keyword))
                            || (item.user.NickName != null && item.user.NickName.Contains(keyword))
                            || (item.user.PhoneNumber != null && item.user.PhoneNumber.Contains(keyword)))));
                }

                var total = await query.CountAsync();
                var items = await query
                    .OrderByDescending(item => item.wallet.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(item => new AdminWalletAccountDto
                    {
                        Id = item.wallet.Id,
                        UserId = item.wallet.UserId,
                        UserName = item.user != null ? (item.user.NickName ?? item.user.UserName ?? item.user.PhoneNumber) : null,
                        AvailableBalance = item.wallet.AvailableBalance,
                        FrozenBalance = item.wallet.FrozenBalance,
                        TotalIncome = item.wallet.TotalIncome,
                        TotalExpense = item.wallet.TotalExpense,
                        LastChangedAt = item.wallet.LastChangedAt,
                        CreatedAt = item.wallet.CreatedAt,
                        UpdatedAt = item.wallet.UpdatedAt,
                    })
                    .ToListAsync();

                return Ok(new { items, total, page, pageSize });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载钱包账户列表失败");
                return BadRequest(new { message = "加载钱包账户列表失败" });
            }
        }

        [HttpGet("wallet-transactions")]
        public async Task<IActionResult> GetWalletTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? keyword = null, [FromQuery] string? businessType = null)
        {
            try
            {
                var query =
                    from transaction in _dbContext.WalletTransactions.AsNoTracking()
                    join user in _dbContext.Users.AsNoTracking() on transaction.UserId equals user.Id into users
                    from user in users.DefaultIfEmpty()
                    select new { transaction, user };

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(item =>
                        item.transaction.BusinessNo.Contains(keyword)
                        || item.transaction.UserId.Contains(keyword)
                        || (item.user != null && (
                            (item.user.UserName != null && item.user.UserName.Contains(keyword))
                            || (item.user.NickName != null && item.user.NickName.Contains(keyword))
                            || (item.user.PhoneNumber != null && item.user.PhoneNumber.Contains(keyword)))));
                }

                if (!string.IsNullOrWhiteSpace(businessType))
                {
                    query = query.Where(item => item.transaction.BusinessType == businessType);
                }

                var total = await query.CountAsync();
                var items = await query
                    .OrderByDescending(item => item.transaction.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(item => new AdminWalletTransactionDto
                    {
                        Id = item.transaction.Id,
                        UserId = item.transaction.UserId,
                        UserName = item.user != null ? (item.user.NickName ?? item.user.UserName ?? item.user.PhoneNumber) : null,
                        BusinessType = item.transaction.BusinessType,
                        BusinessNo = item.transaction.BusinessNo,
                        Direction = item.transaction.Direction,
                        ChangeAvailableBalance = item.transaction.ChangeAvailableBalance,
                        BeforeAvailableBalance = item.transaction.BeforeAvailableBalance,
                        AfterAvailableBalance = item.transaction.AfterAvailableBalance,
                        Remark = item.transaction.Remark,
                        CreatedAt = item.transaction.CreatedAt,
                    })
                    .ToListAsync();

                return Ok(new { items, total, page, pageSize });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载钱包流水列表失败");
                return BadRequest(new { message = "加载钱包流水列表失败" });
            }
        }

        [HttpGet("points-accounts")]
        public async Task<IActionResult> GetPointsAccounts([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? keyword = null)
        {
            try
            {
                var query =
                    from account in _dbContext.PointsAccounts.AsNoTracking()
                    join user in _dbContext.Users.AsNoTracking() on account.UserId equals user.Id into users
                    from user in users.DefaultIfEmpty()
                    select new { account, user };

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(item =>
                        item.account.UserId.Contains(keyword)
                        || (item.user != null && (
                            (item.user.UserName != null && item.user.UserName.Contains(keyword))
                            || (item.user.NickName != null && item.user.NickName.Contains(keyword))
                            || (item.user.PhoneNumber != null && item.user.PhoneNumber.Contains(keyword)))));
                }

                var total = await query.CountAsync();
                var items = await query
                    .OrderByDescending(item => item.account.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(item => new AdminPointsAccountDto
                    {
                        Id = item.account.Id,
                        UserId = item.account.UserId,
                        UserName = item.user != null ? (item.user.NickName ?? item.user.UserName ?? item.user.PhoneNumber) : null,
                        AvailablePoints = item.account.AvailablePoints,
                        FrozenPoints = item.account.FrozenPoints,
                        TotalEarnedPoints = item.account.TotalEarnedPoints,
                        TotalSpentPoints = item.account.TotalSpentPoints,
                        LastChangedAt = item.account.LastChangedAt,
                        CreatedAt = item.account.CreatedAt,
                        UpdatedAt = item.account.UpdatedAt,
                    })
                    .ToListAsync();

                return Ok(new { items, total, page, pageSize });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载积分账户列表失败");
                return BadRequest(new { message = "加载积分账户列表失败" });
            }
        }

        [HttpGet("points-logs")]
        public async Task<IActionResult> GetPointsLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? keyword = null, [FromQuery] string? businessType = null)
        {
            try
            {
                var query =
                    from log in _dbContext.PointsLogs.AsNoTracking()
                    join user in _dbContext.Users.AsNoTracking() on log.UserId equals user.Id into users
                    from user in users.DefaultIfEmpty()
                    select new { log, user };

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(item =>
                        item.log.BusinessNo.Contains(keyword)
                        || item.log.UserId.Contains(keyword)
                        || (item.user != null && (
                            (item.user.UserName != null && item.user.UserName.Contains(keyword))
                            || (item.user.NickName != null && item.user.NickName.Contains(keyword))
                            || (item.user.PhoneNumber != null && item.user.PhoneNumber.Contains(keyword)))));
                }

                if (!string.IsNullOrWhiteSpace(businessType))
                {
                    query = query.Where(item => item.log.BusinessType == businessType);
                }

                var total = await query.CountAsync();
                var items = await query
                    .OrderByDescending(item => item.log.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(item => new AdminPointsLogDto
                    {
                        Id = item.log.Id,
                        UserId = item.log.UserId,
                        UserName = item.user != null ? (item.user.NickName ?? item.user.UserName ?? item.user.PhoneNumber) : null,
                        BusinessType = item.log.BusinessType,
                        BusinessNo = item.log.BusinessNo,
                        Direction = item.log.Direction,
                        ChangeAvailablePoints = item.log.ChangeAvailablePoints,
                        BeforeAvailablePoints = item.log.BeforeAvailablePoints,
                        AfterAvailablePoints = item.log.AfterAvailablePoints,
                        Remark = item.log.Remark,
                        CreatedAt = item.log.CreatedAt,
                    })
                    .ToListAsync();

                return Ok(new { items, total, page, pageSize });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载积分流水列表失败");
                return BadRequest(new { message = "加载积分流水列表失败" });
            }
        }
    }

    public sealed class AdminWalletAccountDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal FrozenBalance { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public DateTime? LastChangedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public sealed class AdminWalletTransactionDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string BusinessType { get; set; } = string.Empty;
        public string BusinessNo { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public decimal ChangeAvailableBalance { get; set; }
        public decimal BeforeAvailableBalance { get; set; }
        public decimal AfterAvailableBalance { get; set; }
        public string? Remark { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public sealed class AdminPointsAccountDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public int AvailablePoints { get; set; }
        public int FrozenPoints { get; set; }
        public int TotalEarnedPoints { get; set; }
        public int TotalSpentPoints { get; set; }
        public DateTime? LastChangedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public sealed class AdminPointsLogDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string BusinessType { get; set; } = string.Empty;
        public string BusinessNo { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public int ChangeAvailablePoints { get; set; }
        public int BeforeAvailablePoints { get; set; }
        public int AfterAvailablePoints { get; set; }
        public string? Remark { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
