using XovoeJ.Contracts.Product;

namespace XovoeJ.Abstractions.Services
{
    /// <summary>
    /// 商品服务接口
    /// </summary>
    public interface IProductService
    {
        #region 分类管理

        /// <summary>
        /// 获取分类列表
        /// </summary>
        /// <returns>分类列表</returns>
        Task<List<CategoryDto>> GetCategoriesAsync();

        /// <summary>
        /// 获取分类树
        /// </summary>
        /// <returns>分类树</returns>
        Task<List<CategoryDto>> GetCategoryTreeAsync();

        /// <summary>
        /// 根据ID获取分类
        /// </summary>
        /// <param name="categoryId">分类ID</param>
        /// <returns>分类信息</returns>
        Task<CategoryDto?> GetCategoryByIdAsync(string categoryId);

        /// <summary>
        /// 创建分类
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>创建的分类信息</returns>
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequestDto request);

        /// <summary>
        /// 更新分类
        /// </summary>
        /// <param name="categoryId">分类ID</param>
        /// <param name="request">更新请求</param>
        /// <returns>更新后的分类信息</returns>
        Task<CategoryDto?> UpdateCategoryAsync(string categoryId, UpdateCategoryRequestDto request);

        /// <summary>
        /// 删除分类
        /// </summary>
        /// <param name="categoryId">分类ID</param>
        /// <returns></returns>
        Task<bool> DeleteCategoryAsync(string categoryId);

        #endregion

        #region 商品管理

        /// <summary>
        /// 获取商品列表（分页）
        /// </summary>
        /// <param name="query">查询参数</param>
        /// <returns>商品列表</returns>
        Task<ProductListResponseDto> GetProductsAsync(ProductListQueryDto query);

        /// <summary>
        /// 根据ID获取商品
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <returns>商品信息</returns>
        Task<ProductDto?> GetProductByIdAsync(string productId);

        /// <summary>
        /// 创建商品
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>创建的商品信息</returns>
        Task<ProductDto> CreateProductAsync(CreateProductRequestDto request);

        /// <summary>
        /// 更新商品
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="request">更新请求</param>
        /// <returns>更新后的商品信息</returns>
        Task<ProductDto?> UpdateProductAsync(string productId, UpdateProductRequestDto request);

        /// <summary>
        /// 删除商品
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <returns></returns>
        Task<bool> DeleteProductAsync(string productId);

        /// <summary>
        /// 更新商品浏览量
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <returns></returns>
        Task IncrementBrowseCountAsync(string productId);

        #endregion
    }
}
