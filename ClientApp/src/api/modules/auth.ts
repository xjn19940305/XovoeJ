import api, { buildAdminApiPath } from '../index'

export default {
  // 登录
  login: (data: Api.Auth.LoginRequest) => api.post<Api.Auth.LoginResponse>(buildAdminApiPath('auth/login'), data),

  // 获取当前用户信息
  getUserInfo: () => api.get<Api.Auth.UserInfo>(buildAdminApiPath('auth/user-info')),

  // 退出登录
  logout: (refreshToken?: string) => api.post(buildAdminApiPath('auth/logout'), { RefreshToken: refreshToken }),

  // 刷新Token
  refreshToken: (data: { userId: string, refreshToken: string }) => api.post<Api.Auth.LoginResponse>(buildAdminApiPath('auth/refresh-token'), {
    UserId: data.userId,
    RefreshToken: data.refreshToken,
  }),
}
