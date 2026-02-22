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

// 对话框
const dialogVisible = ref(false)
const dialogTitle = ref('创建分类')
const dialogLoading = ref(false)
const editingCategoryId = ref<string>()
const formData = ref<Api.Category.CreateCategoryRequest>({
  name: '',
  description: '',
  icon: '',
  sort: 0,
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
    description: '',
    icon: '',
    sort: 0,
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
      description: data.description,
      icon: data.icon,
      sort: data.sort,
      parentId: data.parentId,
    }
  }
  finally {
    dialogLoading.value = false
  }
}

// 删除分类
async function handleDelete(id: string) {
  await ElMessageBox.confirm('确定要删除该分类吗？', '提示', {
    type: 'warning',
  })
  await categoryApi.delete(id)
  ElMessage.success('删除成功')
  getCategoryList()
}

// 提交表单
async function handleSubmit() {
  await formRef.value?.validate()
  dialogLoading.value = true
  try {
    if (editingCategoryId.value) {
      await categoryApi.update(editingCategoryId.value, formData.value)
      ElMessage.success('修改成功')
    }
    else {
      await categoryApi.create(formData.value)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    getCategoryList()
  }
  finally {
    dialogLoading.value = false
  }
}

onMounted(() => {
  getCategoryList()
})
</script>

<template>
  <div class="shop-product-category">
    <!-- 搜索表单 -->
    <FaCard class="mb-4">
      <div class="p-4">
        <el-form :model="searchForm" inline>
          <el-form-item label="关键字">
            <el-input v-model="searchForm.keyword" placeholder="分类名称" clearable />
          </el-form-item>
          <el-form-item>
            <FaButton @click="handleSearch">
              <template #icon>
                <FaIcon name="i-mage-icons:search" />
              </template>
              搜索
            </FaButton>
            <FaButton variant="ghost" @click="handleReset">
              重置
            </FaButton>
          </el-form-item>
        </el-form>
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

    <!-- 表格 -->
    <FaCard>
      <el-table
        v-loading="loading"
        :data="tableData"
        row-key="id"
      >
        <el-table-column prop="name" label="分类名称" min-width="150" />
        <el-table-column prop="description" label="描述" min-width="200" show-overflow-tooltip />
        <el-table-column prop="sort" label="排序" width="100" />
        <el-table-column label="创建时间" min-width="180">
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
        <el-form-item label="分类描述">
          <el-input v-model="formData.description" type="textarea" :rows="3" placeholder="请输入分类描述" />
        </el-form-item>
        <el-form-item label="图标">
          <el-input v-model="formData.icon" placeholder="请输入图标类名" />
        </el-form-item>
        <el-form-item label="排序">
          <el-input-number v-model="formData.sort" :min="0" :max="9999" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="dialogLoading" @click="handleSubmit">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>
