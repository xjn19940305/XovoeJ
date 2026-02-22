declare namespace Api {
  namespace User {
    interface User {
      id: string
      userName: string
      email?: string
      phoneNumber?: string
      realName?: string
      nickName?: string
      avatar?: string
      lockoutEnd?: string
      roles: string[] // 角色名称列表
      createdAt?: string
      updatedAt?: string
    }

    interface UserListParams {
      page: number
      pageSize: number
      keyword?: string
      roleName?: string
    }

    interface UserListResponse {
      items: User[]
      total: number
      page: number
      pageSize: number
    }

    interface CreateUserRequest {
      userName: string
      email?: string
      phoneNumber?: string
      realName?: string
      nickName?: string
      password: string
    }

    interface UpdateUserRequest {
      email?: string
      phoneNumber?: string
      realName?: string
      nickName?: string
    }
  }
}
