using XovoeJ.Contracts.Cart;

namespace XovoeJ.Abstractions.Services
{
    /// <summary>
    /// 购物车服务接口
    /// </summary>
    public interface ICartService
    {
        /// <summary>
        /// 获取购物车列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>购物车信息</returns>
        Task<CartResponseDto> GetCartAsync(string userId);

        /// <summary>
        /// 添加到购物车
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="request">添加请求</param>
        /// <returns></returns>
        Task AddToCartAsync(string userId, AddToCartRequestDto request);

        /// <summary>
        /// 更新购物车商品数量
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="cartItemId">购物车项ID</param>
        /// <param name="request">更新请求</param>
        /// <returns></returns>
        Task UpdateQuantityAsync(string userId, string cartItemId, UpdateCartQuantityRequestDto request);

        /// <summary>
        /// 删除购物车商品
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="cartItemId">购物车项ID</param>
        /// <returns></returns>
        Task RemoveFromCartAsync(string userId, string cartItemId);

        /// <summary>
        /// 清空购物车
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        Task ClearCartAsync(string userId);

        /// <summary>
        /// 获取购物车商品数量
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>商品数量</returns>
        Task<int> GetCartCountAsync(string userId);
    }
}
