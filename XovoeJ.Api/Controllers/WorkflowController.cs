using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XovoeJ.Abstractions;
using XovoeJ.Abstractions.Services;
using XovoeJ.Api.Swaggers;
using XovoeJ.Entities;

namespace XovoeJ.Api.Controllers
{
    [ApiController]
    [Route("api/admin/workflow")]
    [Route("api/workflow")]
    [Produces("application/json")]
    [ApiGroup(ApiGroupNames.MANAGEMENT)]
    [Authorize]
    public class WorkflowController : ControllerBase
    {
        private readonly IWorkflowService _workflowService;
        private readonly ILogger<WorkflowController> _logger;

        public WorkflowController(IWorkflowService workflowService, ILogger<WorkflowController> logger)
        {
            _workflowService = workflowService;
            _logger = logger;
        }

        [HttpPost("definitions")]
        public async Task<IActionResult> CreateDefinition([FromBody] CreateWorkflowDefinitionRequest request)
        {
            try
            {
                var definition = await _workflowService.CreateDefinitionAsync(request);
                return Ok(definition);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create workflow definition.");
                return BadRequest(new { message = "创建工作流定义失败。" });
            }
        }

        [HttpGet("definitions")]
        public async Task<IActionResult> GetDefinitions([FromQuery] string? type = null)
        {
            try
            {
                var definitions = await _workflowService.GetDefinitionsAsync(type);
                return Ok(definitions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load workflow definitions.");
                return BadRequest(new { message = "加载工作流定义列表失败。" });
            }
        }

        [HttpGet("definitions/{code}")]
        public async Task<IActionResult> GetDefinition(string code)
        {
            try
            {
                var definition = await _workflowService.GetDefinitionAsync(code);
                if (definition == null)
                {
                    return NotFound(new { message = "工作流定义不存在。" });
                }

                return Ok(definition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load workflow definition: {Code}", code);
                return BadRequest(new { message = "加载工作流定义失败。" });
            }
        }

        [HttpPut("definitions/{code}")]
        public async Task<IActionResult> UpdateDefinition(string code, [FromBody] CreateWorkflowDefinitionRequest request)
        {
            try
            {
                var definition = await _workflowService.UpdateDefinitionAsync(code, request);
                if (definition == null)
                {
                    return NotFound(new { message = "工作流定义不存在。" });
                }

                return Ok(definition);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update workflow definition: {Code}", code);
                return BadRequest(new { message = "更新工作流定义失败。" });
            }
        }

        [HttpPost("definitions/{code}/status")]
        public async Task<IActionResult> UpdateDefinitionStatus(string code, [FromBody] UpdateWorkflowDefinitionStatusRequest request)
        {
            try
            {
                var result = await _workflowService.SetDefinitionEnabledAsync(code, request.IsEnabled);
                if (!result)
                {
                    return NotFound(new { message = "工作流定义不存在。" });
                }

                return Ok(new
                {
                    message = request.IsEnabled ? "工作流已启用。" : "工作流已禁用。",
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update workflow definition status: {Code}", code);
                return BadRequest(new { message = "更新工作流状态失败。" });
            }
        }

        [HttpDelete("definitions/{code}")]
        public async Task<IActionResult> DeleteDefinition(string code)
        {
            try
            {
                var result = await _workflowService.DeleteDefinitionAsync(code);
                if (!result)
                {
                    return NotFound(new { message = "工作流定义不存在。" });
                }

                return Ok(new { message = "工作流定义已删除。" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete workflow definition: {Code}", code);
                return BadRequest(new { message = "删除工作流定义失败。" });
            }
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartWorkflow([FromBody] StartWorkflowRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "当前用户身份无效。" });
                }

                var userName = User.FindFirstValue("username") ?? User.FindFirstValue("name") ?? string.Empty;
                var instance = await _workflowService.StartWorkflowAsync(userId, userName, request);

                _logger.LogInformation(
                    "Workflow started. InstanceId={InstanceId}, WorkflowCode={WorkflowCode}, UserId={UserId}",
                    instance.Id,
                    request.WorkflowCode,
                    userId);

                return Ok(instance);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start workflow.");
                return BadRequest(new { message = "发起工作流失败。" });
            }
        }

        [HttpGet("instances/{instanceId}")]
        public async Task<IActionResult> GetInstance(string instanceId)
        {
            try
            {
                var instance = await _workflowService.GetInstanceAsync(instanceId);
                if (instance == null)
                {
                    return NotFound(new { message = "工作流实例不存在。" });
                }

                return Ok(instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load workflow instance: {InstanceId}", instanceId);
                return BadRequest(new { message = "加载工作流实例失败。" });
            }
        }

        [HttpGet("instances/my")]
        public async Task<IActionResult> GetMyInstances(
            [FromQuery] string? workflowType = null,
            [FromQuery] WorkflowStatus? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "当前用户身份无效。" });
                }

                var (items, total) = await _workflowService.GetMyInstancesAsync(userId, workflowType, status, page, pageSize);
                return Ok(new
                {
                    items,
                    total,
                    page,
                    pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load my workflow instances.");
                return BadRequest(new { message = "加载我的工作流列表失败。" });
            }
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingTasks(
            [FromQuery] string? workflowType = null,
            [FromQuery] WorkflowStatus? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "当前用户身份无效。" });
                }

                var result = await _workflowService.GetPendingTasksAsync(userId, new PendingTasksQuery
                {
                    WorkflowType = workflowType,
                    Status = status,
                    Page = page,
                    PageSize = pageSize
                });

                return Ok(new
                {
                    items = result.Items,
                    total = result.Total,
                    page = result.Page,
                    pageSize = result.PageSize,
                    statusCount = result.StatusCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load pending workflow tasks.");
                return BadRequest(new { message = "加载待办工作流失败。" });
            }
        }

        [HttpGet("pending/count")]
        public async Task<IActionResult> GetPendingCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "当前用户身份无效。" });
                }

                var counts = await _workflowService.GetPendingCountAsync(userId);
                return Ok(counts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load pending workflow counts.");
                return BadRequest(new { message = "加载待办统计失败。" });
            }
        }

        [HttpPost("instances/{instanceId}/approve")]
        public async Task<IActionResult> ProcessApproval(string instanceId, [FromBody] ApprovalRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "当前用户身份无效。" });
                }

                var userName = User.FindFirstValue("username") ?? User.FindFirstValue("name") ?? string.Empty;
                await _workflowService.ProcessApprovalAsync(userId, userName, instanceId, request);

                _logger.LogInformation(
                    "Workflow approval processed. InstanceId={InstanceId}, UserId={UserId}, Action={Action}",
                    instanceId,
                    userId,
                    request.Action);

                return Ok(new { message = "审批处理成功。" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process workflow approval: {InstanceId}", instanceId);
                return BadRequest(new { message = "处理审批失败。" });
            }
        }

        [HttpPost("instances/{instanceId}/withdraw")]
        public async Task<IActionResult> WithdrawWorkflow(string instanceId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "当前用户身份无效。" });
                }

                var result = await _workflowService.WithdrawWorkflowAsync(userId, instanceId);
                if (!result)
                {
                    return NotFound(new { message = "工作流实例不存在。" });
                }

                _logger.LogInformation("Workflow withdrawn. InstanceId={InstanceId}, UserId={UserId}", instanceId, userId);
                return Ok(new { message = "工作流已撤回。" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to withdraw workflow: {InstanceId}", instanceId);
                return BadRequest(new { message = "撤回工作流失败。" });
            }
        }

        [HttpPost("instances/{instanceId}/terminate")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> TerminateWorkflow(string instanceId, [FromBody] TerminateRequest? request = null)
        {
            try
            {
                var result = await _workflowService.TerminateWorkflowAsync(instanceId, request?.Reason);
                if (!result)
                {
                    return NotFound(new { message = "工作流实例不存在。" });
                }

                _logger.LogInformation("Workflow terminated. InstanceId={InstanceId}, Reason={Reason}", instanceId, request?.Reason);
                return Ok(new { message = "工作流已终止。" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to terminate workflow: {InstanceId}", instanceId);
                return BadRequest(new { message = "终止工作流失败。" });
            }
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }

    public class TerminateRequest
    {
        public string? Reason { get; set; }
    }

    public class UpdateWorkflowDefinitionStatusRequest
    {
        public bool IsEnabled { get; set; }
    }
}
