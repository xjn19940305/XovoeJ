using XovoeJ.Contracts.Dictionary;
using XovoeJ.Enum;

namespace XovoeJ.Abstractions.Services
{
    /// <summary>
    /// 字典服务接口
    /// </summary>
    public interface IDictionaryService
    {
        #region 分组管理

        /// <summary>
        /// 获取分组列表
        /// </summary>
        /// <param name="type">字典类型</param>
        /// <returns>分组列表</returns>
        Task<List<DictionaryGroupDto>> GetGroupsAsync(DictionaryType? type = null);

        /// <summary>
        /// 获取分组树
        /// </summary>
        /// <param name="type">字典类型</param>
        /// <returns>分组树</returns>
        Task<List<DictionaryGroupTreeDto>> GetGroupTreeAsync(DictionaryType? type = null);

        /// <summary>
        /// 根据编码获取分组
        /// </summary>
        /// <param name="code">分组编码</param>
        /// <returns>分组信息</returns>
        Task<DictionaryGroupDto?> GetGroupByCodeAsync(string code);

        /// <summary>
        /// 根据ID获取分组
        /// </summary>
        /// <param name="groupId">分组ID</param>
        /// <returns>分组信息</returns>
        Task<DictionaryGroupDto?> GetGroupByIdAsync(string groupId);

        /// <summary>
        /// 创建分组
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>创建的分组信息</returns>
        Task<DictionaryGroupDto> CreateGroupAsync(CreateDictionaryGroupRequestDto request);

        /// <summary>
        /// 更新分组
        /// </summary>
        /// <param name="groupId">分组ID</param>
        /// <param name="request">更新请求</param>
        /// <returns>更新后的分组信息</returns>
        Task<DictionaryGroupDto?> UpdateGroupAsync(string groupId, UpdateDictionaryGroupRequestDto request);

        /// <summary>
        /// 删除分组
        /// </summary>
        /// <param name="groupId">分组ID</param>
        /// <returns></returns>
        Task<bool> DeleteGroupAsync(string groupId);

        #endregion

        #region 字典项管理

        /// <summary>
        /// 获取字典项列表
        /// </summary>
        /// <param name="groupId">分组ID</param>
        /// <param name="enabledOnly">是否只获取启用的项</param>
        /// <returns>字典项列表</returns>
        Task<List<DictionaryItemDto>> GetItemsAsync(string? groupId = null, bool enabledOnly = false);

        /// <summary>
        /// 根据键获取字典项
        /// </summary>
        /// <param name="key">字典键</param>
        /// <returns>字典项信息</returns>
        Task<DictionaryItemDto?> GetItemByKeyAsync(string key);

        /// <summary>
        /// 批量获取字典项
        /// </summary>
        /// <param name="keys">字典键集合</param>
        /// <returns>字典项字典</returns>
        Task<Dictionary<string, DictionaryItemDto>> GetItemsByKeysAsync(IEnumerable<string> keys);

        /// <summary>
        /// 根据分组编码获取字典项列表
        /// </summary>
        /// <param name="groupCode">分组编码</param>
        /// <param name="enabledOnly">是否只获取启用的项</param>
        /// <returns>字典项列表</returns>
        Task<List<DictionaryItemDto>> GetItemsByGroupCodeAsync(string groupCode, bool enabledOnly = false);

        /// <summary>
        /// 创建字典项
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>创建的字典项信息</returns>
        Task<DictionaryItemDto> CreateItemAsync(CreateDictionaryItemRequestDto request);

        /// <summary>
        /// 批量创建字典项
        /// </summary>
        /// <param name="request">批量创建请求</param>
        /// <returns>创建的字典项信息列表</returns>
        Task<List<DictionaryItemDto>> CreateItemsAsync(BatchCreateDictionaryItemRequestDto request);

        /// <summary>
        /// 更新字典项
        /// </summary>
        /// <param name="itemId">字典项ID</param>
        /// <param name="request">更新请求</param>
        /// <returns>更新后的字典项信息</returns>
        Task<DictionaryItemDto?> UpdateItemAsync(string itemId, UpdateDictionaryItemRequestDto request);

        /// <summary>
        /// 删除字典项
        /// </summary>
        /// <param name="itemId">字典项ID</param>
        /// <returns></returns>
        Task<bool> DeleteItemAsync(string itemId);

        #endregion

        #region 配置值获取便捷方法

        /// <summary>
        /// 获取字符串类型配置值
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值</returns>
        Task<string?> GetStringAsync(string key, string? defaultValue = null);

        /// <summary>
        /// 获取数字类型配置值
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值</returns>
        Task<int> GetIntAsync(string key, int defaultValue = 0);

        /// <summary>
        /// 获取布尔类型配置值
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值</returns>
        Task<bool> GetBoolAsync(string key, bool defaultValue = false);

        /// <summary>
        /// 获取JSON类型配置值
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值</returns>
        Task<string?> GetJsonAsync(string key, string? defaultValue = null);

        /// <summary>
        /// 获取配置值（强类型）
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值</returns>
        Task<T?> GetValueAsync<T>(string key, T? defaultValue = default);

        #endregion
    }
}
