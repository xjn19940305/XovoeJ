declare namespace Api {
  namespace Product {
    interface ProductSku {
      id: string
      skuCode: string
      specs?: Record<string, string>
      price: number
      originalPrice?: number
      costPrice?: number
      stock: number
      lowStock: number
      salesCount: number
      image?: string
    }

    interface Product {
      id: string
      categoryId: string
      categoryName?: string
      name: string
      subtitle?: string
      description?: string
      mainImage?: string
      images: string[]
      detail?: string
      tags?: string[]
      salesCount: number
      browseCount: number
      isEnabled: boolean
      isHot: boolean
      isNew: boolean
      isRecommend: boolean
      createdAt: string
      skus: ProductSku[]
      minPrice?: number
      maxPrice?: number
      totalStock: number
    }

    interface ProductListParams {
      page: number
      pageSize: number
      categoryId?: string
      keyword?: string
      isHot?: boolean
      isNew?: boolean
      isRecommend?: boolean
      priceSort?: 'asc' | 'desc'
      salesSort?: 'asc' | 'desc'
    }

    interface ProductListResponse {
      items: Product[]
      total: number
      page: number
      pageSize: number
      totalPages: number
    }

    interface CreateProductRequest {
      categoryId: string
      name: string
      subtitle?: string
      description?: string
      mainImage?: string
      images?: string[]
      detail?: string
      tags?: string[]
      isHot: boolean
      isNew: boolean
      isRecommend: boolean
      skus: CreateProductSkuRequest[]
    }

    interface CreateProductSkuRequest {
      skuCode: string
      specs?: Record<string, string>
      price: number
      originalPrice?: number
      costPrice?: number
      stock: number
      lowStock: number
      image?: string
    }

    interface UpdateProductRequest {
      categoryId: string
      name: string
      subtitle?: string
      description?: string
      mainImage?: string
      images?: string[]
      detail?: string
      tags?: string[]
      isEnabled: boolean
      isHot: boolean
      isNew: boolean
      isRecommend: boolean
      skus: UpdateProductSkuRequest[]
    }

    interface UpdateProductSkuRequest {
      skuCode: string
      specs?: Record<string, string>
      price: number
      originalPrice?: number
      costPrice?: number
      stock: number
      lowStock: number
      image?: string
    }
  }
}
