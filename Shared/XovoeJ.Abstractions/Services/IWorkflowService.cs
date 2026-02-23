using XovoeJ.Entities;

namespace XovoeJ.Abstractions.Services
{
    /// <summary>
    /// 工作流服务接口
    /// </summary>
    public interface IWorkflowService
    {
        #region 工作流定义管理

        /// <summary>
        /// 创建工作流定义
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>工作流定义</returns>
        Task<WorkflowDefinitionDto> CreateDefinitionAsync(CreateWorkflowDefinitionRequest request);

        /// <summary>
        /// 更新工作流定义
        /// </summary>
        /// <param name="code">工作流编码</param>
        /// <param name="request">更新请求</param>
        /// <returns>更新后的工作流定义</returns>
        Task<WorkflowDefinitionDto?> UpdateDefinitionAsync(string code, CreateWorkflowDefinitionRequest request);

        /// <summary>
        /// 获取工作流定义
        /// </summary>
        /// <param name="code">工作流编码</param>
        /// <returns>工作流定义</returns>
        Task<WorkflowDefinitionDto?> GetDefinitionAsync(string code);

        /// <summary>
        /// 获取工作流定义列表
        /// </summary>
        /// <param name="type">工作流类型（可选）</param>
        /// <returns>工作流定义列表</returns>
        Task<List<WorkflowDefinitionDto>> GetDefinitionsAsync(string? type = null);

        /// <summary>
        /// 删除工作流定义
        /// </summary>
        /// <param name="code">工作流编码</param>
        /// <returns></returns>
        Task<bool> DeleteDefinitionAsync(string code);

        /// <summary>
        /// 启用/禁用工作流定义
        /// </summary>
        /// <param name="code">工作流编码</param>
        /// <param name="enabled">是否启用</param>
        /// <returns></returns>
        Task<bool> SetDefinitionEnabledAsync(string code, bool enabled);

        #endregion

        #region 工作流实例管理

        /// <summary>
        /// 发起工作流
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="userName">用户姓名</param>
        /// <param name="request">发起请求</param>
        /// <returns>工作流实例</returns>
        Task<WorkflowInstanceDto> StartWorkflowAsync(string userId, string userName, StartWorkflowRequest request);

        /// <summary>
        /// 获取工作流实例
        /// </summary>
        /// <param name="instanceId">实例ID</param>
        /// <returns>工作流实例</returns>
        Task<WorkflowInstanceDto?> GetInstanceAsync(string instanceId);

        /// <summary>
        /// 获取我发起的工作流列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="workflowType">工作流类型</param>
        /// <param name="status">状态</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <returns>工作流实例列表</returns>
        Task<(List<WorkflowInstanceDto> Items, int Total)> GetMyInstancesAsync(
            string userId,
            string? workflowType = null,
            WorkflowStatus? status = null,
            int page = 1,
            int pageSize = 20);

        /// <summary>
        /// 获取待办任务列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="query">查询条件</param>
        /// <returns>待办列表</returns>
        Task<PendingTasksResponse> GetPendingTasksAsync(string userId, PendingTasksQuery query);

        /// <summary>
        /// 获取待办数量统计
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>各状态待办数量</returns>
        Task<Dictionary<string, int>> GetPendingCountAsync(string userId);

        /// <summary>
        /// 处理审批
        /// </summary>
        /// <param name="userId">审批人ID</param>
        /// <param name="userName">审批人姓名</param>
        /// <param name="instanceId">实例ID</param>
        /// <param name="request">审批请求</param>
        /// <returns></returns>
        Task ProcessApprovalAsync(string userId, string userName, string instanceId, ApprovalRequest request);

        /// <summary>
        /// 撤回工作流
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="instanceId">实例ID</param>
        /// <returns></returns>
        Task<bool> WithdrawWorkflowAsync(string userId, string instanceId);

        /// <summary>
        /// 终止工作流（管理员操作）
        /// </summary>
        /// <param name="instanceId">实例ID</param>
        /// <param name="reason">终止原因</param>
        /// <returns></returns>
        Task<bool> TerminateWorkflowAsync(string instanceId, string? reason = null);

        #endregion
    }
}
