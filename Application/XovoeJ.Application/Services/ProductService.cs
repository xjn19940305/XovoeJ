using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using XovoeJ.Abstractions.Services;
using XovoeJ.Contracts.Product;
using XovoeJ.Entities;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Application.Services
{
    /// <summary>
    /// 商品服务实现
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly XovoeJDbContext _dbContext;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            XovoeJDbContext dbContext,
            ILogger<ProductService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        #region 分类管理

        /// <summary>
        /// 获取分类列表
        /// </summary>
        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            var categories = await _dbContext.Categories
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            return categories.Select(MapToCategoryDto).ToList();
        }

        /// <summary>
        /// 获取分类树
        /// </summary>
        public async Task<List<CategoryDto>> GetCategoryTreeAsync()
        {
            var categories = await _dbContext.Categories
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            var categoryMap = categories.ToDictionary(c => c.Id, c => MapToCategoryDto(c));
            var rootCategories = new List<CategoryDto>();

            foreach (var category in categories)
            {
                var dto = categoryMap[category.Id];
                if (!string.IsNullOrEmpty(category.ParentId) && categoryMap.TryGetValue(category.ParentId, out var parent))
                {
                    parent.Children.Add(dto);
                }
                else
                {
                    rootCategories.Add(dto);
                }
            }

            return rootCategories;
        }

        /// <summary>
        /// 根据ID获取分类
        /// </summary>
        public async Task<CategoryDto?> GetCategoryByIdAsync(string categoryId)
        {
            var category = await _dbContext.Categories.FindAsync(categoryId);
            return category != null ? MapToCategoryDto(category) : null;
        }

        /// <summary>
        /// 创建分类
        /// </summary>
        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequestDto request)
        {
            int level = 1;
            string? path = null;

            if (!string.IsNullOrEmpty(request.ParentId))
            {
                var parent = await _dbContext.Categories.FindAsync(request.ParentId);
                if (parent == null)
                {
                    throw new ArgumentException("父分类不存在");
                }
                level = parent.Level + 1;
                path = string.IsNullOrEmpty(parent.Path) ? parent.Id : $"{parent.Path}/{parent.Id}";
            }

            var category = new Category
            {
                Name = request.Name,
                ParentId = request.ParentId,
                Level = level,
                Path = path,
                Icon = request.Icon,
                Image = request.Image,
                SortOrder = request.SortOrder,
                IsEnabled = true
            };

            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync();

            return MapToCategoryDto(category);
        }

        /// <summary>
        /// 更新分类
        /// </summary>
        public async Task<CategoryDto?> UpdateCategoryAsync(string categoryId, UpdateCategoryRequestDto request)
        {
            var category = await _dbContext.Categories.FindAsync(categoryId);
            if (category == null)
            {
                return null;
            }

            category.Name = request.Name;
            category.Icon = request.Icon;
            category.Image = request.Image;
            category.SortOrder = request.SortOrder;
            category.IsEnabled = request.IsEnabled;
            category.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return MapToCategoryDto(category);
        }

        /// <summary>
        /// 删除分类
        /// </summary>
        public async Task<bool> DeleteCategoryAsync(string categoryId)
        {
            var category = await _dbContext.Categories.FindAsync(categoryId);
            if (category == null)
            {
                return false;
            }

            // 检查是否有子分类
            var hasChildren = await _dbContext.Categories.AnyAsync(c => c.ParentId == categoryId);
            if (hasChildren)
            {
                throw new ArgumentException("该分类下还有子分类，无法删除");
            }

            // 检查是否有商品
            var hasProducts = await _dbContext.Products.AnyAsync(p => p.CategoryId == categoryId);
            if (hasProducts)
            {
                throw new ArgumentException("该分类下还有商品，无法删除");
            }

            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        #endregion

        #region 商品管理

        /// <summary>
        /// 获取商品列表（分页）
        /// </summary>
        public async Task<ProductListResponseDto> GetProductsAsync(ProductListQueryDto query)
        {
            var queryable = _dbContext.Products
                .Include(p => p.Category)
                .Include(p => p.Skus)
                .AsQueryable();

            // 筛选条件
            if (!string.IsNullOrEmpty(query.CategoryId))
            {
                queryable = queryable.Where(p => p.CategoryId == query.CategoryId);
            }

            if (!string.IsNullOrEmpty(query.Keyword))
            {
                queryable = queryable.Where(p =>
                    (p.Name != null && p.Name.Contains(query.Keyword)) ||
                    (p.Subtitle != null && p.Subtitle.Contains(query.Keyword)) ||
                    (p.Description != null && p.Description.Contains(query.Keyword)));
            }

            if (query.IsHot.HasValue)
            {
                queryable = queryable.Where(p => p.IsHot == query.IsHot.Value);
            }

            if (query.IsNew.HasValue)
            {
                queryable = queryable.Where(p => p.IsNew == query.IsNew.Value);
            }

            if (query.IsRecommend.HasValue)
            {
                queryable = queryable.Where(p => p.IsRecommend == query.IsRecommend.Value);
            }

            // 排序
            if (!string.IsNullOrEmpty(query.PriceSort))
            {
                // 需要通过SKU的价格来排序，这里先按商品的其他字段排序
                if (query.PriceSort.ToLower() == "asc")
                {
                    queryable = queryable.OrderBy(p => p.Skus.Min(s => s.Price));
                }
                else
                {
                    queryable = queryable.OrderByDescending(p => p.Skus.Max(s => s.Price));
                }
            }
            else if (!string.IsNullOrEmpty(query.SalesSort))
            {
                if (query.SalesSort.ToLower() == "asc")
                {
                    queryable = queryable.OrderBy(p => p.SalesCount);
                }
                else
                {
                    queryable = queryable.OrderByDescending(p => p.SalesCount);
                }
            }
            else
            {
                queryable = queryable.OrderByDescending(p => p.CreatedAt);
            }

            // 总数
            var total = await queryable.CountAsync();

            // 分页
            var products = await queryable
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var items = products.Select(MapToProductDto).ToList();

            return new ProductListResponseDto
            {
                Items = items,
                Total = total,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }

        /// <summary>
        /// 根据ID获取商品
        /// </summary>
        public async Task<ProductDto?> GetProductByIdAsync(string productId)
        {
            var product = await _dbContext.Products
                .Include(p => p.Category)
                .Include(p => p.Skus)
                .FirstOrDefaultAsync(p => p.Id == productId);

            return product != null ? MapToProductDto(product) : null;
        }

        /// <summary>
        /// 创建商品
        /// </summary>
        public async Task<ProductDto> CreateProductAsync(CreateProductRequestDto request)
        {
            // 验证分类是否存在
            var category = await _dbContext.Categories.FindAsync(request.CategoryId);
            if (category == null)
            {
                throw new ArgumentException("分类不存在");
            }

            var product = new Product
            {
                CategoryId = request.CategoryId,
                Name = request.Name,
                Subtitle = request.Subtitle,
                Description = request.Description,
                MainImage = request.MainImage,
                Images = request.Images != null ? JsonSerializer.Serialize(request.Images) : null,
                Detail = request.Detail,
                IsEnabled = true,
                IsHot = request.IsHot,
                IsNew = request.IsNew,
                IsRecommend = request.IsRecommend
            };

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            // 创建SKU
            foreach (var skuDto in request.Skus)
            {
                var sku = new ProductSku
                {
                    ProductId = product.Id,
                    SkuCode = skuDto.SkuCode,
                    Specs = skuDto.Specs != null ? JsonSerializer.Serialize(skuDto.Specs) : null,
                    Price = skuDto.Price,
                    OriginalPrice = skuDto.OriginalPrice,
                    CostPrice = skuDto.CostPrice,
                    Stock = skuDto.Stock,
                    LowStock = skuDto.LowStock,
                    Image = skuDto.Image
                };
                _dbContext.ProductSkus.Add(sku);
            }

            await _dbContext.SaveChangesAsync();

            // 重新加载包含SKU的商品
            product = await _dbContext.Products
                .Include(p => p.Category)
                .Include(p => p.Skus)
                .FirstAsync(p => p.Id == product.Id);

            return MapToProductDto(product);
        }

        /// <summary>
        /// 更新商品
        /// </summary>
        public async Task<ProductDto?> UpdateProductAsync(string productId, UpdateProductRequestDto request)
        {
            var product = await _dbContext.Products
                .Include(p => p.Category)
                .Include(p => p.Skus)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return null;
            }

            // 验证分类是否存在
            var category = await _dbContext.Categories.FindAsync(request.CategoryId);
            if (category == null)
            {
                throw new ArgumentException("分类不存在");
            }

            product.CategoryId = request.CategoryId;
            product.Name = request.Name;
            product.Subtitle = request.Subtitle;
            product.Description = request.Description;
            product.MainImage = request.MainImage;
            product.Images = request.Images != null ? JsonSerializer.Serialize(request.Images) : null;
            product.Detail = request.Detail;
            product.IsEnabled = request.IsEnabled;
            product.IsHot = request.IsHot;
            product.IsNew = request.IsNew;
            product.IsRecommend = request.IsRecommend;
            product.UpdatedAt = DateTime.UtcNow;

            // 更新 SKU（根据 SkuCode 匹配，保留 ID 不变）
            if (request.Skus != null)
            {
                var existingSkus = await _dbContext.ProductSkus
                    .Where(s => s.ProductId == productId)
                    .ToListAsync();

                foreach (var skuDto in request.Skus)
                {
                    var existingSku = existingSkus.FirstOrDefault(s => s.SkuCode == skuDto.SkuCode);

                    if (existingSku != null)
                    {
                        // 更新现有 SKU（保留 ID）
                        existingSku.Specs = skuDto.Specs != null ? JsonSerializer.Serialize(skuDto.Specs) : null;
                        existingSku.Price = skuDto.Price;
                        existingSku.OriginalPrice = skuDto.OriginalPrice;
                        existingSku.CostPrice = skuDto.CostPrice;
                        existingSku.Stock = skuDto.Stock;
                        existingSku.LowStock = skuDto.LowStock;
                        existingSku.Image = skuDto.Image;
                    }
                    else
                    {
                        // 添加新 SKU
                        var sku = new ProductSku
                        {
                            Id = Guid.NewGuid().ToString(),
                            ProductId = productId,
                            SkuCode = skuDto.SkuCode,
                            Specs = skuDto.Specs != null ? JsonSerializer.Serialize(skuDto.Specs) : null,
                            Price = skuDto.Price,
                            OriginalPrice = skuDto.OriginalPrice,
                            CostPrice = skuDto.CostPrice,
                            Stock = skuDto.Stock,
                            LowStock = skuDto.LowStock,
                            Image = skuDto.Image,
                        };
                        await _dbContext.ProductSkus.AddAsync(sku);
                    }
                }

                // 删除不在更新列表中的 SKU
                var updatedSkuCodes = request.Skus.Select(s => s.SkuCode).ToHashSet();
                var skusToDelete = existingSkus.Where(s => !updatedSkuCodes.Contains(s.SkuCode)).ToList();
                _dbContext.ProductSkus.RemoveRange(skusToDelete);
            }

            await _dbContext.SaveChangesAsync();

            // 重新加载包含新分类的商品
            product = await _dbContext.Products
                .Include(p => p.Category)
                .Include(p => p.Skus)
                .FirstAsync(p => p.Id == product.Id);

            return MapToProductDto(product);
        }



        /// <summary>
        /// 删除商品
        /// </summary>
        public async Task<bool> DeleteProductAsync(string productId)
        {
            var product = await _dbContext.Products.FindAsync(productId);
            if (product == null)
            {
                return false;
            }

            // 删除关联的SKU
            var skus = await _dbContext.ProductSkus.Where(s => s.ProductId == productId).ToListAsync();
            _dbContext.ProductSkus.RemoveRange(skus);

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 更新商品浏览量
        /// </summary>
        public async Task IncrementBrowseCountAsync(string productId)
        {
            var product = await _dbContext.Products.FindAsync(productId);
            if (product != null)
            {
                product.BrowseCount++;
                await _dbContext.SaveChangesAsync();
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 映射分类实体到DTO
        /// </summary>
        private static CategoryDto MapToCategoryDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                ParentId = category.ParentId,
                Level = category.Level,
                Path = category.Path,
                Icon = category.Icon,
                Image = category.Image,
                SortOrder = category.SortOrder,
                IsEnabled = category.IsEnabled
            };
        }

        /// <summary>
        /// 映射商品实体到DTO
        /// </summary>
        private static ProductDto MapToProductDto(Product product)
        {
            var images = !string.IsNullOrEmpty(product.Images)
                ? JsonSerializer.Deserialize<List<string>>(product.Images)
                : new List<string>();

            return new ProductDto
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                Name = product.Name,
                Subtitle = product.Subtitle,
                Description = product.Description,
                MainImage = product.MainImage,
                Images = images,
                Detail = product.Detail,
                SalesCount = product.SalesCount,
                BrowseCount = product.BrowseCount,
                IsEnabled = product.IsEnabled,
                IsHot = product.IsHot,
                IsNew = product.IsNew,
                IsRecommend = product.IsRecommend,
                CreatedAt = product.CreatedAt,
                Skus = product.Skus.Select(s => new ProductSkuDto
                {
                    Id = s.Id,
                    SkuCode = s.SkuCode,
                    Specs = !string.IsNullOrEmpty(s.Specs) ? JsonSerializer.Deserialize<Dictionary<string, string>>(s.Specs) : null,
                    Price = s.Price,
                    OriginalPrice = s.OriginalPrice,
                    CostPrice = s.CostPrice,
                    Stock = s.Stock,
                    LowStock = s.LowStock,
                    SalesCount = s.SalesCount,
                    Image = s.Image
                }).ToList()
            };
        }

        #endregion
    }
}
