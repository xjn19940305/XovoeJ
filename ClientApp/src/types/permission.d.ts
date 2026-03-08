declare namespace Api {
  namespace Permission {
    type PermissionType = 'menu' | 'page' | 'button'

    interface Permission {
      id: number
      parentId: number
      name: string
      code: string
      type: PermissionType
      path?: string
      icon?: string
      sort: number
      status: 0 | 1
      children?: Permission[]
    }

    interface PermissionTree extends Permission {
      children?: PermissionTree[]
    }

    interface PermissionFormData {
      id?: number
      parentId: number
      name: string
      code: string
      type: PermissionType
      path?: string
      icon?: string
      sort: number
      status: 0 | 1
    }

    interface PermissionListParams {
      page?: number
      pageSize?: number
      name?: string
      code?: string
      type?: PermissionType
      status?: 0 | 1
    }

    interface PermissionListResponse {
      items: Permission[]
      total: number
      page: number
      pageSize: number
    }

    interface PermissionTreeResponse {
      items: PermissionTree[]
    }
  }
}
