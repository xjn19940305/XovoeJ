using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XovoeJ.Api.Swaggers;
using XovoeJ.Entities;
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
                _logger.LogError(ex, "加载消息模板列表失败。");
                return BadRequest(new { message = "加载消息模板列表失败。" });
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
                    return NotFound(new { message = "消息模板不存在。" });
                }

                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载消息模板详情失败：{TemplateId}", templateId);
                return BadRequest(new { message = "加载消息模板详情失败。" });
            }
        }

        [HttpPost("templates/{templateId}/status")]
        public async Task<IActionResult> UpdateTemplateStatus(string templateId, [FromBody] UpdateMessageStatusRequest request)
        {
            try
            {
                if (request.Status is < 0 or > 2)
                {
                    return BadRequest(new { message = "消息模板状态不合法。" });
                }

                var template = await _dbContext.MessageTemplates.FirstOrDefaultAsync(item => item.Id == templateId);
                if (template == null)
                {
                    return NotFound(new { message = "消息模板不存在。" });
                }

                template.Status = request.Status;
                template.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "消息模板状态更新成功。" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新消息模板状态失败：{TemplateId}", templateId);
                return BadRequest(new { message = "更新消息模板状态失败。" });
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
                _logger.LogError(ex, "加载消息任务列表失败。");
                return BadRequest(new { message = "加载消息任务列表失败。" });
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
                    return NotFound(new { message = "消息任务不存在。" });
                }

                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载消息任务详情失败：{TaskId}", taskId);
                return BadRequest(new { message = "加载消息任务详情失败。" });
            }
        }

        [HttpPost("tasks/{taskId}/send")]
        public async Task<IActionResult> SendTask(string taskId)
        {
            try
            {
                var task = await _dbContext.MessageTasks
                    .Include(item => item.Template)
                    .Include(item => item.Records)
                    .FirstOrDefaultAsync(item => item.Id == taskId);
                if (task == null)
                {
                    return NotFound(new { message = "消息任务不存在。" });
                }

                if (task.Status == 2)
                {
                    return BadRequest(new { message = "消息任务已发送完成，无需重复执行。" });
                }

                if (task.TemplateId != null && task.Template?.Status != 1)
                {
                    return BadRequest(new { message = "关联模板未启用，无法执行发送。" });
                }

                var now = DateTime.UtcNow;
                if (task.Records.Count == 0)
                {
                    _dbContext.MessageSendRecords.Add(new MessageSendRecord
                    {
                        TemplateId = task.TemplateId,
                        TaskId = task.Id,
                        Channel = task.Channel,
                        Recipient = $"batch:{task.RecipientCount}",
                        BusinessType = task.Template?.BusinessType,
                        TraceId = $"msg-{Guid.NewGuid():N}"[..20],
                        Status = 1,
                        SentAt = now,
                        CreatedAt = now,
                        UpdatedAt = now,
                    });
                }
                else
                {
                    foreach (var record in task.Records)
                    {
                        record.Status = 1;
                        record.ErrorMessage = null;
                        record.SentAt = now;
                        record.UpdatedAt = now;
                        record.TraceId ??= $"msg-{Guid.NewGuid():N}"[..20];
                    }
                }

                task.SuccessCount = task.RecipientCount;
                task.FailedCount = 0;
                task.Status = 2;
                task.SentAt = now;
                task.UpdatedAt = now;

                await _dbContext.SaveChangesAsync();
                return Ok(new { message = "消息任务执行成功。" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行消息任务失败：{TaskId}", taskId);
                return BadRequest(new { message = "执行消息任务失败。" });
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
                _logger.LogError(ex, "加载发送记录列表失败。");
                return BadRequest(new { message = "加载发送记录列表失败。" });
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
                    return NotFound(new { message = "发送记录不存在。" });
                }

                return Ok(record);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载发送记录详情失败：{RecordId}", recordId);
                return BadRequest(new { message = "加载发送记录详情失败。" });
            }
        }

        [HttpPost("records/{recordId}/retry")]
        public async Task<IActionResult> RetryRecord(string recordId)
        {
            try
            {
                var record = await _dbContext.MessageSendRecords
                    .Include(item => item.Template)
                    .Include(item => item.Task)
                    .FirstOrDefaultAsync(item => item.Id == recordId);
                if (record == null)
                {
                    return NotFound(new { message = "发送记录不存在。" });
                }

                if (record.Status == 1)
                {
                    return BadRequest(new { message = "该发送记录已送达，无需重试。" });
                }

                if (record.TemplateId != null && record.Template?.Status != 1)
                {
                    return BadRequest(new { message = "关联模板未启用，无法重试发送。" });
                }

                var now = DateTime.UtcNow;
                record.Status = 1;
                record.ErrorMessage = null;
                record.SentAt = now;
                record.UpdatedAt = now;
                record.TraceId ??= $"msg-{Guid.NewGuid():N}"[..20];

                if (record.TaskId != null)
                {
                    var relatedRecords = await _dbContext.MessageSendRecords
                        .Where(item => item.TaskId == record.TaskId)
                        .ToListAsync();

                    var successCount = relatedRecords.Count(item => item.Status == 1);
                    var failedCount = relatedRecords.Count(item => item.Status == 2);
                    var pendingCount = relatedRecords.Count(item => item.Status == 0);

                    if (record.Task != null)
                    {
                        record.Task.SuccessCount = successCount;
                        record.Task.FailedCount = failedCount;
                        record.Task.Status = failedCount > 0
                            ? 3
                            : pendingCount > 0
                                ? 1
                                : 2;
                        if (record.Task.Status == 2)
                        {
                            record.Task.SentAt ??= now;
                        }
                        record.Task.UpdatedAt = now;
                    }
                }

                await _dbContext.SaveChangesAsync();
                return Ok(new { message = "发送记录重试成功。" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重试发送记录失败：{RecordId}", recordId);
                return BadRequest(new { message = "发送记录重试失败。" });
            }
        }
    }

    public sealed class UpdateMessageStatusRequest
    {
        public int Status { get; set; }
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
