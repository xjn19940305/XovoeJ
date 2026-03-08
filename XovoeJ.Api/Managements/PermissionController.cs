using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XovoeJ.Api.Swaggers;

namespace XovoeJ.Api.Managements
{
    [ApiController]
    [Route("api/admin/permissions")]
    [Produces("application/json")]
    [Authorize]
    [ApiGroup(ApiGroupNames.MANAGEMENT)]
    public class PermissionController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetPermissions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? name = null,
            [FromQuery] string? code = null,
            [FromQuery] string? type = null,
            [FromQuery] int? status = null)
        {
            var query = PermissionCatalog.GetFlatList().AsEnumerable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(item => item.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(code))
            {
                query = query.Where(item => item.Code.Contains(code, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(item => string.Equals(item.Type, type, StringComparison.OrdinalIgnoreCase));
            }

            if (status.HasValue)
            {
                query = query.Where(item => item.Status == status.Value);
            }

            var total = query.Count();
            var list = query
                .OrderBy(item => item.Id)
                .Skip((Math.Max(page, 1) - 1) * Math.Max(pageSize, 1))
                .Take(Math.Max(pageSize, 1))
                .ToList();

            return Ok(new
            {
                items = list,
                total,
                page = Math.Max(page, 1),
                pageSize = Math.Max(pageSize, 1),
            });
        }

        [HttpGet("tree")]
        public IActionResult GetPermissionTree()
        {
            return Ok(new
            {
                items = PermissionCatalog.GetTree(),
            });
        }
    }
}
