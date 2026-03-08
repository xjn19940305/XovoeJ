using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XovoeJ.Api.Swaggers;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Api.Managements
{
    [ApiController]
    [Route("api/admin/message")]
    [Produces("application/json")]
    [Authorize]
    [ApiGroup(ApiGroupNames.MANAGEMENT)]
    public class AdminMessageController : ControllerBase
    {
        private readonly XovoeJDbContext _dbContext;
        private readonly ILogger<AdminMessageController> _logger;

        public AdminMessageController(XovoeJDbContext dbContext, ILogger<AdminMessageController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? keyword = null,
            [FromQuery] string? channel = null,
            [FromQuery] int? status = null)
        {
            try
            {
                var query = _dbContext.MessageTemplates.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(item =>
                        item.Name.Contains(keyword)
                        || item.Code.Contains(keyword)
                        || (item.BusinessType != null && item.BusinessType.Contains(keyword)));
                }

                if (!string.IsNullOrWhiteSpace(channel))
                {
                    query = query.Where(item => item.Channel == channel);
                }

                if (status.HasValue)
                {
                    query = query.Where(item => item.Status == status.Value);
                }

                var total = await query.CountAsync();
                var items = await query
                    .OrderByDescending(item => item.UpdatedAt ?? item.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(item => new MessageTemplateDto
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Code = item.Code,
                        Channel = item.Channel,
                        BusinessType = item.BusinessType,
                        Subject = item.Subject,
                        ContentPreview = item.ContentPreview,
                        Description = item.Description,
                        Status = item.Status,
                        CreatedAt = item.CreatedAt,
                        UpdatedAt = item.UpdatedAt,
                    })
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
                _logger.LogError(ex, "Failed to load message templates.");
                return BadRequest(new { message = "Failed to load message templates." });
            }
        }

        [HttpGet("templates/{templateId}")]
        public async Task<IActionResult> GetTemplateById(string templateId)
        {
            try
            {
                var template = await _dbContext.MessageTemplates
                    .AsNoTracking()
                    .Where(item => item.Id == templateId)
                    .Select(item => new MessageTemplateDto
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Code = item.Code,
                        Channel = item.Channel,
                        BusinessType = item.BusinessType,
                        Subject = item.Subject,
                        ContentPreview = item.ContentPreview,
                        Description = item.Description,
                        Status = item.Status,
                        CreatedAt = item.CreatedAt,
                        UpdatedAt = item.UpdatedAt,
                    })
                    .FirstOrDefaultAsync();

                if (template == null)
                {
                    return NotFound(new { message = "Message template not found." });
                }

                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load message template detail: {TemplateId}", templateId);
                return BadRequest(new { message = "Failed to load message template detail." });
            }
        }

        [HttpGet("tasks")]
        public async Task<IActionResult> GetTasks(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? keyword = null,
            [FromQuery] string? channel = null,
            [FromQuery] int? status = null)
        {
            try
            {
                var query =
                    from task in _dbContext.MessageTasks.AsNoTracking()
                    join template in _dbContext.MessageTemplates.AsNoTracking() on task.TemplateId equals template.Id into templates
                    from template in templates.DefaultIfEmpty()
                    select new { task, template };

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(item =>
                        item.task.Name.Contains(keyword)
                        || (item.template != null && item.template.Name.Contains(keyword)));
                }

                if (!string.IsNullOrWhiteSpace(channel))
                {
                    query = query.Where(item => item.task.Channel == channel);
                }

                if (status.HasValue)
                {
                    query = query.Where(item => item.task.Status == status.Value);
                }

                var total = await query.CountAsync();
                var items = await query
                    .OrderByDescending(item => item.task.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(item => new MessageTaskDto
                    {
                        Id = item.task.Id,
                        Name = item.task.Name,
                        TemplateName = item.template != null ? item.template.Name : null,
                        Channel = item.task.Channel,
                        TriggerType = item.task.TriggerType,
                        RecipientCount = item.task.RecipientCount,
                        SuccessCount = item.task.SuccessCount,
                        FailedCount = item.task.FailedCount,
                        Status = item.task.Status,
                        ScheduledAt = item.task.ScheduledAt,
                        SentAt = item.task.SentAt,
                        CreatedAt = item.task.CreatedAt,
                    })
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
                _logger.LogError(ex, "Failed to load message tasks.");
                return BadRequest(new { message = "Failed to load message tasks." });
            }
        }

        [HttpGet("tasks/{taskId}")]
        public async Task<IActionResult> GetTaskById(string taskId)
        {
            try
            {
                var task = await (
                    from item in _dbContext.MessageTasks.AsNoTracking()
                    join template in _dbContext.MessageTemplates.AsNoTracking() on item.TemplateId equals template.Id into templates
                    from template in templates.DefaultIfEmpty()
                    where item.Id == taskId
                    select new MessageTaskDto
                    {
                        Id = item.Id,
                        Name = item.Name,
                        TemplateName = template != null ? template.Name : null,
                        Channel = item.Channel,
                        TriggerType = item.TriggerType,
                        RecipientCount = item.RecipientCount,
                        SuccessCount = item.SuccessCount,
                        FailedCount = item.FailedCount,
                        Status = item.Status,
                        ScheduledAt = item.ScheduledAt,
                        SentAt = item.SentAt,
                        CreatedAt = item.CreatedAt,
                    })
                    .FirstOrDefaultAsync();

                if (task == null)
                {
                    return NotFound(new { message = "Message task not found." });
                }

                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load message task detail: {TaskId}", taskId);
                return BadRequest(new { message = "Failed to load message task detail." });
            }
        }

        [HttpGet("records")]
        public async Task<IActionResult> GetRecords(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? keyword = null,
            [FromQuery] string? channel = null,
            [FromQuery] int? status = null)
        {
            try
            {
                var query =
                    from record in _dbContext.MessageSendRecords.AsNoTracking()
                    join template in _dbContext.MessageTemplates.AsNoTracking() on record.TemplateId equals template.Id into templates
                    from template in templates.DefaultIfEmpty()
                    join task in _dbContext.MessageTasks.AsNoTracking() on record.TaskId equals task.Id into tasks
                    from task in tasks.DefaultIfEmpty()
                    select new { record, template, task };

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(item =>
                        (item.record.Recipient != null && item.record.Recipient.Contains(keyword))
                        || (item.record.TraceId != null && item.record.TraceId.Contains(keyword))
                        || (item.template != null && item.template.Name.Contains(keyword))
                        || (item.task != null && item.task.Name.Contains(keyword)));
                }

                if (!string.IsNullOrWhiteSpace(channel))
                {
                    query = query.Where(item => item.record.Channel == channel);
                }

                if (status.HasValue)
                {
                    query = query.Where(item => item.record.Status == status.Value);
                }

                var total = await query.CountAsync();
                var items = await query
                    .OrderByDescending(item => item.record.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(item => new MessageRecordDto
                    {
                        Id = item.record.Id,
                        TemplateName = item.template != null ? item.template.Name : null,
                        TaskName = item.task != null ? item.task.Name : null,
                        Channel = item.record.Channel,
                        Recipient = item.record.Recipient,
                        BusinessType = item.record.BusinessType,
                        TraceId = item.record.TraceId,
                        ErrorMessage = item.record.ErrorMessage,
                        Status = item.record.Status,
                        SentAt = item.record.SentAt,
                        CreatedAt = item.record.CreatedAt,
                    })
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
                _logger.LogError(ex, "Failed to load message send records.");
                return BadRequest(new { message = "Failed to load message send records." });
            }
        }

        [HttpGet("records/{recordId}")]
        public async Task<IActionResult> GetRecordById(string recordId)
        {
            try
            {
                var record = await (
                    from item in _dbContext.MessageSendRecords.AsNoTracking()
                    join template in _dbContext.MessageTemplates.AsNoTracking() on item.TemplateId equals template.Id into templates
                    from template in templates.DefaultIfEmpty()
                    join task in _dbContext.MessageTasks.AsNoTracking() on item.TaskId equals task.Id into tasks
                    from task in tasks.DefaultIfEmpty()
                    where item.Id == recordId
                    select new MessageRecordDto
                    {
                        Id = item.Id,
                        TemplateName = template != null ? template.Name : null,
                        TaskName = task != null ? task.Name : null,
                        Channel = item.Channel,
                        Recipient = item.Recipient,
                        BusinessType = item.BusinessType,
                        TraceId = item.TraceId,
                        ErrorMessage = item.ErrorMessage,
                        Status = item.Status,
                        SentAt = item.SentAt,
                        CreatedAt = item.CreatedAt,
                    })
                    .FirstOrDefaultAsync();

                if (record == null)
                {
                    return NotFound(new { message = "Message send record not found." });
                }

                return Ok(record);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load message send record detail: {RecordId}", recordId);
                return BadRequest(new { message = "Failed to load message send record detail." });
            }
        }
    }

    public sealed class MessageTemplateDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string? BusinessType { get; set; }
        public string? Subject { get; set; }
        public string? ContentPreview { get; set; }
        public string? Description { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public sealed class MessageTaskDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? TemplateName { get; set; }
        public string Channel { get; set; } = string.Empty;
        public string? TriggerType { get; set; }
        public int RecipientCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int Status { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public sealed class MessageRecordDto
    {
        public string Id { get; set; } = string.Empty;
        public string? TemplateName { get; set; }
        public string? TaskName { get; set; }
        public string Channel { get; set; } = string.Empty;
        public string? Recipient { get; set; }
        public string? BusinessType { get; set; }
        public string? TraceId { get; set; }
        public string? ErrorMessage { get; set; }
        public int Status { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
