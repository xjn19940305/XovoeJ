using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using XovoeJ.Abstractions;
using XovoeJ.Abstractions.Services;
using XovoeJ.Entities;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Workflow.Services
{
    /// <summary>
    /// 工作流服务实现
    /// </summary>
    public class WorkflowService : IWorkflowService
    {
        private readonly XovoeJDbContext _dbContext;
        private readonly ILogger<WorkflowService> _logger;

        public WorkflowService(
            XovoeJDbContext dbContext,
            ILogger<WorkflowService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        #region 工作流定义管理

        public async Task<WorkflowDefinitionDto> CreateDefinitionAsync(CreateWorkflowDefinitionRequest request)
        {
            // 生成工作流编码
            var code = $"WF_{request.Type.ToUpper()}_{DateTime.UtcNow:yyyyMMddHHmmss}_{new Random().Next(1000, 9999)}";

            var definition = new XovoeJ.Entities.WorkflowDefinition
            {
                Code = code,
                Name = request.Name,
                Description = request.Description,
                Type = request.Type,
                Version = 1,
                IsEnabled = true,
                StepsConfig = JsonSerializer.Serialize(request.Steps),
                FormConfig = request.FormConfig != null ? JsonSerializer.Serialize(request.FormConfig) : null,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.WorkflowDefinitions.Add(definition);
            await _dbContext.SaveChangesAsync();

            return await GetDefinitionAsync(code);
        }

        public async Task<WorkflowDefinitionDto?> UpdateDefinitionAsync(string code, CreateWorkflowDefinitionRequest request)
        {
            var definition = await _dbContext.WorkflowDefinitions.FirstOrDefaultAsync(d => d.Code == code);
            if (definition == null)
            {
                return null;
            }

            definition.Name = request.Name;
            definition.Description = request.Description;
            definition.StepsConfig = JsonSerializer.Serialize(request.Steps);
            definition.FormConfig = request.FormConfig != null ? JsonSerializer.Serialize(request.FormConfig) : null;
            definition.Version += 1;
            definition.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return await GetDefinitionAsync(code);
        }

        public async Task<WorkflowDefinitionDto?> GetDefinitionAsync(string code)
        {
            var definition = await _dbContext.WorkflowDefinitions.AsNoTracking()
                .FirstOrDefaultAsync(d => d.Code == code);

            if (definition == null) return null;

            var steps = string.IsNullOrEmpty(definition.StepsConfig)
                ? new List<WorkflowStepDefinition>()
                : JsonSerializer.Deserialize<List<WorkflowStepDefinition>>(definition.StepsConfig);

            var formConfig = string.IsNullOrEmpty(definition.FormConfig)
                ? null
                : JsonSerializer.Deserialize<object>(definition.FormConfig);

            return new WorkflowDefinitionDto
            {
                Code = definition.Code,
                Name = definition.Name,
                Description = definition.Description,
                Type = definition.Type,
                Version = definition.Version,
                IsEnabled = definition.IsEnabled,
                Steps = steps,
                FormConfig = formConfig,
                CreatedAt = definition.CreatedAt,
                UpdatedAt = definition.UpdatedAt,
                UsageCount = _dbContext.WorkflowInstances.Count(i => i.WorkflowCode == code)
            };
        }

        public async Task<List<WorkflowDefinitionDto>> GetDefinitionsAsync(string? type = null)
        {
            var query = _dbContext.WorkflowDefinitions.AsNoTracking();

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(d => d.Type == type);
            }

            var definitions = await query.OrderByDescending(d => d.CreatedAt).ToListAsync();

            return definitions.Select(d =>
            {
                var steps = string.IsNullOrEmpty(d.StepsConfig)
                    ? new List<WorkflowStepDefinition>()
                    : JsonSerializer.Deserialize<List<WorkflowStepDefinition>>(d.StepsConfig);

                var formConfig = string.IsNullOrEmpty(d.FormConfig)
                    ? null
                    : JsonSerializer.Deserialize<object>(d.FormConfig);

                return new WorkflowDefinitionDto
                {
                    Code = d.Code,
                    Name = d.Name,
                    Description = d.Description,
                    Type = d.Type,
                    Version = d.Version,
                    IsEnabled = d.IsEnabled,
                    Steps = steps,
                    FormConfig = formConfig,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt,
                    UsageCount = 0
                };
            }).ToList();
        }

        public async Task<bool> DeleteDefinitionAsync(string code)
        {
            var definition = await _dbContext.WorkflowDefinitions.FirstOrDefaultAsync(d => d.Code == code);
            if (definition == null) return false;

            // 检查是否有正在运行的实例
            var hasRunningInstances = await _dbContext.WorkflowInstances
                .AnyAsync(i => i.WorkflowCode == code && i.Status != WorkflowStatus.Completed && i.Status != WorkflowStatus.Rejected && i.Status != WorkflowStatus.Terminated);

            if (hasRunningInstances)
            {
                throw new InvalidOperationException("该工作流有正在运行的实例，无法删除");
            }

            _dbContext.WorkflowDefinitions.Remove(definition);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SetDefinitionEnabledAsync(string code, bool enabled)
        {
            var definition = await _dbContext.WorkflowDefinitions.FirstOrDefaultAsync(d => d.Code == code);
            if (definition == null) return false;

            definition.IsEnabled = enabled;
            await _dbContext.SaveChangesAsync();

            return true;
        }

        #endregion

        #region 工作流实例管理

        public async Task<WorkflowInstanceDto> StartWorkflowAsync(string userId, string userName, StartWorkflowRequest request)
        {
            // 获取工作流定义
            var definition = await _dbContext.WorkflowDefinitions
                .FirstOrDefaultAsync(d => d.Code == request.WorkflowCode && d.IsEnabled);

            if (definition == null)
            {
                throw new ArgumentException("工作流不存在或已禁用");
            }

            // 解析步骤配置
            var steps = string.IsNullOrEmpty(definition.StepsConfig)
                ? new List<WorkflowStepDefinition>()
                : JsonSerializer.Deserialize<List<WorkflowStepDefinition>>(definition.StepsConfig);

            if (steps == null || steps.Count == 0)
            {
                throw new ArgumentException("工作流配置错误：没有定义审批步骤");
            }

            // 获取第一步
            var firstStep = steps.OrderBy(s => s.Order).First();

            // 创建工作流实例
            var instance = new XovoeJ.Entities.WorkflowInstance
            {
                WorkflowCode = definition.Code,
                WorkflowName = definition.Name,
                InitiatorId = userId,
                InitiatorName = userName,
                Title = request.Title,
                BusinessKey = request.BusinessKey,
                BusinessType = request.BusinessType ?? definition.Type,
                CurrentStepId = firstStep.Id,
                CurrentStepName = firstStep.Name,
                Status = WorkflowStatus.InProgress,
                FormData = request.FormData != null ? JsonSerializer.Serialize(request.FormData) : null,
                Variables = request.Variables,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.WorkflowInstances.Add(instance);
            await _dbContext.SaveChangesAsync();

            // 创建待办记录
            foreach (var approverId in firstStep.ApproverIds)
            {
                _dbContext.WorkflowPendingItems.Add(new XovoeJ.Entities.WorkflowPendingItem
                {
                    InstanceId = instance.Id,
                    StepId = firstStep.Id,
                    StepName = firstStep.Name,
                    ApproverId = approverId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _dbContext.SaveChangesAsync();

            return await GetInstanceAsync(instance.Id);
        }

        public async Task<WorkflowInstanceDto?> GetInstanceAsync(string instanceId)
        {
            var instance = await _dbContext.WorkflowInstances
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == instanceId);

            if (instance == null) return null;

            // 获取审批记录
            var approvalRecords = await _dbContext.WorkflowApprovalRecords
                .Where(r => r.InstanceId == instanceId)
                .OrderBy(r => r.ActionTime)
                .ToListAsync();

            // 按步骤分组
            var completedSteps = approvalRecords
                .GroupBy(r => r.StepId)
                .Select(g => new CompletedStepDto
                {
                    StepId = g.Key,
                    StepName = g.First().StepName,
                    Approvals = g.Select(r => new ApprovalRecordDto
                    {
                        Id = r.Id,
                        StepId = r.StepId,
                        StepName = r.StepName,
                        ApproverId = r.ApproverId,
                        ApproverName = r.ApproverName,
                        Action = r.Action,
                        Comment = r.Comment,
                        Attachments = string.IsNullOrEmpty(r.Attachments) ? null : JsonSerializer.Deserialize<List<string>>(r.Attachments),
                        ActionTime = r.ActionTime
                    }).ToList(),
                    CompletedAt = g.Max(r => r.ActionTime)
                }).ToList();

            // 获取当前待办人数
            var pendingCount = await _dbContext.WorkflowPendingItems
                .CountAsync(p => p.InstanceId == instanceId);

            return new WorkflowInstanceDto
            {
                Id = instance.Id,
                WorkflowCode = instance.WorkflowCode,
                WorkflowName = instance.WorkflowName,
                InitiatorId = instance.InitiatorId,
                InitiatorName = instance.InitiatorName,
                Title = instance.Title,
                BusinessKey = instance.BusinessKey,
                BusinessType = instance.BusinessType,
                CurrentStepId = instance.CurrentStepId,
                CurrentStepName = instance.CurrentStepName,
                Status = instance.Status,
                FormData = string.IsNullOrEmpty(instance.FormData) ? null : JsonSerializer.Deserialize<object>(instance.FormData),
                Variables = instance.Variables,
                PendingCount = pendingCount,
                CompletedSteps = completedSteps,
                CreatedAt = instance.CreatedAt,
                UpdatedAt = instance.UpdatedAt,
                CompletedAt = instance.CompletedAt
            };
        }

        public async Task<(List<WorkflowInstanceDto> Items, int Total)> GetMyInstancesAsync(
            string userId,
            string? workflowType = null,
            WorkflowStatus? status = null,
            int page = 1,
            int pageSize = 20)
        {
            var query = _dbContext.WorkflowInstances
                .AsNoTracking()
                .Where(i => i.InitiatorId == userId);

            if (!string.IsNullOrEmpty(workflowType))
            {
                query = query.Where(i => i.BusinessType == workflowType);
            }

            if (status.HasValue)
            {
                query = query.Where(i => i.Status == status.Value);
            }

            var total = await query.CountAsync();

            var instances = await query
                .OrderByDescending(i => i.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = await BatchMapToInstanceDtosAsync(instances);

            return (items, total);
        }

        public async Task<PendingTasksResponse> GetPendingTasksAsync(string userId, PendingTasksQuery query)
        {
            // 获取当前用户需要审批的实例ID列表
            var pendingItemQuery = _dbContext.WorkflowPendingItems
                .AsNoTracking()
                .Where(p => p.ApproverId == userId);

            var instanceIds = pendingItemQuery.Select(p => p.InstanceId).Distinct().ToList();

            var instancesQuery = _dbContext.WorkflowInstances
                .AsNoTracking()
                .Where(i => instanceIds.Contains(i.Id));

            // 类型筛选
            if (!string.IsNullOrEmpty(query.WorkflowType))
            {
                instancesQuery = instancesQuery.Where(i => i.BusinessType == query.WorkflowType);
            }

            // 状态筛选
            if (query.Status.HasValue)
            {
                instancesQuery = instancesQuery.Where(i => i.Status == query.Status.Value);
            }

            var total = await instancesQuery.CountAsync();

            var instances = await instancesQuery
                .OrderByDescending(i => i.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var items = await BatchMapToInstanceDtosAsync(instances);

            // 状态统计
            var statusCount = await instancesQuery
                .GroupBy(i => (int)i.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status.ToString(), x => x.Count);

            return new PendingTasksResponse
            {
                Items = items,
                Total = total,
                StatusCount = statusCount,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }

        public async Task<Dictionary<string, int>> GetPendingCountAsync(string userId)
        {
            var instanceIds = await _dbContext.WorkflowPendingItems
                .AsNoTracking()
                .Where(p => p.ApproverId == userId)
                .Select(p => p.InstanceId)
                .Distinct()
                .ToListAsync();

            var counts = await _dbContext.WorkflowInstances
                .AsNoTracking()
                .Where(i => instanceIds.Contains(i.Id) && i.Status == WorkflowStatus.InProgress)
                .GroupBy(i => i.BusinessType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);

            return counts;
        }

        public async Task ProcessApprovalAsync(string userId, string userName, string instanceId, ApprovalRequest request)
        {
            // 获取实例
            var instance = await _dbContext.WorkflowInstances
                .FirstOrDefaultAsync(i => i.Id == instanceId);

            if (instance == null)
            {
                throw new ArgumentException("工作流实例不存在");
            }

            if (instance.Status != WorkflowStatus.InProgress)
            {
                throw new InvalidOperationException("当前状态不允许审批");
            }

            // 获取当前步骤配置
            var definition = await _dbContext.WorkflowDefinitions
                .FirstOrDefaultAsync(d => d.Code == instance.WorkflowCode);
            var steps = string.IsNullOrEmpty(definition?.StepsConfig)
                ? new List<WorkflowStepDefinition>()
                : JsonSerializer.Deserialize<List<WorkflowStepDefinition>>(definition.StepsConfig);

            var currentStep = steps?.FirstOrDefault(s => s.Id == instance.CurrentStepId);
            if (currentStep == null)
            {
                throw new InvalidOperationException("当前步骤配置不存在");
            }

            // 检查是否有审批权限
            if (!currentStep.ApproverIds.Contains(userId))
            {
                throw new UnauthorizedAccessException("您没有权限审批此工作流");
            }

            // 记录审批
            var record = new XovoeJ.Entities.WorkflowApprovalRecord
            {
                InstanceId = instanceId,
                StepId = currentStep.Id,
                StepName = currentStep.Name,
                ApproverId = userId,
                ApproverName = userName,
                Action = request.Action,
                Comment = request.Comment,
                Attachments = request.Attachments != null ? JsonSerializer.Serialize(request.Attachments) : null,
                ActionTime = DateTime.UtcNow
            };

            _dbContext.WorkflowApprovalRecords.Add(record);

            // 处理审批结果
            if (request.Action == ApprovalAction.Approve)
            {
                await HandleApproveAsync(instance, currentStep, steps, userId);
            }
            else if (request.Action == ApprovalAction.Reject)
            {
                await HandleRejectAsync(instance, userId);
            }
            else if (request.Action == ApprovalAction.Withdraw)
            {
                await HandleWithdrawAsync(instance, userId);
            }
            else if (request.Action == ApprovalAction.Forward)
            {
                // 转交处理
                if (!string.IsNullOrEmpty(request.ForwardToUserId))
                {
                    var pendingItem = await _dbContext.WorkflowPendingItems
                        .FirstOrDefaultAsync(p => p.InstanceId == instanceId && p.ApproverId == userId);

                    if (pendingItem != null)
                    {
                        _dbContext.WorkflowPendingItems.Remove(pendingItem);
                        _dbContext.WorkflowPendingItems.Add(new XovoeJ.Entities.WorkflowPendingItem
                        {
                            InstanceId = instanceId,
                            StepId = currentStep.Id,
                            StepName = currentStep.Name,
                            ApproverId = request.ForwardToUserId,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            instance.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> WithdrawWorkflowAsync(string userId, string instanceId)
        {
            var instance = await _dbContext.WorkflowInstances
                .FirstOrDefaultAsync(i => i.Id == instanceId);

            if (instance == null) return false;

            if (instance.InitiatorId != userId)
            {
                throw new UnauthorizedAccessException("只有发起人可以撤回工作流");
            }

            if (instance.Status != WorkflowStatus.InProgress)
            {
                throw new InvalidOperationException("当前状态不允许撤回");
            }

            // 检查当前步骤是否允许撤回
            var definition = await _dbContext.WorkflowDefinitions
                .FirstOrDefaultAsync(d => d.Code == instance.WorkflowCode);
            var steps = string.IsNullOrEmpty(definition?.StepsConfig)
                ? new List<WorkflowStepDefinition>()
                : JsonSerializer.Deserialize<List<WorkflowStepDefinition>>(definition.StepsConfig);
            var currentStep = steps?.FirstOrDefault(s => s.Id == instance.CurrentStepId);

            if (currentStep != null && !currentStep.AllowWithdraw)
            {
                throw new InvalidOperationException("当前步骤不允许撤回");
            }

            // 删除待办
            await _dbContext.WorkflowPendingItems
                .Where(p => p.InstanceId == instanceId)
                .ExecuteDeleteAsync();

            instance.Status = WorkflowStatus.Withdrawn;
            instance.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> TerminateWorkflowAsync(string instanceId, string? reason = null)
        {
            var instance = await _dbContext.WorkflowInstances
                .FirstOrDefaultAsync(i => i.Id == instanceId);

            if (instance == null) return false;

            // 删除待办
            await _dbContext.WorkflowPendingItems
                .Where(p => p.InstanceId == instanceId)
                .ExecuteDeleteAsync();

            instance.Status = WorkflowStatus.Terminated;
            instance.UpdatedAt = DateTime.UtcNow;
            instance.CompletedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 批量将实例实体转换为 DTO（避免 N+1 查询）
        /// </summary>
        private async Task<List<WorkflowInstanceDto>> BatchMapToInstanceDtosAsync(List<XovoeJ.Entities.WorkflowInstance> instances)
        {
            if (instances.Count == 0) return new List<WorkflowInstanceDto>();

            var instanceIds = instances.Select(i => i.Id).ToList();

            // 一次性加载所有审批记录
            var allApprovalRecords = await _dbContext.WorkflowApprovalRecords
                .Where(r => instanceIds.Contains(r.InstanceId))
                .OrderBy(r => r.ActionTime)
                .ToListAsync();

            // 一次性加载所有待办数量
            var pendingCounts = await _dbContext.WorkflowPendingItems
                .Where(p => instanceIds.Contains(p.InstanceId))
                .GroupBy(p => p.InstanceId)
                .Select(g => new { InstanceId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.InstanceId, x => x.Count);

            var recordsByInstance = allApprovalRecords
                .GroupBy(r => r.InstanceId)
                .ToDictionary(g => g.Key, g => g.ToList());

            return instances.Select(instance =>
            {
                var records = recordsByInstance.GetValueOrDefault(instance.Id, new List<XovoeJ.Entities.WorkflowApprovalRecord>());
                var completedSteps = records
                    .GroupBy(r => r.StepId)
                    .Select(g => new CompletedStepDto
                    {
                        StepId = g.Key,
                        StepName = g.First().StepName,
                        Approvals = g.Select(r => new ApprovalRecordDto
                        {
                            Id = r.Id,
                            StepId = r.StepId,
                            StepName = r.StepName,
                            ApproverId = r.ApproverId,
                            ApproverName = r.ApproverName,
                            Action = r.Action,
                            Comment = r.Comment,
                            Attachments = string.IsNullOrEmpty(r.Attachments) ? null : JsonSerializer.Deserialize<List<string>>(r.Attachments),
                            ActionTime = r.ActionTime
                        }).ToList(),
                        CompletedAt = g.Max(r => r.ActionTime)
                    }).ToList();

                pendingCounts.TryGetValue(instance.Id, out var pendingCount);

                return new WorkflowInstanceDto
                {
                    Id = instance.Id,
                    WorkflowCode = instance.WorkflowCode,
                    WorkflowName = instance.WorkflowName,
                    InitiatorId = instance.InitiatorId,
                    InitiatorName = instance.InitiatorName,
                    Title = instance.Title,
                    BusinessKey = instance.BusinessKey,
                    BusinessType = instance.BusinessType,
                    CurrentStepId = instance.CurrentStepId,
                    CurrentStepName = instance.CurrentStepName,
                    Status = instance.Status,
                    FormData = string.IsNullOrEmpty(instance.FormData) ? null : JsonSerializer.Deserialize<object>(instance.FormData),
                    Variables = instance.Variables,
                    PendingCount = pendingCount,
                    CompletedSteps = completedSteps,
                    CreatedAt = instance.CreatedAt,
                    UpdatedAt = instance.UpdatedAt,
                    CompletedAt = instance.CompletedAt
                };
            }).ToList();
        }

        /// <summary>
        /// 处理审批通过
        /// </summary>
        private async Task HandleApproveAsync(
            XovoeJ.Entities.WorkflowInstance instance,
            WorkflowStepDefinition currentStep,
            List<WorkflowStepDefinition> steps,
            string userId)
        {
            // 只删除当前审批人的待办（而非所有人的待办）
            var currentUserPendingItem = await _dbContext.WorkflowPendingItems
                .FirstOrDefaultAsync(p => p.InstanceId == instance.Id && p.StepId == currentStep.Id && p.ApproverId == userId);

            if (currentUserPendingItem != null)
            {
                _dbContext.WorkflowPendingItems.Remove(currentUserPendingItem);
            }

            // 检查审批规则（注意：当前审批人的记录已在调用方添加到 ApprovalRecords）
            var allApprovals = await _dbContext.WorkflowApprovalRecords
                .Where(r => r.InstanceId == instance.Id && r.StepId == currentStep.Id && r.Action == ApprovalAction.Approve)
                .Select(r => r.ApproverId)
                .Distinct()
                .ToListAsync();

            // 加上当前审批人（尚未 SaveChanges，所以手动计入）
            if (!allApprovals.Contains(userId))
            {
                allApprovals.Add(userId);
            }

            bool stepCompleted = currentStep.ApprovalRule switch
            {
                ApprovalRule.AllApprove => allApprovals.Count >= currentStep.ApproverIds.Count,
                ApprovalRule.AnyApprove => allApprovals.Count > 0,
                ApprovalRule.Percentage => (double)allApprovals.Count / currentStep.ApproverIds.Count >= 0.5,
                _ => allApprovals.Count > 0
            };

            if (!stepCompleted)
            {
                // 未完成，继续等待其他审批人
                return;
            }

            // 步骤完成，删除该步骤剩余的所有待办
            var remainingPendingItems = await _dbContext.WorkflowPendingItems
                .Where(p => p.InstanceId == instance.Id && p.StepId == currentStep.Id)
                .ToListAsync();
            _dbContext.WorkflowPendingItems.RemoveRange(remainingPendingItems);

            // 查找下一步骤
            var nextStep = steps
                .Where(s => s.Order > currentStep.Order)
                .OrderBy(s => s.Order)
                .FirstOrDefault(s => s.Type == WorkflowStepType.Approval);

            if (nextStep == null)
            {
                // 没有下一步骤，流程结束
                instance.Status = WorkflowStatus.Completed;
                instance.CurrentStepId = null;
                instance.CurrentStepName = null;
                instance.CompletedAt = DateTime.UtcNow;
            }
            else
            {
                // 进入下一步骤
                instance.CurrentStepId = nextStep.Id;
                instance.CurrentStepName = nextStep.Name;

                // 创建下一步的待办
                foreach (var approverId in nextStep.ApproverIds)
                {
                    _dbContext.WorkflowPendingItems.Add(new XovoeJ.Entities.WorkflowPendingItem
                    {
                        InstanceId = instance.Id,
                        StepId = nextStep.Id,
                        StepName = nextStep.Name,
                        ApproverId = approverId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        /// <summary>
        /// 处理审批拒绝
        /// </summary>
        private async Task HandleRejectAsync(XovoeJ.Entities.WorkflowInstance instance, string userId)
        {
            // 删除所有待办
            await _dbContext.WorkflowPendingItems
                .Where(p => p.InstanceId == instance.Id)
                .ExecuteDeleteAsync();

            instance.Status = WorkflowStatus.Rejected;
            instance.CurrentStepId = null;
            instance.CurrentStepName = null;
            instance.CompletedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// 处理撤回
        /// </summary>
        private async Task HandleWithdrawAsync(XovoeJ.Entities.WorkflowInstance instance, string userId)
        {
            // 撤回逻辑在 WithdrawWorkflowAsync 中处理
            await Task.CompletedTask;
        }

        #endregion
    }
}
