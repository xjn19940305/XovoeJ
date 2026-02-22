using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XovoeJ.Abstractions.Services;
using XovoeJ.Api.Swaggers;
using XovoeJ.Contracts.Product;

namespace XovoeJ.Api.Managements
{
    /// <summary>
    /// 商品管理控制器
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [ApiGroup(ApiGroupNames.MANAGEMENT)]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        #region 分类管理

        /// <summary>
        /// 获取分类列表
        /// </summary>
        /// <returns>分类列表</returns>
        [HttpGet("api/categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _productService.GetCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分类列表失败");
                return BadRequest(new { message = "获取分类列表失败" });
            }
        }

        /// <summary>
        /// 获取分类树
        /// </summary>
        /// <returns>分类树</returns>
        [HttpGet("api/categories/tree")]
        public async Task<IActionResult> GetCategoryTree()
        {
            try
            {
                var categories = await _productService.GetCategoryTreeAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分类树失败");
                return BadRequest(new { message = "获取分类树失败" });
            }
        }

        /// <summary>
        /// 根据ID获取分类
        /// </summary>
        /// <param name="categoryId">分类ID</param>
        /// <returns>分类信息</returns>
        [HttpGet("api/categories/{categoryId}")]
        public async Task<IActionResult> GetCategoryById(string categoryId)
        {
            try
            {
                var category = await _productService.GetCategoryByIdAsync(categoryId);
                if (category == null)
                {
                    return NotFound(new { message = "分类不存在" });
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分类信息失败: {CategoryId}", categoryId);
                return BadRequest(new { message = "获取分类信息失败" });
            }
        }

        /// <summary>
        /// 创建分类
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>创建的分类信息</returns>
        [HttpPost("api/categories")]
        [Authorize]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequestDto request)
        {
            try
            {
                var category = await _productService.CreateCategoryAsync(request);
                return Ok(category);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建分类失败: {CategoryName}", request.Name);
                return BadRequest(new { message = "创建分类失败" });
            }
        }

        /// <summary>
        /// 更新分类
        /// </summary>
        /// <param name="categoryId">分类ID</param>
        /// <param name="request">更新请求</param>
        /// <returns>更新后的分类信息</returns>
        [HttpPut("api/categories/{categoryId}")]
        [Authorize]
        public async Task<IActionResult> UpdateCategory(string categoryId, [FromBody] UpdateCategoryRequestDto request)
        {
            try
            {
                var category = await _productService.UpdateCategoryAsync(categoryId, request);
                if (category == null)
                {
                    return NotFound(new { message = "分类不存在" });
                }
                return Ok(category);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新分类失败: {CategoryId}", categoryId);
                return BadRequest(new { message = "更新分类失败" });
            }
        }

        /// <summary>
        /// 删除分类
        /// </summary>
        /// <param name="categoryId">分类ID</param>
        /// <returns></returns>
        [HttpDelete("api/categories/{categoryId}")]
        [Authorize]
        public async Task<IActionResult> DeleteCategory(string categoryId)
        {
            try
            {
                var result = await _productService.DeleteCategoryAsync(categoryId);
                if (!result)
                {
                    return NotFound(new { message = "分类不存在" });
                }
                return Ok(new { message = "删除成功" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除分类失败: {CategoryId}", categoryId);
                return BadRequest(new { message = "删除分类失败" });
            }
        }

        #endregion

        #region 商品管理

        /// <summary>
        /// 获取商品列表（分页）
        /// </summary>
        /// <param name="query">查询参数</param>
        /// <returns>商品列表</returns>
        [HttpGet("api/products")]
        public async Task<IActionResult> GetProducts([FromQuery] ProductListQueryDto query)
        {
            try
            {
                var result = await _productService.GetProductsAsync(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取商品列表失败");
                return BadRequest(new { message = "获取商品列表失败" });
            }
        }

        /// <summary>
        /// 根据ID获取商品
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <returns>商品信息</returns>
        [HttpGet("api/products/{productId}")]
        public async Task<IActionResult> GetProductById(string productId)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return NotFound(new { message = "商品不存在" });
                }

                // 增加浏览量
                await _productService.IncrementBrowseCountAsync(productId);

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取商品信息失败: {ProductId}", productId);
                return BadRequest(new { message = "获取商品信息失败" });
            }
        }

        /// <summary>
        /// 创建商品
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>创建的商品信息</returns>
        [HttpPost("api/products")]
        [Authorize]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequestDto request)
        {
            try
            {
                var product = await _productService.CreateProductAsync(request);
                return Ok(product);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建商品失败: {ProductName}", request.Name);
                return BadRequest(new { message = "创建商品失败" });
            }
        }

        /// <summary>
        /// 更新商品
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="request">更新请求</param>
        /// <returns>更新后的商品信息</returns>
        [HttpPut("api/products/{productId}")]
        [Authorize]
        public async Task<IActionResult> UpdateProduct(string productId, [FromBody] UpdateProductRequestDto request)
        {
            try
            {
                var product = await _productService.UpdateProductAsync(productId, request);
                if (product == null)
                {
                    return NotFound(new { message = "商品不存在" });
                }
                return Ok(product);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新商品失败: {ProductId}", productId);
                return BadRequest(new { message = "更新商品失败" });
            }
        }

        /// <summary>
        /// 删除商品
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <returns></returns>
        [HttpDelete("api/products/{productId}")]
        [Authorize]
        public async Task<IActionResult> DeleteProduct(string productId)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(productId);
                if (!result)
                {
                    return NotFound(new { message = "商品不存在" });
                }
                return Ok(new { message = "删除成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除商品失败: {ProductId}", productId);
                return BadRequest(new { message = "删除商品失败" });
            }
        }

        #endregion
    }
}
