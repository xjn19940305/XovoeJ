declare namespace Api {
  namespace Category {
    interface Category {
      id: string
      name: string
      description?: string
      icon?: string
      sort: number
      parentId?: string
      parentName?: string
      createdAt?: string
      updatedAt?: string
    }

    interface CategoryTreeNode extends Category {
      children?: CategoryTreeNode[]
    }

    interface CreateCategoryRequest {
      name: string
      description?: string
      icon?: string
      sort: number
      parentId?: string
    }

    interface UpdateCategoryRequest {
      name?: string
      description?: string
      icon?: string
      sort?: number
      parentId?: string
    }
  }
}
