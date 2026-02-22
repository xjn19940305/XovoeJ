using System.ComponentModel.DataAnnotations;

namespace XovoeJ.Contracts.Product
{
    /// <summary>
    /// 更新分类请求DTO
    /// </summary>
    public class UpdateCategoryRequestDto
    {
        /// <summary>
        /// 分类名称
        /// </summary>
        [Required(ErrorMessage = "分类名称不能为空")]
        [MaxLength(128, ErrorMessage = "分类名称长度不能超过128个字符")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 图标
        /// </summary>
        [MaxLength(256)]
        public string? Icon { get; set; }

        /// <summary>
        /// 分类图片
        /// </summary>
        [MaxLength(512)]
        public string? Image { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}
