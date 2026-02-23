using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using XovoeJ.Abstractions.Services;
using XovoeJ.Contracts.Dictionary;
using XovoeJ.Entities;
using XovoeJ.Enum;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Application.Services
{
    /// <summary>
    /// 字典服务实现
    /// </summary>
    public class DictionaryService : IDictionaryService
    {
        private readonly XovoeJDbContext _dbContext;
        private readonly ILogger<DictionaryService> _logger;

        public DictionaryService(
            XovoeJDbContext dbContext,
            ILogger<DictionaryService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        #region 分组管理

        /// <summary>
        /// 获取分组列表
        /// </summary>
        public async Task<List<DictionaryGroupDto>> GetGroupsAsync(DictionaryType? type = null)
        {
            var query = _dbContext.DictionaryGroups.AsQueryable();

            if (type.HasValue)
            {
                query = query.Where(g => g.Type == type.Value);
            }

            var groups = await query
                .OrderBy(g => g.SortOrder)
                .ThenBy(g => g.Name)
                .ToListAsync();

            // 统计子分组和字典项数量
            var groupIds = groups.Select(g => g.Id).ToList();
            var childrenCounts = await _dbContext.DictionaryGroups
                .Where(g => groupIds.Contains(g.ParentId!))
                .GroupBy(g => g.ParentId!)
                .Select(g => new { GroupId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.GroupId, x => x.Count);

            var itemsCounts = await _dbContext.DictionaryItems
                .Where(i => groupIds.Contains(i.GroupId))
                .GroupBy(i => i.GroupId)
                .Select(i => new { GroupId = i.Key, Count = i.Count() })
                .ToDictionaryAsync(x => x.GroupId, x => x.Count);

            return groups.Select(g => MapToGroupDto(g,
                childrenCounts.GetValueOrDefault(g.Id, 0),
                itemsCounts.GetValueOrDefault(g.Id, 0))).ToList();
        }

        /// <summary>
        /// 获取分组树
        /// </summary>
        public async Task<List<DictionaryGroupTreeDto>> GetGroupTreeAsync(DictionaryType? type = null)
        {
            var query = _dbContext.DictionaryGroups.AsQueryable();

            if (type.HasValue)
            {
                query = query.Where(g => g.Type == type.Value);
            }

            var groups = await query
                .OrderBy(g => g.SortOrder)
                .ThenBy(g => g.Name)
                .ToListAsync();

            var groupMap = groups.ToDictionary(g => g.Id, g => MapToGroupTreeDto(g));
            var rootGroups = new List<DictionaryGroupTreeDto>();

            foreach (var group in groups)
            {
                var dto = groupMap[group.Id];
                if (!string.IsNullOrEmpty(group.ParentId) && groupMap.TryGetValue(group.ParentId, out var parent))
                {
                    parent.Children.Add(dto);
                }
                else
                {
                    rootGroups.Add(dto);
                }
            }

            // 加载每个分组的字典项
            foreach (var group in groups)
            {
                if (groupMap.TryGetValue(group.Id, out var dto))
                {
                    var items = await _dbContext.DictionaryItems
                        .Where(i => i.GroupId == group.Id && i.IsEnabled)
                        .OrderBy(i => i.SortOrder)
                        .ThenBy(i => i.Name)
                        .ToListAsync();

                    dto.Items = items.Select(i => MapToItemDto(i)).ToList();
                }
            }

            return rootGroups;
        }

        /// <summary>
        /// 根据编码获取分组
        /// </summary>
        public async Task<DictionaryGroupDto?> GetGroupByCodeAsync(string code)
        {
            var group = await _dbContext.DictionaryGroups
                .FirstOrDefaultAsync(g => g.Code == code);

            if (group == null) return null;

            var childrenCount = await _dbContext.DictionaryGroups
                .CountAsync(g => g.ParentId == group.Id);

            var itemsCount = await _dbContext.DictionaryItems
                .CountAsync(i => i.GroupId == group.Id);

            return MapToGroupDto(group, childrenCount, itemsCount);
        }

        /// <summary>
        /// 根据ID获取分组
        /// </summary>
        public async Task<DictionaryGroupDto?> GetGroupByIdAsync(string groupId)
        {
            var group = await _dbContext.DictionaryGroups
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null) return null;

            var childrenCount = await _dbContext.DictionaryGroups
                .CountAsync(g => g.ParentId == group.Id);

            var itemsCount = await _dbContext.DictionaryItems
                .CountAsync(i => i.GroupId == group.Id);

            return MapToGroupDto(group, childrenCount, itemsCount);
        }

        /// <summary>
        /// 创建分组
        /// </summary>
        public async Task<DictionaryGroupDto> CreateGroupAsync(CreateDictionaryGroupRequestDto request)
        {
            // 检查编码是否重复
            if (await _dbContext.DictionaryGroups.AnyAsync(g => g.Code == request.Code))
            {
                throw new ArgumentException($"分组编码 '{request.Code}' 已存在");
            }

            // 验证父分组
            int level = 0;
            string? path = null;

            if (!string.IsNullOrEmpty(request.ParentId))
            {
                var parent = await _dbContext.DictionaryGroups.FindAsync(request.ParentId);
                if (parent == null)
                {
                    throw new ArgumentException("父分组不存在");
                }

                if (parent.Type != request.Type)
                {
                    throw new ArgumentException("父分组类型与当前分组类型不一致");
                }

                level = parent.Level + 1;
                path = string.IsNullOrEmpty(parent.Path) ? parent.Id : $"{parent.Path}/{parent.Id}";
            }

            var group = new DictionaryGroup
            {
                Code = request.Code,
                Name = request.Name,
                Description = request.Description,
                Type = request.Type,
                ParentId = request.ParentId,
                Level = level,
                Path = path,
                Icon = request.Icon,
                SortOrder = request.SortOrder,
                IsEnabled = true,
                Remark = request.Remark
            };

            _dbContext.DictionaryGroups.Add(group);
            await _dbContext.SaveChangesAsync();

            return MapToGroupDto(group, 0, 0);
        }

        /// <summary>
        /// 更新分组
        /// </summary>
        public async Task<DictionaryGroupDto?> UpdateGroupAsync(string groupId, UpdateDictionaryGroupRequestDto request)
        {
            var group = await _dbContext.DictionaryGroups.FindAsync(groupId);
            if (group == null)
            {
                return null;
            }

            if (request.Name != null)
                group.Name = request.Name;

            if (request.Description != null)
                group.Description = request.Description;

            if (request.Icon != null)
                group.Icon = request.Icon;

            if (request.SortOrder.HasValue)
                group.SortOrder = request.SortOrder.Value;

            if (request.IsEnabled.HasValue)
                group.IsEnabled = request.IsEnabled.Value;

            if (request.Remark != null)
                group.Remark = request.Remark;

            group.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            var childrenCount = await _dbContext.DictionaryGroups
                .CountAsync(g => g.ParentId == group.Id);

            var itemsCount = await _dbContext.DictionaryItems
                .CountAsync(i => i.GroupId == group.Id);

            return MapToGroupDto(group, childrenCount, itemsCount);
        }

        /// <summary>
        /// 删除分组
        /// </summary>
        public async Task<bool> DeleteGroupAsync(string groupId)
        {
            var group = await _dbContext.DictionaryGroups.FindAsync(groupId);
            if (group == null)
            {
                return false;
            }

            // 检查是否有子分组
            var hasChildren = await _dbContext.DictionaryGroups.AnyAsync(g => g.ParentId == groupId);
            if (hasChildren)
            {
                throw new ArgumentException("该分组下还有子分组，无法删除");
            }

            // 检查是否有字典项
            var hasItems = await _dbContext.DictionaryItems.AnyAsync(i => i.GroupId == groupId);
            if (hasItems)
            {
                throw new ArgumentException("该分组下还有字典项，无法删除");
            }

            _dbContext.DictionaryGroups.Remove(group);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        #endregion

        #region 字典项管理

        /// <summary>
        /// 获取字典项列表
        /// </summary>
        public async Task<List<DictionaryItemDto>> GetItemsAsync(string? groupId = null, bool enabledOnly = false)
        {
            var query = _dbContext.DictionaryItems.AsQueryable();

            if (!string.IsNullOrEmpty(groupId))
            {
                query = query.Where(i => i.GroupId == groupId);
            }

            if (enabledOnly)
            {
                query = query.Where(i => i.IsEnabled);
            }

            var items = await query
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.Name)
                .ToListAsync();

            // 加载分组信息
            var groupIds = items.Select(i => i.GroupId).Distinct().ToList();
            var groups = await _dbContext.DictionaryGroups
                .Where(g => groupIds.Contains(g.Id))
                .ToDictionaryAsync(g => g.Id, g => g);

            return items.Select(i => MapToItemDto(i,
                groups.GetValueOrDefault(i.GroupId)?.Code,
                groups.GetValueOrDefault(i.GroupId)?.Name)).ToList();
        }

        /// <summary>
        /// 根据键获取字典项
        /// </summary>
        public async Task<DictionaryItemDto?> GetItemByKeyAsync(string key)
        {
            var item = await _dbContext.DictionaryItems
                .Include(i => i.Group)
                .FirstOrDefaultAsync(i => i.Key == key);

            return item != null ? MapToItemDto(item, item.Group?.Code, item.Group?.Name) : null;
        }

        /// <summary>
        /// 批量获取字典项
        /// </summary>
        public async Task<Dictionary<string, DictionaryItemDto>> GetItemsByKeysAsync(IEnumerable<string> keys)
        {
            var keyList = keys.Distinct().ToList();

            var items = await _dbContext.DictionaryItems
                .Include(i => i.Group)
                .Where(i => keyList.Contains(i.Key))
                .ToListAsync();

            return items.ToDictionary(
                i => i.Key,
                i => MapToItemDto(i, i.Group?.Code, i.Group?.Name));
        }

        /// <summary>
        /// 根据分组编码获取字典项列表
        /// </summary>
        public async Task<List<DictionaryItemDto>> GetItemsByGroupCodeAsync(string groupCode, bool enabledOnly = false)
        {
            var group = await _dbContext.DictionaryGroups
                .FirstOrDefaultAsync(g => g.Code == groupCode);

            if (group == null)
            {
                return new List<DictionaryItemDto>();
            }

            var query = _dbContext.DictionaryItems
                .Where(i => i.GroupId == group.Id);

            if (enabledOnly)
            {
                query = query.Where(i => i.IsEnabled);
            }

            var items = await query
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.Name)
                .ToListAsync();

            return items.Select(i => MapToItemDto(i, group.Code, group.Name)).ToList();
        }

        /// <summary>
        /// 创建字典项
        /// </summary>
        public async Task<DictionaryItemDto> CreateItemAsync(CreateDictionaryItemRequestDto request)
        {
            // 验证分组是否存在
            var group = await _dbContext.DictionaryGroups.FindAsync(request.GroupId);
            if (group == null)
            {
                throw new ArgumentException("分组不存在");
            }

            // 检查键是否重复
            if (await _dbContext.DictionaryItems.AnyAsync(i => i.Key == request.Key))
            {
                throw new ArgumentException($"字典键 '{request.Key}' 已存在");
            }

            var item = new DictionaryItem
            {
                GroupId = request.GroupId,
                Key = request.Key,
                Name = request.Name,
                Value = request.Value ?? request.DefaultValue,
                DataType = request.DataType,
                DefaultValue = request.DefaultValue,
                Placeholder = request.Placeholder,
                Description = request.Description,
                Options = request.Options,
                Validation = request.Validation,
                IsSystem = request.IsSystem,
                IsRequired = request.IsRequired,
                IsEnabled = true,
                SortOrder = request.SortOrder,
                Remark = request.Remark
            };

            _dbContext.DictionaryItems.Add(item);
            await _dbContext.SaveChangesAsync();

            return MapToItemDto(item, group.Code, group.Name);
        }

        /// <summary>
        /// 批量创建字典项
        /// </summary>
        public async Task<List<DictionaryItemDto>> CreateItemsAsync(BatchCreateDictionaryItemRequestDto request)
        {
            // 验证分组是否存在
            var group = await _dbContext.DictionaryGroups.FindAsync(request.GroupId);
            if (group == null)
            {
                throw new ArgumentException("分组不存在");
            }

            // 获取已存在的键
            var existingKeys = await _dbContext.DictionaryItems
                .Where(i => i.GroupId == request.GroupId)
                .Select(i => i.Key)
                .ToHashSetAsync();

            var items = new List<DictionaryItem>();

            foreach (var itemData in request.Items)
            {
                // 跳过已存在的键
                if (existingKeys.Contains(itemData.Key))
                {
                    continue;
                }

                var item = new DictionaryItem
                {
                    GroupId = request.GroupId,
                    Key = itemData.Key,
                    Name = itemData.Name,
                    Value = itemData.Value,
                    DataType = DictionaryDataType.String,
                    IsSystem = false,
                    IsRequired = false,
                    IsEnabled = true,
                    SortOrder = itemData.SortOrder
                };

                items.Add(item);
                existingKeys.Add(itemData.Key);
            }

            _dbContext.DictionaryItems.AddRange(items);
            await _dbContext.SaveChangesAsync();

            return items.Select(i => MapToItemDto(i, group.Code, group.Name)).ToList();
        }

        /// <summary>
        /// 更新字典项
        /// </summary>
        public async Task<DictionaryItemDto?> UpdateItemAsync(string itemId, UpdateDictionaryItemRequestDto request)
        {
            var item = await _dbContext.DictionaryItems
                .Include(i => i.Group)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null)
            {
                return null;
            }

            if (request.Name != null)
                item.Name = request.Name;

            if (request.Value != null)
                item.Value = request.Value;

            if (request.Placeholder != null)
                item.Placeholder = request.Placeholder;

            if (request.Description != null)
                item.Description = request.Description;

            if (request.IsEnabled.HasValue)
                item.IsEnabled = request.IsEnabled.Value;

            if (request.SortOrder.HasValue)
                item.SortOrder = request.SortOrder.Value;

            if (request.Remark != null)
                item.Remark = request.Remark;

            item.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return MapToItemDto(item, item.Group?.Code, item.Group?.Name);
        }

        /// <summary>
        /// 删除字典项
        /// </summary>
        public async Task<bool> DeleteItemAsync(string itemId)
        {
            var item = await _dbContext.DictionaryItems.FindAsync(itemId);
            if (item == null)
            {
                return false;
            }

            // 系统内置项不可删除
            if (item.IsSystem)
            {
                throw new ArgumentException("系统内置字典项不可删除");
            }

            _dbContext.DictionaryItems.Remove(item);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        #endregion

        #region 配置值获取便捷方法

        /// <summary>
        /// 获取字符串类型配置值
        /// </summary>
        public async Task<string?> GetStringAsync(string key, string? defaultValue = null)
        {
            var item = await _dbContext.DictionaryItems
                .FirstOrDefaultAsync(i => i.Key == key && i.IsEnabled);

            if (item == null)
            {
                return defaultValue;
            }

            return !string.IsNullOrEmpty(item.Value) ? item.Value : (item.DefaultValue ?? defaultValue);
        }

        /// <summary>
        /// 获取数字类型配置值
        /// </summary>
        public async Task<int> GetIntAsync(string key, int defaultValue = 0)
        {
            var strValue = await GetStringAsync(key);
            if (string.IsNullOrEmpty(strValue) || !int.TryParse(strValue, out var result))
            {
                return defaultValue;
            }
            return result;
        }

        /// <summary>
        /// 获取布尔类型配置值
        /// </summary>
        public async Task<bool> GetBoolAsync(string key, bool defaultValue = false)
        {
            var strValue = await GetStringAsync(key);
            if (string.IsNullOrEmpty(strValue))
            {
                return defaultValue;
            }
            return strValue.ToLower() is "true" or "1" or "yes";
        }

        /// <summary>
        /// 获取JSON类型配置值
        /// </summary>
        public async Task<string?> GetJsonAsync(string key, string? defaultValue = null)
        {
            return await GetStringAsync(key, defaultValue);
        }

        /// <summary>
        /// 获取配置值（强类型）
        /// </summary>
        public async Task<T?> GetValueAsync<T>(string key, T? defaultValue = default)
        {
            var strValue = await GetStringAsync(key);
            if (string.IsNullOrEmpty(strValue))
            {
                return defaultValue;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(strValue);
            }
            catch
            {
                return defaultValue;
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 映射分组实体到DTO
        /// </summary>
        private static DictionaryGroupDto MapToGroupDto(DictionaryGroup group, int childrenCount, int itemsCount)
        {
            return new DictionaryGroupDto
            {
                Id = group.Id,
                Code = group.Code,
                Name = group.Name,
                Description = group.Description,
                Type = group.Type,
                ParentId = group.ParentId,
                Level = group.Level,
                Path = group.Path,
                Icon = group.Icon,
                SortOrder = group.SortOrder,
                IsEnabled = group.IsEnabled,
                Remark = group.Remark,
                ChildrenCount = childrenCount,
                ItemsCount = itemsCount,
                CreatedAt = group.CreatedAt,
                UpdatedAt = group.UpdatedAt
            };
        }

        /// <summary>
        /// 映射分组实体到树节点DTO
        /// </summary>
        private static DictionaryGroupTreeDto MapToGroupTreeDto(DictionaryGroup group)
        {
            return new DictionaryGroupTreeDto
            {
                Id = group.Id,
                Code = group.Code,
                Name = group.Name,
                Description = group.Description,
                Type = group.Type,
                Level = group.Level,
                Icon = group.Icon,
                SortOrder = group.SortOrder,
                IsEnabled = group.IsEnabled,
                Children = new List<DictionaryGroupTreeDto>(),
                Items = new List<DictionaryItemDto>()
            };
        }

        /// <summary>
        /// 映射字典项实体到DTO
        /// </summary>
        private static DictionaryItemDto MapToItemDto(DictionaryItem item, string? groupCode = null, string? groupName = null)
        {
            return new DictionaryItemDto
            {
                Id = item.Id,
                GroupId = item.GroupId,
                GroupCode = groupCode,
                GroupName = groupName,
                Key = item.Key,
                Name = item.Name,
                Value = item.Value,
                DataType = item.DataType,
                DefaultValue = item.DefaultValue,
                Placeholder = item.Placeholder,
                Description = item.Description,
                Options = item.Options,
                Validation = item.Validation,
                IsSystem = item.IsSystem,
                IsRequired = item.IsRequired,
                IsEnabled = item.IsEnabled,
                SortOrder = item.SortOrder,
                Remark = item.Remark,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            };
        }

        #endregion
    }
}
