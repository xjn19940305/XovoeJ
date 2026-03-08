<script setup lang="ts">
import type { FormInstance, FormRules } from 'element-plus'
import dayjs from 'dayjs'
import { ElMessage, ElMessageBox } from 'element-plus'
import roleApi from '@/api/modules/role'

defineOptions({
  name: 'SystemRole',
})

// 搜索表单
const searchForm = ref({
  name: '',
})

// 表格数据
const tableData = ref<Api.Role.Role[]>([])
const loading = ref(false)

// 选中的行
const selectedRows = ref<Api.Role.Role[]>([])
const selectedIds = computed(() => selectedRows.value.map(item => item.id))

// 对话框
const dialogVisible = ref(false)
const dialogTitle = ref('创建角色')
const dialogLoading = ref(false)
const formData = ref<Api.Role.CreateRoleRequest | Api.Role.UpdateRoleRequest>({
  name: '',
  description: '',
  sort: 0,
})

// 当前编辑的角色ID
const editingRoleId = ref<string>()

// 表单引用
const formRef = ref<FormInstance>()

// 表单验证规则
const rules: FormRules = {
  name: [
    { required: true, message: '请输入角色名称', trigger: 'blur' },
    { max: 128, message: '角色名称长度不能超过128个字符', trigger: 'blur' },
  ],
  description: [
    { max: 512, message: '角色描述长度不能超过512个字符', trigger: 'blur' },
  ],
}

// 获取角色列表
async function getRoleList() {
  loading.value = true
  try {
    const res = await roleApi.getList(searchForm.value)
    tableData.value = res.data
  }
  finally {
    loading.value = false
  }
}

// 搜索
function handleSearch() {
  getRoleList()
}

// 重置搜索
function handleReset() {
  searchForm.value = {
    name: '',
  }
  getRoleList()
}

// 打开创建对话框
function handleCreate() {
  dialogTitle.value = '创建角色'
  editingRoleId.value = undefined
  formData.value = {
    name: '',
    description: '',
    sort: 0,
  }
  dialogVisible.value = true
}

// 打开编辑对话框
async function handleEdit(row: Api.Role.Role) {
  dialogTitle.value = '修改角色'
  dialogLoading.value = true
  dialogVisible.value = true
  try {
    const res = await roleApi.getDetail(row.id)
    const data = res.data
    editingRoleId.value = data.id
    formData.value = {
      name: data.name,
      description: data.description,
      sort: data.sort,
    }
  }
  finally {
    dialogLoading.value = false
  }
}

// 删除角色
async function handleDelete(id: string) {
  await ElMessageBox.confirm('确定要删除该角色吗？', '提示', {
    type: 'warning',
  })
  await roleApi.delete(id)
  ElMessage.success('删除成功')
  getRoleList()
}

// 批量删除
async function handleBatchDelete() {
  if (selectedIds.value.length === 0) {
    ElMessage.warning('请选择要删除的角色')
    return
  }
  await ElMessageBox.confirm(`确定要删除选中的 ${selectedIds.value.length} 个角色吗？`, '提示', {
    type: 'warning',
  })
  // 批量删除需要后端支持，暂时循环调用单个删除
  for (const id of selectedIds.value) {
    await roleApi.delete(id)
  }
  ElMessage.success('删除成功')
  getRoleList()
}

// 提交表单
async function handleSubmit() {
  await formRef.value?.validate()
  dialogLoading.value = true
  try {
    if (editingRoleId.value) {
      await roleApi.update(editingRoleId.value, formData.value)
      ElMessage.success('修改成功')
    }
    else {
      await roleApi.create(formData.value)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    getRoleList()
  }
  finally {
    dialogLoading.value = false
  }
}

onMounted(() => {
  getRoleList()
})
</script>

<template>
  <div class="system-role">
    <!-- 搜索表单 -->
    <FaCard class="search-card mb-4">
      <div class="search-header">
        <div class="search-title">
          <FaIcon name="i-heroicons-solid:magnifying-glass" class="size-5" />
          <span>角色筛选</span>
        </div>
      </div>
      <div class="search-body">
        <div class="search-grid">
          <div class="search-field">
            <label class="search-label">角色名称</label>
            <el-input
              v-model="searchForm.name"
              placeholder="搜索角色名称"
              clearable
            >
              <template #prefix>
                <FaIcon name="i-heroicons-solid:user-circle" class="size-4 text-gray-400" />
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
          创建角色
        </FaButton>
        <FaButton variant="destructive" :disabled="selectedIds.length === 0" @click="handleBatchDelete">
          <template #icon>
            <FaIcon name="i-iconoir:trash" />
          </template>
          批量删除
        </FaButton>
      </div>
    </div>

    <!-- 表格 -->
    <FaCard>
      <el-table
        v-loading="loading"
        :data="tableData"
        @selection-change="(rows) => selectedRows = rows"
      >
        <el-table-column type="selection" width="55" />
        <el-table-column prop="id" label="ID" width="250" />
        <el-table-column prop="name" label="角色名称" min-width="150" />
        <el-table-column prop="description" label="描述" min-width="200" show-overflow-tooltip />
        <el-table-column prop="sort" label="排序" width="100" />
        <el-table-column label="创建时间" min-width="180">
          <template #default="{ row }">
            {{ dayjs(row.createdAt).format('YYYY-MM-DD HH:mm:ss') }}
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
      width="600px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="formRef"
        v-loading="dialogLoading"
        :model="formData"
        :rules="rules"
        label-width="100px"
      >
        <el-form-item label="角色名称" prop="name">
          <el-input v-model="formData.name" placeholder="请输入角色名称" />
        </el-form-item>
        <el-form-item label="角色描述">
          <el-input v-model="formData.description" type="textarea" :rows="3" placeholder="请输入角色描述" />
        </el-form-item>
        <el-form-item label="排序">
          <el-input-number v-model="formData.sort" :min="0" :max="9999" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">
          取消
        </el-button>
        <el-button type="primary" :loading="dialogLoading" @click="handleSubmit">
          确定
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>
