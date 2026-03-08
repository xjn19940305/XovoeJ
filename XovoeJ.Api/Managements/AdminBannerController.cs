using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XovoeJ.Api.Swaggers;
using XovoeJ.Entities;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Api.Managements
{
    [ApiController]
    [Route("api/admin/content/banners")]
    [Produces("application/json")]
    [Authorize]
    [ApiGroup(ApiGroupNames.MANAGEMENT)]
    public class AdminBannerController : ControllerBase
    {
        private readonly XovoeJDbContext _dbContext;
        private readonly ILogger<AdminBannerController> _logger;

        public AdminBannerController(XovoeJDbContext dbContext, ILogger<AdminBannerController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? keyword = null,
            [FromQuery] bool? isEnabled = null)
        {
            try
            {
                var query = _dbContext.Banners.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(item =>
                        item.Title.Contains(keyword)
                        || (item.Subtitle != null && item.Subtitle.Contains(keyword))
                        || (item.LinkUrl != null && item.LinkUrl.Contains(keyword)));
                }

                if (isEnabled.HasValue)
                {
                    query = query.Where(item => item.IsEnabled == isEnabled.Value);
                }

                var total = await query.CountAsync();
                var items = await query
                    .OrderBy(item => item.SortOrder)
                    .ThenByDescending(item => item.UpdatedAt ?? item.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(item => MapDto(item))
                    .ToListAsync();

                return Ok(new
                {
                    items,
                    total,
                    page,
                    pageSize,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载轮播图列表失败。");
                return BadRequest(new { message = "加载轮播图列表失败。" });
            }
        }

        [HttpGet("{bannerId}")]
        public async Task<IActionResult> GetById(string bannerId)
        {
            try
            {
                var item = await _dbContext.Banners.AsNoTracking()
                    .Where(row => row.Id == bannerId)
                    .Select(row => MapDto(row))
                    .FirstOrDefaultAsync();

                if (item == null)
                {
                    return NotFound(new { message = "轮播图不存在。" });
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载轮播图详情失败：{BannerId}", bannerId);
                return BadRequest(new { message = "加载轮播图详情失败。" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveBannerRequest request)
        {
            try
            {
                ValidateRequest(request);

                var entity = new Banner
                {
                    Title = request.Title.Trim(),
                    Subtitle = request.Subtitle?.Trim(),
                    ImageUrl = request.ImageUrl.Trim(),
                    LinkUrl = request.LinkUrl?.Trim(),
                    SortOrder = request.SortOrder,
                    IsEnabled = request.IsEnabled,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Remark = request.Remark?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                };

                _dbContext.Banners.Add(entity);
                await _dbContext.SaveChangesAsync();

                return Ok(MapDto(entity));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建轮播图失败。");
                return BadRequest(new { message = "创建轮播图失败。" });
            }
        }

        [HttpPut("{bannerId}")]
        public async Task<IActionResult> Update(string bannerId, [FromBody] SaveBannerRequest request)
        {
            try
            {
                ValidateRequest(request);

                var entity = await _dbContext.Banners.FirstOrDefaultAsync(item => item.Id == bannerId);
                if (entity == null)
                {
                    return NotFound(new { message = "轮播图不存在。" });
                }

                entity.Title = request.Title.Trim();
                entity.Subtitle = request.Subtitle?.Trim();
                entity.ImageUrl = request.ImageUrl.Trim();
                entity.LinkUrl = request.LinkUrl?.Trim();
                entity.SortOrder = request.SortOrder;
                entity.IsEnabled = request.IsEnabled;
                entity.StartTime = request.StartTime;
                entity.EndTime = request.EndTime;
                entity.Remark = request.Remark?.Trim();
                entity.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                return Ok(MapDto(entity));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新轮播图失败：{BannerId}", bannerId);
                return BadRequest(new { message = "更新轮播图失败。" });
            }
        }

        [HttpPost("{bannerId}/status")]
        public async Task<IActionResult> UpdateStatus(string bannerId, [FromBody] UpdateBannerStatusRequest request)
        {
            try
            {
                var entity = await _dbContext.Banners.FirstOrDefaultAsync(item => item.Id == bannerId);
                if (entity == null)
                {
                    return NotFound(new { message = "轮播图不存在。" });
                }

                entity.IsEnabled = request.IsEnabled;
                entity.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = request.IsEnabled ? "轮播图已启用。" : "轮播图已禁用。",
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新轮播图状态失败：{BannerId}", bannerId);
                return BadRequest(new { message = "更新轮播图状态失败。" });
            }
        }

        [HttpDelete("{bannerId}")]
        public async Task<IActionResult> Delete(string bannerId)
        {
            try
            {
                var entity = await _dbContext.Banners.FirstOrDefaultAsync(item => item.Id == bannerId);
                if (entity == null)
                {
                    return NotFound(new { message = "轮播图不存在。" });
                }

                _dbContext.Banners.Remove(entity);
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "轮播图已删除。" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除轮播图失败：{BannerId}", bannerId);
                return BadRequest(new { message = "删除轮播图失败。" });
            }
        }

        private static BannerDto MapDto(Banner entity)
        {
            return new BannerDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Subtitle = entity.Subtitle,
                ImageUrl = entity.ImageUrl,
                LinkUrl = entity.LinkUrl,
                SortOrder = entity.SortOrder,
                IsEnabled = entity.IsEnabled,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                Remark = entity.Remark,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
            };
        }

        private static void ValidateRequest(SaveBannerRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new ArgumentException("轮播图标题不能为空。");
            }

            if (string.IsNullOrWhiteSpace(request.ImageUrl))
            {
                throw new ArgumentException("轮播图图片地址不能为空。");
            }

            if (request.StartTime.HasValue && request.EndTime.HasValue && request.EndTime < request.StartTime)
            {
                throw new ArgumentException("结束时间不能早于开始时间。");
            }
        }
    }

    public sealed class SaveBannerRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Subtitle { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public string? LinkUrl { get; set; }

        public int SortOrder { get; set; }

        public bool IsEnabled { get; set; } = true;

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string? Remark { get; set; }
    }

    public sealed class UpdateBannerStatusRequest
    {
        public bool IsEnabled { get; set; }
    }

    public sealed class BannerDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? LinkUrl { get; set; }
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Remark { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
