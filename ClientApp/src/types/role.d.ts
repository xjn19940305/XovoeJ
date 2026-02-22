declare namespace Api {
  namespace Role {
    interface Role {
      id: string
      name: string
      description?: string
      sort?: number
      createdAt: string
      updateAt?: string
    }

    interface RoleListParams {
      page?: number
      pageSize?: number
      name?: string
    }

    interface RoleListResponse {
      list: Role[]
      total: number
    }

    interface CreateRoleRequest {
      name: string
      description?: string
      sort?: number
    }

    interface UpdateRoleRequest {
      name: string
      description?: string
      sort?: number
    }

    interface AssignPermissionsRequest {
      roleId: string
      permissionCodes: string[]
    }
  }
}
