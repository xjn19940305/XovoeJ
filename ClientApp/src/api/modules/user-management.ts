import api from '../index'

export default {
  // 获取用户列表
  getList: (params: Api.User.UserListParams) => api.get<Api.User.UserListResponse>('api/users', { params }),

  // 获取用户详情
  getDetail: (id: string) => api.get<Api.User.User>(`api/users/${id}`),

  // 创建用户
  create: (data: Api.User.CreateUserRequest) => api.post('api/users', data),

  // 更新用户
  update: (id: string, data: Api.User.UpdateUserRequest) => api.put(`api/users/${id}`, data),

  // 删除用户
  delete: (id: string) => api.delete(`api/users/${id}`),

  // 分配角色
  assignRoles: (data: { userId: string; roleNames: string[] }) => api.post('api/users/assign-roles', data),

  // 重置密码
  resetPassword: (id: string, password: string) => api.post(`api/users/${id}/reset-password`, password),

  // 锁定用户
  lock: (id: string) => api.post(`api/users/${id}/lock`),

  // 解锁用户
  unlock: (id: string) => api.post(`api/users/${id}/unlock`),
}
