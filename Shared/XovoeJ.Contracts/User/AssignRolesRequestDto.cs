using System.ComponentModel.DataAnnotations;

namespace XovoeJ.Contracts.User
{
    /// <summary>
    /// 分配角色请求DTO
    /// </summary>
    public class AssignRolesRequestDto
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [Required(ErrorMessage = "用户ID不能为空")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 角色名称列表
        /// </summary>
        [Required(ErrorMessage = "角色列表不能为空")]
        public List<string> RoleNames { get; set; } = new();
    }
}
