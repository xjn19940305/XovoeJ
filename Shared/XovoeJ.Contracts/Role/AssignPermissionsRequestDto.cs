using System.ComponentModel.DataAnnotations;

namespace XovoeJ.Contracts.Role
{
    /// <summary>
    /// 分配权限请求DTO
    /// </summary>
    public class AssignPermissionsRequestDto
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        [Required(ErrorMessage = "角色ID不能为空")]
        public string RoleId { get; set; } = string.Empty;

        /// <summary>
        /// 权限代码列表
        /// </summary>
        [Required(ErrorMessage = "权限列表不能为空")]
        public List<string> PermissionCodes { get; set; } = new();
    }
}
