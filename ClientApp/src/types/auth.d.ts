declare namespace Api {
  namespace Auth {
    interface LoginRequest {
      account: string
      password: string
    }

    interface UserInfo {
      id: string
      userName?: string
      nickName?: string
      email?: string
      phoneNumber?: string
      realName?: string
      picture?: string
      roles: string[]
      permissions: string[]
      isProfileCompleted: boolean
      createdAt: string
      lastSignDate?: string
    }

    interface LoginResponse {
      accessToken: string
      refreshToken: string
      expiresAt: string
      user: UserInfo
    }
  }
}
