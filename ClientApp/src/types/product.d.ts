declare namespace Api {
  namespace Product {
    interface Product {
      id: string
      name: string
      description?: string
      price: number
      stock: number
      categoryId?: string
      categoryName?: string
      coverImage?: string
      images?: string[]
      status: number // 0-下架 1-上架
      createdAt?: string
      updatedAt?: string
    }

    interface ProductListParams {
      page: number
      pageSize: number
      keyword?: string
      categoryId?: string
      status?: number
    }

    interface ProductListResponse {
      items: Product[]
      total: number
      page: number
      pageSize: number
    }

    interface CreateProductRequest {
      name: string
      description?: string
      price: number
      stock: number
      categoryId?: string
      coverImage?: string
      images?: string[]
      status: number
    }

    interface UpdateProductRequest {
      name?: string
      description?: string
      price?: number
      stock?: number
      categoryId?: string
      coverImage?: string
      images?: string[]
      status?: number
    }
  }
}
