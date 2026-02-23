declare namespace Api {
  namespace Dictionary {
    // 字典类型枚举
    enum DictionaryType {
      System = 0,
      Business = 1,
    }

    // 数据类型枚举
    enum DictionaryDataType {
      String = 0,
      Number = 1,
      Boolean = 2,
      Json = 3,
    }

    interface DictionaryGroup {
      id: string
      code: string
      name: string
      description?: string
      type: DictionaryType
      parentId?: string
      level: number
      path?: string
      icon?: string
      sortOrder: number
      isEnabled: boolean
      remark?: string
      childrenCount: number
      itemsCount: number
      createdAt: string
      updatedAt?: string
    }

    interface DictionaryGroupTree {
      id: string
      code: string
      name: string
      description?: string
      type: DictionaryType
      level: number
      icon?: string
      sortOrder: number
      isEnabled: boolean
      children: DictionaryGroupTree[]
      items: DictionaryItem[]
    }

    interface DictionaryItem {
      id: string
      groupId: string
      groupCode?: string
      groupName?: string
      key: string
      name: string
      value?: string
      dataType: DictionaryDataType
      defaultValue?: string
      placeholder?: string
      description?: string
      options?: string
      validation?: string
      isSystem: boolean
      isRequired: boolean
      isEnabled: boolean
      sortOrder: number
      remark?: string
      createdAt: string
      updatedAt?: string
    }

    interface CreateDictionaryGroupRequest {
      code: string
      name: string
      description?: string
      type: DictionaryType
      parentId?: string
      icon?: string
      sortOrder: number
      isEnabled: boolean
      remark?: string
    }

    interface UpdateDictionaryGroupRequest {
      name: string
      description?: string
      icon?: string
      sortOrder: number
      isEnabled: boolean
      remark?: string
    }

    interface CreateDictionaryItemRequest {
      groupId: string
      key: string
      name: string
      value?: string
      dataType: DictionaryDataType
      defaultValue?: string
      placeholder?: string
      description?: string
      options?: string
      validation?: string
      isRequired: boolean
      isEnabled: boolean
      sortOrder: number
      remark?: string
    }

    interface UpdateDictionaryItemRequest {
      key: string
      name: string
      value?: string
      dataType: DictionaryDataType
      defaultValue?: string
      placeholder?: string
      description?: string
      options?: string
      validation?: string
      isRequired: boolean
      isEnabled: boolean
      sortOrder: number
      remark?: string
    }
  }
}
