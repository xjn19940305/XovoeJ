import api, { buildAdminApiPath } from '../index'

export default {
  // 获取商品列表
  getList: (params: Api.Product.ProductListParams) => api.get<Api.Product.ProductListResponse>(buildAdminApiPath('products'), { params }),

  // 获取商品详情
  getDetail: (id: string) => api.get<Api.Product.Product>(buildAdminApiPath(`products/${id}`)),

  // 创建商品
  create: (data: Api.Product.CreateProductRequest) => api.post(buildAdminApiPath('products'), data),

  // 更新商品
  update: (id: string, data: Api.Product.UpdateProductRequest) => api.put(buildAdminApiPath(`products/${id}`), data),

  // 删除商品
  delete: (id: string) => api.delete(buildAdminApiPath(`products/${id}`)),
}
