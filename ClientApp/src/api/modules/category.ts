import api from '../index'

export default {
  // 获取分类列表（树形）
  getTree: () => api.get<Api.Category.CategoryTreeNode[]>('api/categories/tree'),

  // 获取分类列表（平铺）
  getList: () => api.get<Api.Category.Category[]>('api/categories'),

  // 获取分类详情
  getDetail: (id: string) => api.get<Api.Category.Category>(`api/categories/${id}`),

  // 创建分类
  create: (data: Api.Category.CreateCategoryRequest) => api.post('api/categories', data),

  // 更新分类
  update: (id: string, data: Api.Category.UpdateCategoryRequest) => api.put(`api/categories/${id}`, data),

  // 删除分类
  delete: (id: string) => api.delete(`api/categories/${id}`),
}
