using System.ComponentModel.DataAnnotations;

namespace XovoeJ.Contracts.Role
{
    /// <summary>
    /// 创建角色请求DTO
    /// </summary>
    public class CreateRoleRequestDto
    {
        /// <summary>
        /// 角色名称
        /// </summary>
        [Required(ErrorMessage = "角色名称不能为空")]
        [MaxLength(128, ErrorMessage = "角色名称长度不能超过128个字符")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 角色描述
        /// </summary>
        [MaxLength(512, ErrorMessage = "角色描述长度不能超过512个字符")]
        public string? Description { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int? Sort { get; set; }
    }
}
