<script setup lang="ts">
import type { FormInstance, FormRules } from 'element-plus'
import { ElMessage, ElMessageBox } from 'element-plus'
import categoryApi from '@/api/modules/category'
import dayjs from 'dayjs'

defineOptions({
  name: 'ShopProductCategory',
})

// 搜索表单
const searchForm = ref({
  keyword: '',
})

// 表格数据
const tableData = ref<Api.Category.Category[]>([])
const loading = ref(false)

// 分类树数据
const categoryTree = ref<Api.Category.CategoryTreeNode[]>([])

// 对话框
const dialogVisible = ref(false)
const dialogTitle = ref('创建分类')
const dialogLoading = ref(false)
const editingCategoryId = ref<string>()
const formData = ref<Api.Category.CreateCategoryRequest>({
  name: '',
  icon: '',
  image: '',
  sortOrder: 0,
  parentId: '',
})

// 表单引用
const formRef = ref<FormInstance>()

// 表单验证规则
const rules: FormRules = {
  name: [
    { required: true, message: '请输入分类名称', trigger: 'blur' },
  ],
}

// 获取分类列表
async function getCategoryList() {
  loading.value = true
  try {
    const res = await categoryApi.getList()
    tableData.value = res.data
  }
  finally {
    loading.value = false
  }
}

// 获取分类树
async function getCategoryTree() {
  const res = await categoryApi.getTree()
  categoryTree.value = res.data
}

// 搜索
function handleSearch() {
  getCategoryList()
}

// 重置搜索
function handleReset() {
  searchForm.value = {
    keyword: '',
  }
  getCategoryList()
}

// 打开创建对话框
function handleCreate() {
  dialogTitle.value = '创建分类'
  editingCategoryId.value = undefined
  formData.value = {
    name: '',
    icon: '',
    image: '',
    sortOrder: 0,
    parentId: '',
  }
  dialogVisible.value = true
}

// 打开编辑对话框
async function handleEdit(row: Api.Category.Category) {
  dialogTitle.value = '修改分类'
  editingCategoryId.value = row.id
  dialogLoading.value = true
  dialogVisible.value = true
  try {
    const res = await categoryApi.getDetail(row.id)
    const data = res.data
    formData.value = {
      name: data.name,
      icon: data.icon,
      image: data.image,
      sortOrder: data.sortOrder,
      parentId: data.parentId,
    }
  }
  finally {
    dialogLoading.value = false
  }
}

// 删除分类
async function handleDelete(id: string) {
  await ElMessageBox.confirm('确定要删除该分类吗？删除后子分类也会被删除。', '提示', {
    type: 'warning',
  })
  await categoryApi.delete(id)
  ElMessage.success('删除成功')
  getCategoryList()
  getCategoryTree()
}

// 提交表单
async function handleSubmit() {
  await formRef.value?.validate()
  dialogLoading.value = true
  try {
    if (editingCategoryId.value) {
      await categoryApi.update(editingCategoryId.value, {
        name: formData.value.name,
        icon: formData.value.icon,
        image: formData.value.image,
        sortOrder: formData.value.sortOrder,
        isEnabled: true,
      })
      ElMessage.success('修改成功')
    }
    else {
      // 处理 parentId，空字符串转为 null
      const submitData = {
        ...formData.value,
        parentId: formData.value.parentId || undefined,
      }
      await categoryApi.create(submitData)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    getCategoryList()
    getCategoryTree()
  }
  finally {
    dialogLoading.value = false
  }
}

// 格式化分类层级
function getCategoryLevel(level: number) {
  return '　'.repeat(level) + (level > 0 ? '└ ' : '')
}

onMounted(() => {
  getCategoryList()
  getCategoryTree()
})
</script>

<template>
  <div class="shop-product-category">
    <!-- 搜索表单 -->
    <FaCard class="search-card mb-4">
      <div class="search-header">
        <div class="search-title">
          <FaIcon name="i-heroicons-solid:magnifying-glass" class="size-5" />
          <span>分类筛选</span>
        </div>
      </div>
      <div class="search-body">
        <div class="search-grid">
          <div class="search-field">
            <label class="search-label">关键字</label>
            <el-input
              v-model="searchForm.keyword"
              placeholder="搜索分类名称"
              clearable
            >
              <template #prefix>
                <FaIcon name="i-heroicons-solid:tag" class="size-4 text-gray-400" />
              </template>
            </el-input>
          </div>
        </div>
      </div>
      <div class="search-footer">
        <FaButton @click="handleSearch">
          <template #icon>
            <FaIcon name="i-heroicons-solid:magnifying-glass" />
          </template>
          搜索
        </FaButton>
        <FaButton class="search-reset-btn" @click="handleReset">
          <template #icon>
            <FaIcon name="i-heroicons-solid:arrow-path" />
          </template>
          重置
        </FaButton>
      </div>
    </FaCard>

    <!-- 操作按钮 -->
    <div class="mb-4 flex items-center justify-between">
      <div class="flex gap-2">
        <FaButton @click="handleCreate">
          <template #icon>
            <FaIcon name="i-iconoir:plus" />
          </template>
          创建分类
        </FaButton>
      </div>
    </div>

    <div class="flex gap-4">
      <!-- 左侧：分类树 -->
      <div class="w-72 flex-shrink-0">
        <FaCard>
          <template #header>
            <span class="font-medium">分类树</span>
          </template>
          <el-tree
            :data="categoryTree"
            :props="{ children: 'children', label: 'name' }"
            node-key="id"
            default-expand-all
          >
            <template #default="{ node, data }">
              <span class="flex items-center">
                <FaIcon v-if="data.icon" :name="data.icon" class="mr-2" />
                <span>{{ node.label }}</span>
                <el-tag v-if="!data.isEnabled" size="small" type="info" class="ml-2">禁用</el-tag>
              </span>
            </template>
          </el-tree>
        </FaCard>
      </div>

      <!-- 右侧：分类列表 -->
      <div class="flex-1">
        <FaCard>
          <el-table
            v-loading="loading"
            :data="tableData"
            row-key="id"
          >
            <el-table-column prop="name" label="分类名称" min-width="200">
              <template #default="{ row }">
                <span>{{ getCategoryLevel(row.level) }}{{ row.name }}</span>
              </template>
            </el-table-column>
            <el-table-column label="图标" width="80">
              <template #default="{ row }">
                <FaIcon v-if="row.icon" :name="row.icon" class="size-5" />
              </template>
            </el-table-column>
            <el-table-column prop="sortOrder" label="排序" width="100" />
            <el-table-column label="状态" width="100" align="center">
              <template #default="{ row }">
                <el-tag :type="row.isEnabled ? 'success' : 'info'" size="small">
                  {{ row.isEnabled ? '启用' : '禁用' }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column label="创建时间" width="180">
              <template #default="{ row }">
                {{ row.createdAt ? dayjs(row.createdAt).format('YYYY-MM-DD HH:mm:ss') : '-' }}
              </template>
            </el-table-column>
            <el-table-column label="操作" width="150" fixed="right">
              <template #default="{ row }">
                <FaButton variant="ghost" size="sm" @click="handleEdit(row)">
                  <template #icon>
                    <FaIcon name="i-iconoir:edit-pencil" />
                  </template>
                  编辑
                </FaButton>
                <FaButton variant="ghost" size="sm" class="text-red-500" @click="handleDelete(row.id)">
                  <template #icon>
                    <FaIcon name="i-iconoir:trash" />
                  </template>
                  删除
                </FaButton>
              </template>
            </el-table-column>
          </el-table>
        </FaCard>
      </div>
    </div>

    <!-- 创建/编辑对话框 -->
    <el-dialog
      v-model="dialogVisible"
      :title="dialogTitle"
      width="500px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="formRef"
        v-loading="dialogLoading"
        :model="formData"
        :rules="rules"
        label-width="100px"
      >
        <el-form-item label="分类名称" prop="name">
          <el-input v-model="formData.name" placeholder="请输入分类名称" />
        </el-form-item>
        <el-form-item label="父分类">
          <el-select
            v-model="formData.parentId"
            placeholder="请选择父分类（不选则为顶级分类）"
            clearable
            class="w-full"
          >
            <el-option
              v-for="item in categoryTree"
              :key="item.id"
              :label="item.name"
              :value="item.id"
            />
            <template v-for="parent in categoryTree" :key="parent.id">
              <el-option
                v-for="child in parent.children"
                :key="child.id"
                :label="`　${child.name}`"
                :value="child.id"
              />
            </template>
          </el-select>
        </el-form-item>
        <el-form-item label="图标">
          <FaIconPicker v-model="formData.icon" />
        </el-form-item>
        <el-form-item label="图片">
          <el-input v-model="formData.image" placeholder="请输入图片URL" />
        </el-form-item>
        <el-form-item label="排序">
          <el-input-number v-model="formData.sortOrder" :min="0" :max="9999" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="dialogLoading" @click="handleSubmit">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>
