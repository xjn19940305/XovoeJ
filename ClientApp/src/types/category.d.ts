declare namespace Api {
  namespace Category {
    interface Category {
      id: string
      name: string
      parentId?: string
      level: number
      path?: string
      icon?: string
      image?: string
      sortOrder: number
      isEnabled: boolean
      children?: Category[]
    }

    interface CategoryTreeNode extends Category {
      children: CategoryTreeNode[]
    }

    interface CreateCategoryRequest {
      name: string
      parentId?: string
      icon?: string
      image?: string
      sortOrder: number
    }

    interface UpdateCategoryRequest {
      name: string
      icon?: string
      image?: string
      sortOrder: number
      isEnabled: boolean
    }
  }
}
