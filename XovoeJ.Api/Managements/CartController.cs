using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XovoeJ.Abstractions.Services;
using XovoeJ.Api.Swaggers;
using XovoeJ.Contracts.Cart;

namespace XovoeJ.Api.Managements
{
    /// <summary>
    /// 购物车控制器
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [ApiGroup(ApiGroupNames.USER)]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        /// <summary>
        /// 获取购物车列表
        /// </summary>
        /// <returns>购物车信息</returns>
        [HttpGet("api/cart")]
        [HttpGet("api/mall/cart")]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var cart = await _cartService.GetCartAsync(userId);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取购物车失败");
                return BadRequest(new { message = "获取购物车失败" });
            }
        }

        /// <summary>
        /// 获取购物车商品数量
        /// </summary>
        /// <returns>商品数量</returns>
        [HttpGet("api/cart/count")]
        [HttpGet("api/mall/cart/count")]
        public async Task<IActionResult> GetCartCount()
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var count = await _cartService.GetCartCountAsync(userId);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取购物车数量失败");
                return BadRequest(new { message = "获取购物车数量失败" });
            }
        }

        /// <summary>
        /// 添加到购物车
        /// </summary>
        /// <param name="request">添加请求</param>
        /// <returns></returns>
        [HttpPost("api/cart")]
        [HttpPost("api/mall/cart")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequestDto request)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                await _cartService.AddToCartAsync(userId, request);
                return Ok(new { message = "添加成功" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加购物车失败");
                return BadRequest(new { message = "添加购物车失败" });
            }
        }

        /// <summary>
        /// 更新购物车商品数量
        /// </summary>
        /// <param name="cartItemId">购物车项ID</param>
        /// <param name="request">更新请求</param>
        /// <returns></returns>
        [HttpPut("api/cart/{cartItemId}")]
        [HttpPut("api/mall/cart/{cartItemId}")]
        public async Task<IActionResult> UpdateQuantity(string cartItemId, [FromBody] UpdateCartQuantityRequestDto request)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                await _cartService.UpdateQuantityAsync(userId, cartItemId, request);
                return Ok(new { message = "更新成功" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新购物车失败: {CartItemId}", cartItemId);
                return BadRequest(new { message = "更新购物车失败" });
            }
        }

        /// <summary>
        /// 删除购物车商品
        /// </summary>
        /// <param name="cartItemId">购物车项ID</param>
        /// <returns></returns>
        [HttpDelete("api/cart/{cartItemId}")]
        [HttpDelete("api/mall/cart/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(string cartItemId)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                await _cartService.RemoveFromCartAsync(userId, cartItemId);
                return Ok(new { message = "删除成功" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除购物车商品失败: {CartItemId}", cartItemId);
                return BadRequest(new { message = "删除购物车商品失败" });
            }
        }

        /// <summary>
        /// 清空购物车
        /// </summary>
        /// <returns></returns>
        [HttpDelete("api/cart")]
        [HttpDelete("api/mall/cart")]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                await _cartService.ClearCartAsync(userId);
                return Ok(new { message = "清空成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清空购物车失败");
                return BadRequest(new { message = "清空购物车失败" });
            }
        }
    }
}
