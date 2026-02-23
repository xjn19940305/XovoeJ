using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XovoeJ.Abstractions;
using XovoeJ.Abstractions.Services;
using XovoeJ.Api.Swaggers;
using XovoeJ.Entities;

namespace XovoeJ.Api.Controllers
{
    /// <summary>
    /// 工作流控制器
    /// </summary>
    [ApiController]
    [Route("api/workflow")]
    [Produces("application/json")]
    [ApiGroup(ApiGroupNames.MANAGEMENT)]
    [Authorize]
    public class WorkflowController : ControllerBase
    {
        private readonly IWorkflowService _workflowService;
        private readonly ILogger<WorkflowController> _logger;

        public WorkflowController(
            IWorkflowService workflowService,
            ILogger<WorkflowController> logger)
        {
            _workflowService = workflowService;
            _logger = logger;
        }

        #region 工作流定义管理（管理员）

        /// <summary>
        /// 创建工作流定义
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>工作流定义</returns>
        [HttpPost("definitions")]
        public async Task<IActionResult> CreateDefinition([FromBody] CreateWorkflowDefinitionRequest request)
        {
            try
            {
                var definition = await _workflowService.CreateDefinitionAsync(request);
                return Ok(new { data = definition, message = "创建成功" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建工作流定义失败");
                return BadRequest(new { message = "创建失败" });
            }
        }

        /// <summary>
        /// 获取工作流定义列表
        /// </summary>
        /// <param name="type">工作流类型（可选）</param>
        /// <returns>工作流定义列表</returns>
        [HttpGet("definitions")]
        public async Task<IActionResult> GetDefinitions([FromQuery] string? type = null)
        {
            try
            {
                var definitions = await _workflowService.GetDefinitionsAsync(type);
                return Ok(new { data = definitions, message = "获取成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取工作流定义列表失败");
                return BadRequest(new { message = "获取失败" });
            }
        }

        /// <summary>
        /// 获取工作流定义详情
        /// </summary>
        /// <param name="code">工作流编码</param>
        /// <returns>工作流定义详情</returns>
        [HttpGet("definitions/{code}")]
        public async Task<IActionResult> GetDefinition(string code)
        {
            try
            {
                var definition = await _workflowService.GetDefinitionAsync(code);
                if (definition == null)
                {
                    return NotFound(new { message = "工作流定义不存在" });
                }
                return Ok(new { data = definition, message = "获取成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取工作流定义失败: {Code}", code);
                return BadRequest(new { message = "获取失败" });
            }
        }

        /// <summary>
        /// 更新工作流定义
        /// </summary>
        /// <param name="code">工作流编码</param>
        /// <param name="request">更新请求</param>
        /// <returns>更新后的工作流定义</returns>
        [HttpPut("definitions/{code}")]
        public async Task<IActionResult> UpdateDefinition(string code, [FromBody] CreateWorkflowDefinitionRequest request)
        {
            try
            {
                var definition = await _workflowService.UpdateDefinitionAsync(code, request);
                if (definition == null)
                {
                    return NotFound(new { message = "工作流定义不存在" });
                }
                return Ok(new { data = definition, message = "更新成功" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新工作流定义失败: {Code}", code);
                return BadRequest(new { message = "更新失败" });
            }
        }

        /// <summary>
        /// 删除工作流定义
        /// </summary>
        /// <param name="code">工作流编码</param>
        /// <returns></returns>
        [HttpDelete("definitions/{code}")]
        public async Task<IActionResult> DeleteDefinition(string code)
        {
            try
            {
                var result = await _workflowService.DeleteDefinitionAsync(code);
                if (!result)
                {
                    return NotFound(new { message = "工作流定义不存在" });
                }
                return Ok(new { message = "删除成功" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除工作流定义失败: {Code}", code);
                return BadRequest(new { message = "删除失败" });
            }
        }

        #endregion

        #region 工作流实例管理

        /// <summary>
        /// 发起工作流
        /// </summary>
        /// <param name="request">发起请求</param>
        /// <returns>工作流实例</returns>
        [HttpPost("start")]
        public async Task<IActionResult> StartWorkflow([FromBody] StartWorkflowRequest request)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var userName = User.FindFirstValue("username") ?? User.FindFirstValue("name");

                var instance = await _workflowService.StartWorkflowAsync(userId, userName ?? "", request);

                _logger.LogInformation("工作流已发起: InstanceId={InstanceId}, WorkflowCode={WorkflowCode}, UserId={UserId}",
                    instance.Id, request.WorkflowCode, userId);

                return Ok(new { data = instance, message = "发起成功" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发起工作流失败");
                return BadRequest(new { message = "发起失败" });
            }
        }

        /// <summary>
        /// 获取工作流实例详情
        /// </summary>
        /// <param name="instanceId">实例ID</param>
        /// <returns>工作流实例详情</returns>
        [HttpGet("instances/{instanceId}")]
        public async Task<IActionResult> GetInstance(string instanceId)
        {
            try
            {
                var instance = await _workflowService.GetInstanceAsync(instanceId);
                if (instance == null)
                {
                    return NotFound(new { message = "工作流实例不存在" });
                }
                return Ok(new { data = instance, message = "获取成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取工作流实例失败: {InstanceId}", instanceId);
                return BadRequest(new { message = "获取失败" });
            }
        }

        /// <summary>
        /// 获取我发起的工作流列表
        /// </summary>
        /// <param name="workflowType">工作流类型（可选）</param>
        /// <param name="status">状态（可选）</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <returns>工作流列表</returns>
        [HttpGet("instances/my")]
        public async Task<IActionResult> GetMyInstances(
            [FromQuery] string? workflowType = null,
            [FromQuery] WorkflowStatus? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var (items, total) = await _workflowService.GetMyInstancesAsync(userId, workflowType, status, page, pageSize);

                return Ok(new
                {
                    data = items,
                    total,
                    page,
                    pageSize,
                    message = "获取成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取我的工作流列表失败");
                return BadRequest(new { message = "获取失败" });
            }
        }

        /// <summary>
        /// 获取待办任务列表
        /// </summary>
        /// <param name="workflowType">工作流类型（可选）</param>
        /// <param name="status">状态（可选）</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <returns>待办列表</returns>
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingTasks(
            [FromQuery] string? workflowType = null,
            [FromQuery] WorkflowStatus? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var query = new PendingTasksQuery
                {
                    WorkflowType = workflowType,
                    Status = status,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _workflowService.GetPendingTasksAsync(userId, query);

                return Ok(new { data = result, message = "获取成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取待办列表失败");
                return BadRequest(new { message = "获取失败" });
            }
        }

        /// <summary>
        /// 获取待办数量统计
        /// </summary>
        /// <returns>待办数量统计</returns>
        [HttpGet("pending/count")]
        public async Task<IActionResult> GetPendingCount()
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var counts = await _workflowService.GetPendingCountAsync(userId);

                return Ok(new { data = counts, message = "获取成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取待办数量失败");
                return BadRequest(new { message = "获取失败" });
            }
        }

        /// <summary>
        /// 处理审批
        /// </summary>
        /// <param name="instanceId">实例ID</param>
        /// <param name="request">审批请求</param>
        /// <returns></returns>
        [HttpPost("instances/{instanceId}/approve")]
        public async Task<IActionResult> ProcessApproval(string instanceId, [FromBody] ApprovalRequest request)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var userName = User.FindFirstValue("username") ?? User.FindFirstValue("name");

                await _workflowService.ProcessApprovalAsync(userId, userName ?? "", instanceId, request);

                _logger.LogInformation("审批处理完成: InstanceId={InstanceId}, UserId={UserId}, Action={Action}",
                    instanceId, userId, request.Action);

                return Ok(new { message = "审批成功" });
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
                _logger.LogError(ex, "处理审批失败: {InstanceId}", instanceId);
                return BadRequest(new { message = "处理失败" });
            }
        }

        /// <summary>
        /// 撤回工作流
        /// </summary>
        /// <param name="instanceId">实例ID</param>
        /// <returns></returns>
        [HttpPost("instances/{instanceId}/withdraw")]
        public async Task<IActionResult> WithdrawWorkflow(string instanceId)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var result = await _workflowService.WithdrawWorkflowAsync(userId, instanceId);
                if (!result)
                {
                    return NotFound(new { message = "工作流实例不存在" });
                }

                _logger.LogInformation("工作流已撤回: InstanceId={InstanceId}, UserId={UserId}", instanceId, userId);

                return Ok(new { message = "撤回成功" });
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
                _logger.LogError(ex, "撤回工作流失败: {InstanceId}", instanceId);
                return BadRequest(new { message = "撤回失败" });
            }
        }

        /// <summary>
        /// 终止工作流（管理员）
        /// </summary>
        /// <param name="instanceId">实例ID</param>
        /// <param name="reason">终止原因</param>
        /// <returns></returns>
        [HttpPost("instances/{instanceId}/terminate")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> TerminateWorkflow(string instanceId, [FromBody] TerminateRequest? request = null)
        {
            try
            {
                var result = await _workflowService.TerminateWorkflowAsync(instanceId, request?.Reason);
                if (!result)
                {
                    return NotFound(new { message = "工作流实例不存在" });
                }

                _logger.LogInformation("工作流已终止: InstanceId={InstanceId}, Reason={Reason}", instanceId, request?.Reason);

                return Ok(new { message = "工作流已终止" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "终止工作流失败: {InstanceId}", instanceId);
                return BadRequest(new { message = "终止失败" });
            }
        }
        #endregion
    }

    /// <summary>
    /// 终止工作流请求
    /// </summary>
    public class TerminateRequest
    {
        /// <summary>
        /// 终止原因
        /// </summary>
        public string? Reason { get; set; }
    }
}
