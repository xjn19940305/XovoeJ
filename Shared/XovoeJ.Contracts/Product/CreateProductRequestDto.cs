using System.ComponentModel.DataAnnotations;

namespace XovoeJ.Contracts.Product
{
    /// <summary>
    /// 创建商品请求DTO
    /// </summary>
    public class CreateProductRequestDto
    {
        /// <summary>
        /// 分类ID
        /// </summary>
        [Required(ErrorMessage = "分类ID不能为空")]
        public string CategoryId { get; set; } = string.Empty;

        /// <summary>
        /// 商品名称
        /// </summary>
        [Required(ErrorMessage = "商品名称不能为空")]
        [MaxLength(256, ErrorMessage = "商品名称长度不能超过256个字符")]
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

        /// <summary>
        /// SKU列表
        /// </summary>
        [Required(ErrorMessage = "SKU列表不能为空")]
        public List<CreateProductSkuDto> Skus { get; set; } = new();
    }

    /// <summary>
    /// 创建商品SKU请求DTO
    /// </summary>
    public class CreateProductSkuDto
    {
        /// <summary>
        /// SKU编码
        /// </summary>
        [Required(ErrorMessage = "SKU编码不能为空")]
        [MaxLength(64)]
        public string SkuCode { get; set; } = string.Empty;

        /// <summary>
        /// 规格属性
        /// </summary>
        public Dictionary<string, string>? Specs { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        [Required(ErrorMessage = "价格不能为空")]
        public decimal Price { get; set; }

        /// <summary>
        /// 原价
        /// </summary>
        public decimal? OriginalPrice { get; set; }

        /// <summary>
        /// 成本价
        /// </summary>
        public decimal? CostPrice { get; set; }

        /// <summary>
        /// 库存
        /// </summary>
        [Required(ErrorMessage = "库存不能为空")]
        public int Stock { get; set; }

        /// <summary>
        /// 低库存预警
        /// </summary>
        public int LowStock { get; set; } = 10;

        /// <summary>
        /// SKU图片
        /// </summary>
        [MaxLength(512)]
        public string? Image { get; set; }
    }
}
