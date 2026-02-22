namespace XovoeJ.Contracts.Order
{
    /// <summary>
    /// 订单列表查询参数
    /// </summary>
    public class OrderListQueryDto
    {
        /// <summary>
        /// 页码（从1开始）
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// 订单状态筛选
        /// </summary>
        public int? Status { get; set; }
    }

    /// <summary>
    /// 订单列表响应DTO
    /// </summary>
    public class OrderListResponseDto
    {
        /// <summary>
        /// 订单列表
        /// </summary>
        public List<OrderDto> Items { get; set; } = new();

        /// <summary>
        /// 总记录数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
    }
}
