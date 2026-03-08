import api, { buildAdminApiPath } from '../index'

export default {
  getList: (params?: Api.Permission.PermissionListParams) => api.get<Api.Permission.PermissionListResponse>(buildAdminApiPath('permissions'), { params }),

  getTree: () => api.get<Api.Permission.PermissionTreeResponse>(buildAdminApiPath('permissions/tree')),
}
