using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XovoeJ.Abstractions.Storage;
using XovoeJ.Api.Swaggers;

namespace XovoeJ.Api.Controllers
{
    /// <summary>
    /// 文件上传控制器
    /// </summary>
    [ApiController]
    [Route("api/files")]
    [Produces("application/json")]
    [ApiGroup(ApiGroupNames.MANAGEMENT, ApiGroupNames.USER)]
    public class FileController : ControllerBase
    {
        private readonly IObjectStorage _objectStorage;
        private readonly ILogger<FileController> _logger;

        public FileController(IObjectStorage objectStorage, ILogger<FileController> logger)
        {
            _objectStorage = objectStorage;
            _logger = logger;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="file">文件</param>
        /// <param name="path">存储路径（可选，如：images/avatar.jpg）</param>
        /// <returns>上传结果</returns>
        [HttpPost("upload")]
        [RequestSizeLimit(104857600)] // 限制100MB
        public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string? path = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "请选择要上传的文件" });
                }

                // 生成文件路径
                var fileName = Path.GetFileName(file.FileName);
                var key = string.IsNullOrEmpty(path)
                    ? $"{DateTime.UtcNow:yyyyMMdd}/{Guid.NewGuid()}{Path.GetExtension(fileName)}"
                    : path.Trim('/');

                // 如果path是目录，则加上文件名
                if (!string.IsNullOrEmpty(path) && !Path.HasExtension(path))
                {
                    key = $"{key.TrimEnd('/')}/{Guid.NewGuid()}{Path.GetExtension(fileName)}";
                }

                using var stream = file.OpenReadStream();
                await _objectStorage.PutAsync(key, stream, file.ContentType);

                _logger.LogInformation("文件上传成功: {Key}, 大小: {Size} bytes", key, file.Length);

                return Ok(new
                {
                    key,
                    fileName,
                    size = file.Length,
                    contentType = file.ContentType,
                    message = "上传成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文件上传失败: {FileName}", file?.FileName);
                return BadRequest(new { message = "文件上传失败" });
            }
        }

        /// <summary>
        /// 批量上传文件
        /// </summary>
        /// <param name="files">文件集合</param>
        /// <param name="path">存储路径（可选）</param>
        /// <returns>上传结果</returns>
        [HttpPost("upload-batch")]
        [RequestSizeLimit(104857600)] // 限制100MB
        public async Task<IActionResult> UploadBatch(IFormFileCollection files, [FromQuery] string? path = null)
        {
            try
            {
                if (files == null || files.Count == 0)
                {
                    return BadRequest(new { message = "请选择要上传的文件" });
                }

                var results = new List<object>();

                foreach (var file in files)
                {
                    if (file.Length == 0) continue;

                    var fileName = Path.GetFileName(file.FileName);
                    var key = string.IsNullOrEmpty(path)
                        ? $"{DateTime.UtcNow:yyyyMMdd}/{Guid.NewGuid()}{Path.GetExtension(fileName)}"
                        : path.Trim('/');

                    if (!string.IsNullOrEmpty(path) && !Path.HasExtension(path))
                    {
                        key = $"{key.TrimEnd('/')}/{Guid.NewGuid()}{Path.GetExtension(fileName)}";
                    }

                    using var stream = file.OpenReadStream();
                    await _objectStorage.PutAsync(key, stream, file.ContentType);

                    results.Add(new
                    {
                        key,
                        fileName,
                        size = file.Length,
                        contentType = file.ContentType
                    });

                    _logger.LogInformation("文件上传成功: {Key}, 大小: {Size} bytes", key, file.Length);
                }

                return Ok(new
                {
                    files = results,
                    total = results.Count,
                    message = $"成功上传 {results.Count} 个文件"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量文件上传失败");
                return BadRequest(new { message = "批量文件上传失败" });
            }
        }

        /// <summary>
        /// 获取文件访问URL
        /// </summary>
        /// <param name="key">文件key</param>
        /// <param name="expiresIn">过期时间（秒，默认3600）</param>
        /// <returns>访问URL</returns>
        [HttpGet("url")]
        public async Task<IActionResult> GetPresignedUrl([FromQuery] string key, [FromQuery] int expiresIn = 3600)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    return BadRequest(new { message = "文件key不能为空" });
                }

                var exists = await _objectStorage.ExistsAsync(key);
                if (!exists)
                {
                    return NotFound(new { message = "文件不存在" });
                }

                var url = await _objectStorage.GetPresignedUrlAsync(key, TimeSpan.FromSeconds(expiresIn));

                return Ok(new
                {
                    key,
                    url,
                    expiresIn,
                    message = "获取成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取文件URL失败: {Key}", key);
                return BadRequest(new { message = "获取文件URL失败" });
            }
        }

        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="key">文件key</param>
        /// <returns>文件信息</returns>
        [HttpGet("info")]
        public async Task<IActionResult> GetFileInfo([FromQuery] string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    return BadRequest(new { message = "文件key不能为空" });
                }

                var metadata = await _objectStorage.StatAsync(key);
                if (metadata == null)
                {
                    return NotFound(new { message = "文件不存在" });
                }

                return Ok(new
                {
                    metadata.Key,
                    metadata.Size,
                    metadata.ContentType,
                    metadata.ETag,
                    message = "获取成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取文件信息失败: {Key}", key);
                return BadRequest(new { message = "获取文件信息失败" });
            }
        }

        /// <summary>
        /// 获取永久访问的文件URL（用于富文本内容等长期存储的场景）
        /// </summary>
        /// <param name="key">文件key</param>
        /// <returns>永久访问URL（通过后端代理）</returns>
        [HttpGet("view")]
        public IActionResult GetFileUrl([FromQuery] string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return BadRequest(new { message = "文件key不能为空" });
            }

            // 返回后端代理URL，通过后端转发请求
            var fileUrl = $"{Request.Scheme}://{Request.Host}/api/files/proxy?key={Uri.EscapeDataString(key)}";
            return Ok(new { key, url = fileUrl, message = "获取成功" });
        }

        /// <summary>
        /// 代理访问文件（用于前端直接访问，支持缓存）
        /// </summary>
        /// <param name="key">文件key</param>
        /// <returns>文件内容</returns>
        [HttpGet("proxy")]
        [ResponseCache(Duration = 365 * 24 * 60 * 60)] // 缓存1年
        public async Task<IActionResult> GetFile([FromQuery] string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return BadRequest(new { message = "文件key不能为空" });
            }

            var exists = await _objectStorage.ExistsAsync(key);
            if (!exists)
            {
                return NotFound(new { message = "文件不存在" });
            }

            var content = await _objectStorage.GetAsync(key);
            var metadata = await _objectStorage.StatAsync(key);

            return File(content, metadata?.ContentType ?? "application/octet-stream");
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="key">文件key</param>
        /// <returns>删除结果</returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteFile([FromQuery] string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    return BadRequest(new { message = "文件key不能为空" });
                }

                var exists = await _objectStorage.ExistsAsync(key);
                if (!exists)
                {
                    return NotFound(new { message = "文件不存在" });
                }

                await _objectStorage.DeleteAsync(key);
                _logger.LogInformation("文件删除成功: {Key}", key);

                return Ok(new { message = "删除成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除文件失败: {Key}", key);
                return BadRequest(new { message = "删除文件失败" });
            }
        }

        /// <summary>
        /// 批量删除文件
        /// </summary>
        /// <param name="keys">文件key集合</param>
        /// <returns>删除结果</returns>
        [HttpDelete("batch")]
        public async Task<IActionResult> DeleteFiles([FromBody] string[] keys)
        {
            try
            {
                if (keys == null || keys.Length == 0)
                {
                    return BadRequest(new { message = "请提供要删除的文件key" });
                }

                var results = new Dictionary<string, object>();
                var successCount = 0;
                var failCount = 0;

                foreach (var key in keys)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(key))
                        {
                            results[key] = new { success = false, message = "key不能为空" };
                            failCount++;
                            continue;
                        }

                        var exists = await _objectStorage.ExistsAsync(key);
                        if (!exists)
                        {
                            results[key] = new { success = false, message = "文件不存在" };
                            failCount++;
                            continue;
                        }

                        await _objectStorage.DeleteAsync(key);
                        results[key] = new { success = true, message = "删除成功" };
                        successCount++;

                        _logger.LogInformation("文件删除成功: {Key}", key);
                    }
                    catch (Exception ex)
                    {
                        results[key] = new { success = false, message = ex.Message };
                        failCount++;
                        _logger.LogError(ex, "删除文件失败: {Key}", key);
                    }
                }

                return Ok(new
                {
                    results,
                    successCount,
                    failCount,
                    total = keys.Length,
                    message = $"删除完成，成功 {successCount} 个，失败 {failCount} 个"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量删除文件失败");
                return BadRequest(new { message = "批量删除文件失败" });
            }
        }
    }
}
