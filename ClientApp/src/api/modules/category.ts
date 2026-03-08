import api, { buildAdminApiPath } from '../index'

export default {
  // 获取分类列表
  getList: () => api.get<Api.Category.Category[]>(buildAdminApiPath('categories')),

  // 获取分类树
  getTree: () => api.get<Api.Category.CategoryTreeNode[]>(buildAdminApiPath('categories/tree')),

  // 获取分类详情
  getDetail: (id: string) => api.get<Api.Category.Category>(buildAdminApiPath(`categories/${id}`)),

  // 创建分类
  create: (data: Api.Category.CreateCategoryRequest) => api.post(buildAdminApiPath('categories'), data),

  // 更新分类
  update: (id: string, data: Api.Category.UpdateCategoryRequest) => api.put(buildAdminApiPath(`categories/${id}`), data),

  // 删除分类
  delete: (id: string) => api.delete(buildAdminApiPath(`categories/${id}`)),
}
