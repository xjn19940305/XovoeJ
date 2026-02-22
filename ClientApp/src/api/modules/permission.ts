import api from '../index'

export default {
  // 获取权限列表
  getList: (params?: Api.Permission.PermissionListParams) => api.get<Api.Permission.PermissionListResponse>('permission/list', { params }),

  // 获取权限树
  getTree: () => api.get<Api.Permission.PermissionTreeResponse>('permission/tree'),

  // 获取角色权限
  getRolePermissions: (roleId: number) => api.get<number[]>(`permission/role/${roleId}`),

  // 保存角色权限
  saveRolePermissions: (roleId: number, permissionIds: number[]) => api.post('permission/role/save', { roleId, permissionIds }),

  // 创建权限
  create: (data: Api.Permission.PermissionFormData) => api.post('permission/create', data),

  // 更新权限
  update: (data: Api.Permission.PermissionFormData) => api.post('permission/update', data),

  // 删除权限
  delete: (id: number) => api.post('permission/delete', { id }),
}
