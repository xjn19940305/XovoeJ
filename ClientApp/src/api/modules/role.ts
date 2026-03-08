import api, { buildAdminApiPath } from '../index'

export default {
  // 获取角色列表
  getList: (params?: Api.Role.RoleListParams) => api.get<Api.Role.Role[]>(buildAdminApiPath('roles'), { params }),

  // 获取角色详情
  getDetail: (roleId: string) => api.get<Api.Role.Role>(buildAdminApiPath(`roles/${roleId}`)),

  // 创建角色
  create: (data: Api.Role.CreateRoleRequest) => api.post(buildAdminApiPath('roles'), data),

  // 更新角色
  update: (roleId: string, data: Api.Role.UpdateRoleRequest) => api.put(buildAdminApiPath(`roles/${roleId}`), data),

  // 删除角色
  delete: (roleId: string) => api.delete(buildAdminApiPath(`roles/${roleId}`)),

  // 获取角色权限
  getPermissions: (roleId: string) => api.get<string[]>(buildAdminApiPath(`roles/${roleId}/permissions`)),

  // 分配权限
  assignPermissions: (data: Api.Role.AssignPermissionsRequest) => api.post(buildAdminApiPath('roles/assign-permissions'), data),
}
