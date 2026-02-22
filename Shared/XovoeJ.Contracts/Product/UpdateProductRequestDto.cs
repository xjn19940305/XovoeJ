using System.ComponentModel.DataAnnotations;

namespace XovoeJ.Contracts.Product
{
    /// <summary>
    /// 更新商品请求DTO
    /// </summary>
    public class UpdateProductRequestDto
    {
        /// <summary>
        /// 商品名称
        /// </summary>
        [Required(ErrorMessage = "商品名称不能为空")]
        [MaxLength(256)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 副标题
        /// </summary>
        [MaxLength(256)]
        public string? Subtitle { get; set; }

        /// <summary>
        /// 商品描述
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// 主图
        /// </summary>
        [MaxLength(512)]
        public string? MainImage { get; set; }

        /// <summary>
        /// 图片列表
        /// </summary>
        public List<string>? Images { get; set; }

        /// <summary>
        /// 详情内容
        /// </summary>
        public string? Detail { get; set; }

        /// <summary>
        /// 是否上架
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否热门
        /// </summary>
        public bool IsHot { get; set; }

        /// <summary>
        /// 是否新品
        /// </summary>
        public bool IsNew { get; set; }

        /// <summary>
        /// 是否推荐
        /// </summary>
        public bool IsRecommend { get; set; }
    }
}
