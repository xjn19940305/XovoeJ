using XovoeJ.Contracts.Order;

namespace XovoeJ.Abstractions.Services
{
    /// <summary>
    /// 订单服务接口
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// 提交订单
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="request">提交订单请求</param>
        /// <returns>订单信息</returns>
        Task<OrderDto> SubmitOrderAsync(string userId, SubmitOrderRequestDto request);

        /// <summary>
        /// 获取订单列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="query">查询参数</param>
        /// <returns>订单列表</returns>
        Task<OrderListResponseDto> GetOrdersAsync(string userId, OrderListQueryDto query);

        /// <summary>
        /// 根据ID获取订单
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="orderId">订单ID</param>
        /// <returns>订单信息</returns>
        Task<OrderDto?> GetOrderByIdAsync(string userId, string orderId);

        /// <summary>
        /// 根据订单号获取订单
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="orderNo">订单号</param>
        /// <returns>订单信息</returns>
        Task<OrderDto?> GetOrderByNoAsync(string userId, string orderNo);

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="orderId">订单ID</param>
        /// <returns></returns>
        Task<bool> CancelOrderAsync(string userId, string orderId);

        /// <summary>
        /// 确认收货
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="orderId">订单ID</param>
        /// <returns></returns>
        Task<bool> ConfirmReceiptAsync(string userId, string orderId);

        /// <summary>
        /// 删除订单（软删除）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="orderId">订单ID</param>
        /// <returns></returns>
        Task<bool> DeleteOrderAsync(string userId, string orderId);

        /// <summary>
        /// 支付订单（支付回调）
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="paymentMethod">支付方式</param>
        /// <returns></returns>
        Task<bool> PayOrderAsync(string orderId, string paymentMethod, string? paymentOrderId = null, string? paymentOrderNo = null, decimal? paidAmount = null);

        /// <summary>
        /// 发货（管理端）
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns></returns>
        Task<bool> ShipOrderAsync(string orderId, ShipOrderRequestDto request);

        /// <summary>
        /// 获取订单物流信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="orderId">订单ID</param>
        /// <returns></returns>
        Task<OrderTrackingDto?> GetOrderTrackingAsync(string userId, string orderId);

        /// <summary>
        /// 完成订单
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="orderId">订单ID</param>
        /// <returns></returns>
        Task<bool> CompleteOrderAsync(string userId, string orderId);
    }
}
