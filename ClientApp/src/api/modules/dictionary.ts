import api from '../index'

export default {
  // ========== 分组管理 ==========

  // 获取分组列表
  getGroups: (type?: Api.Dictionary.DictionaryType) => api.get<Api.Dictionary.DictionaryGroup[]>('api/dictionary/groups', { params: { type } }),

  // 获取分组树
  getGroupTree: (type?: Api.Dictionary.DictionaryType) => api.get<Api.Dictionary.DictionaryGroupTree[]>('api/dictionary/groups/tree', { params: { type } }),

  // 根据编码获取分组
  getGroupByCode: (code: string) => api.get<Api.Dictionary.DictionaryGroup>(`api/dictionary/groups/by-code/${code}`),

  // 根据ID获取分组
  getGroupById: (groupId: string) => api.get<Api.Dictionary.DictionaryGroup>(`api/dictionary/groups/${groupId}`),

  // 创建分组
  createGroup: (data: Api.Dictionary.CreateDictionaryGroupRequest) => api.post<Api.Dictionary.DictionaryGroup>('api/dictionary/groups', data),

  // 更新分组
  updateGroup: (groupId: string, data: Api.Dictionary.UpdateDictionaryGroupRequest) => api.put<Api.Dictionary.DictionaryGroup>(`api/dictionary/groups/${groupId}`, data),

  // 删除分组
  deleteGroup: (groupId: string) => api.delete(`api/dictionary/groups/${groupId}`),

  // ========== 字典项管理 ==========

  // 获取字典项列表
  getItems: (groupId?: string, enabledOnly = false) => {
    const params: Record<string, any> = {}
    if (groupId)
      params.groupId = groupId
    if (enabledOnly)
      params.enabledOnly = enabledOnly
    return api.get<Api.Dictionary.DictionaryItem[]>('api/dictionary/items', { params })
  },

  // 根据分组编码获取字典项列表
  getItemsByGroupCode: (groupCode: string, enabledOnly?: boolean) => api.get<Api.Dictionary.DictionaryItem[]>(`api/dictionary/items/by-group/${groupCode}`, { params: { enabledOnly } }),

  // 根据键获取字典项
  getItemByKey: (key: string) => api.get<Api.Dictionary.DictionaryItem>(`api/dictionary/items/by-key/${key}`),

  // 批量获取字典项
  getItemsByKeys: (keys: string[]) => api.get<Record<string, Api.Dictionary.DictionaryItem>>('api/dictionary/items/batch', { params: { keys: keys.join(',') } }),

  // 创建字典项
  createItem: (data: Api.Dictionary.CreateDictionaryItemRequest) => api.post<Api.Dictionary.DictionaryItem>('api/dictionary/items', data),

  // 更新字典项
  updateItem: (itemId: string, data: Api.Dictionary.UpdateDictionaryItemRequest) => api.put<Api.Dictionary.DictionaryItem>(`api/dictionary/items/${itemId}`, data),

  // 删除字典项
  deleteItem: (itemId: string) => api.delete(`api/dictionary/items/${itemId}`),
}
