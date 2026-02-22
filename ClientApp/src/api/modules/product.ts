import api from '../index'

export default {
  // 获取商品列表
  getList: (params: Api.Product.ProductListParams) => api.get<Api.Product.ProductListResponse>('api/products', { params }),

  // 获取商品详情
  getDetail: (id: string) => api.get<Api.Product.Product>(`api/products/${id}`),

  // 创建商品
  create: (data: Api.Product.CreateProductRequest) => api.post('api/products', data),

  // 更新商品
  update: (id: string, data: Api.Product.UpdateProductRequest) => api.put(`api/products/${id}`, data),

  // 删除商品
  delete: (id: string) => api.delete(`api/products/${id}`),
}
