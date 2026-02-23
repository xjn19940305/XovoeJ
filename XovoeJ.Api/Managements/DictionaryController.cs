using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XovoeJ.Abstractions.Services;
using XovoeJ.Api.Swaggers;
using XovoeJ.Contracts.Dictionary;
using XovoeJ.Enum;

namespace XovoeJ.Api.Managements
{
    /// <summary>
    /// 字典管理控制器
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [ApiGroup(ApiGroupNames.MANAGEMENT)]
    public class DictionaryController : ControllerBase
    {
        private readonly IDictionaryService _dictionaryService;
        private readonly ILogger<DictionaryController> _logger;

        public DictionaryController(IDictionaryService dictionaryService, ILogger<DictionaryController> logger)
        {
            _dictionaryService = dictionaryService;
            _logger = logger;
        }

        #region 分组管理

        /// <summary>
        /// 获取分组列表
        /// </summary>
        /// <param name="type">字典类型（0=系统配置, 1=业务字典）</param>
        /// <returns>分组列表</returns>
        [HttpGet("api/dictionary/groups")]
        public async Task<IActionResult> GetGroups([FromQuery] DictionaryType? type)
        {
            try
            {
                var groups = await _dictionaryService.GetGroupsAsync(type);
                return Ok(groups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分组列表失败");
                return BadRequest(new { message = "获取分组列表失败" });
            }
        }

        /// <summary>
        /// 获取分组树
        /// </summary>
        /// <param name="type">字典类型（0=系统配置, 1=业务字典）</param>
        /// <returns>分组树</returns>
        [HttpGet("api/dictionary/groups/tree")]
        public async Task<IActionResult> GetGroupTree([FromQuery] DictionaryType? type)
        {
            try
            {
                var tree = await _dictionaryService.GetGroupTreeAsync(type);
                return Ok(tree);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分组树失败");
                return BadRequest(new { message = "获取分组树失败" });
            }
        }

        /// <summary>
        /// 根据编码获取分组
        /// </summary>
        /// <param name="code">分组编码</param>
        /// <returns>分组信息</returns>
        [HttpGet("api/dictionary/groups/by-code/{code}")]
        public async Task<IActionResult> GetGroupByCode(string code)
        {
            try
            {
                var group = await _dictionaryService.GetGroupByCodeAsync(code);
                if (group == null)
                {
                    return NotFound(new { message = "分组不存在" });
                }
                return Ok(group);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分组信息失败: {Code}", code);
                return BadRequest(new { message = "获取分组信息失败" });
            }
        }

        /// <summary>
        /// 根据ID获取分组
        /// </summary>
        /// <param name="groupId">分组ID</param>
        /// <returns>分组信息</returns>
        [HttpGet("api/dictionary/groups/{groupId}")]
        public async Task<IActionResult> GetGroupById(string groupId)
        {
            try
            {
                var group = await _dictionaryService.GetGroupByIdAsync(groupId);
                if (group == null)
                {
                    return NotFound(new { message = "分组不存在" });
                }
                return Ok(group);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分组信息失败: {GroupId}", groupId);
                return BadRequest(new { message = "获取分组信息失败" });
            }
        }

        /// <summary>
        /// 创建分组
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>创建的分组信息</returns>
        [HttpPost("api/dictionary/groups")]
        [Authorize]
        public async Task<IActionResult> CreateGroup([FromBody] CreateDictionaryGroupRequestDto request)
        {
            try
            {
                var group = await _dictionaryService.CreateGroupAsync(request);
                return Ok(group);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建分组失败: {GroupName}", request.Name);
                return BadRequest(new { message = "创建分组失败" });
            }
        }

        /// <summary>
        /// 更新分组
        /// </summary>
        /// <param name="groupId">分组ID</param>
        /// <param name="request">更新请求</param>
        /// <returns>更新后的分组信息</returns>
        [HttpPut("api/dictionary/groups/{groupId}")]
        [Authorize]
        public async Task<IActionResult> UpdateGroup(string groupId, [FromBody] UpdateDictionaryGroupRequestDto request)
        {
            try
            {
                var group = await _dictionaryService.UpdateGroupAsync(groupId, request);
                if (group == null)
                {
                    return NotFound(new { message = "分组不存在" });
                }
                return Ok(group);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新分组失败: {GroupId}", groupId);
                return BadRequest(new { message = "更新分组失败" });
            }
        }

        /// <summary>
        /// 删除分组
        /// </summary>
        /// <param name="groupId">分组ID</param>
        /// <returns></returns>
        [HttpDelete("api/dictionary/groups/{groupId}")]
        [Authorize]
        public async Task<IActionResult> DeleteGroup(string groupId)
        {
            try
            {
                var result = await _dictionaryService.DeleteGroupAsync(groupId);
                if (!result)
                {
                    return NotFound(new { message = "分组不存在" });
                }
                return Ok(new { message = "删除成功" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除分组失败: {GroupId}", groupId);
                return BadRequest(new { message = "删除分组失败" });
            }
        }

        #endregion

        #region 字典项管理

        /// <summary>
        /// 获取字典项列表
        /// </summary>
        /// <param name="groupId">分组ID</param>
        /// <param name="enabledOnly">是否只获取启用的项</param>
        /// <returns>字典项列表</returns>
        [HttpGet("api/dictionary/items")]
        public async Task<IActionResult> GetItems([FromQuery] string? groupId = null, [FromQuery] bool enabledOnly = false)
        {
            try
            {
                var items = await _dictionaryService.GetItemsAsync(groupId, enabledOnly);
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取字典项列表失败");
                return BadRequest(new { message = "获取字典项列表失败" });
            }
        }

        /// <summary>
        /// 根据分组编码获取字典项列表
        /// </summary>
        /// <param name="groupCode">分组编码</param>
        /// <param name="enabledOnly">是否只获取启用的项</param>
        /// <returns>字典项列表</returns>
        [HttpGet("api/dictionary/items/by-group/{groupCode}")]
        public async Task<IActionResult> GetItemsByGroupCode(string groupCode, [FromQuery] bool enabledOnly = false)
        {
            try
            {
                var items = await _dictionaryService.GetItemsByGroupCodeAsync(groupCode, enabledOnly);
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取字典项列表失败: {GroupCode}", groupCode);
                return BadRequest(new { message = "获取字典项列表失败" });
            }
        }

        /// <summary>
        /// 根据键获取字典项
        /// </summary>
        /// <param name="key">字典键</param>
        /// <returns>字典项信息</returns>
        [HttpGet("api/dictionary/items/by-key/{key}")]
        public async Task<IActionResult> GetItemByKey(string key)
        {
            try
            {
                var item = await _dictionaryService.GetItemByKeyAsync(key);
                if (item == null)
                {
                    return NotFound(new { message = "字典项不存在" });
                }
                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取字典项信息失败: {Key}", key);
                return BadRequest(new { message = "获取字典项信息失败" });
            }
        }

        /// <summary>
        /// 批量获取字典项
        /// </summary>
        /// <param name="keys">字典键集合（逗号分隔）</param>
        /// <returns>字典项字典</returns>
        [HttpGet("api/dictionary/items/batch")]
        public async Task<IActionResult> GetItemsByKeys([FromQuery] string keys)
        {
            try
            {
                var keyList = keys.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var items = await _dictionaryService.GetItemsByKeysAsync(keyList);
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量获取字典项失败");
                return BadRequest(new { message = "批量获取字典项失败" });
            }
        }

        /// <summary>
        /// 获取配置值（字符串）
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值</returns>
        [HttpGet("api/dictionary/value/string/{key}")]
        public async Task<IActionResult> GetStringValue(string key, [FromQuery] string? defaultValue = null)
        {
            try
            {
                var value = await _dictionaryService.GetStringAsync(key, defaultValue);
                return Ok(new { key, value });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取配置值失败: {Key}", key);
                return BadRequest(new { message = "获取配置值失败" });
            }
        }

        /// <summary>
        /// 获取配置值（数字）
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值</returns>
        [HttpGet("api/dictionary/value/int/{key}")]
        public async Task<IActionResult> GetIntValue(string key, [FromQuery] int defaultValue = 0)
        {
            try
            {
                var value = await _dictionaryService.GetIntAsync(key, defaultValue);
                return Ok(new { key, value });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取配置值失败: {Key}", key);
                return BadRequest(new { message = "获取配置值失败" });
            }
        }

        /// <summary>
        /// 获取配置值（布尔）
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值</returns>
        [HttpGet("api/dictionary/value/bool/{key}")]
        public async Task<IActionResult> GetBoolValue(string key, [FromQuery] bool defaultValue = false)
        {
            try
            {
                var value = await _dictionaryService.GetBoolAsync(key, defaultValue);
                return Ok(new { key, value });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取配置值失败: {Key}", key);
                return BadRequest(new { message = "获取配置值失败" });
            }
        }

        /// <summary>
        /// 创建字典项
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>创建的字典项信息</returns>
        [HttpPost("api/dictionary/items")]
        [Authorize]
        public async Task<IActionResult> CreateItem([FromBody] CreateDictionaryItemRequestDto request)
        {
            try
            {
                var item = await _dictionaryService.CreateItemAsync(request);
                return Ok(item);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建字典项失败: {ItemName}", request.Name);
                return BadRequest(new { message = "创建字典项失败" });
            }
        }

        /// <summary>
        /// 批量创建字典项
        /// </summary>
        /// <param name="request">批量创建请求</param>
        /// <returns>创建的字典项信息列表</returns>
        [HttpPost("api/dictionary/items/batch")]
        [Authorize]
        public async Task<IActionResult> CreateItems([FromBody] BatchCreateDictionaryItemRequestDto request)
        {
            try
            {
                var items = await _dictionaryService.CreateItemsAsync(request);
                return Ok(items);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量创建字典项失败");
                return BadRequest(new { message = "批量创建字典项失败" });
            }
        }

        /// <summary>
        /// 更新字典项
        /// </summary>
        /// <param name="itemId">字典项ID</param>
        /// <param name="request">更新请求</param>
        /// <returns>更新后的字典项信息</returns>
        [HttpPut("api/dictionary/items/{itemId}")]
        [Authorize]
        public async Task<IActionResult> UpdateItem(string itemId, [FromBody] UpdateDictionaryItemRequestDto request)
        {
            try
            {
                var item = await _dictionaryService.UpdateItemAsync(itemId, request);
                if (item == null)
                {
                    return NotFound(new { message = "字典项不存在" });
                }
                return Ok(item);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新字典项失败: {ItemId}", itemId);
                return BadRequest(new { message = "更新字典项失败" });
            }
        }

        /// <summary>
        /// 删除字典项
        /// </summary>
        /// <param name="itemId">字典项ID</param>
        /// <returns></returns>
        [HttpDelete("api/dictionary/items/{itemId}")]
        [Authorize]
        public async Task<IActionResult> DeleteItem(string itemId)
        {
            try
            {
                var result = await _dictionaryService.DeleteItemAsync(itemId);
                if (!result)
                {
                    return NotFound(new { message = "字典项不存在" });
                }
                return Ok(new { message = "删除成功" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除字典项失败: {ItemId}", itemId);
                return BadRequest(new { message = "删除字典项失败" });
            }
        }

        #endregion
    }
}
