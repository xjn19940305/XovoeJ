<script setup lang="ts">
import type { FormInstance, FormRules } from 'element-plus'
import dayjs from 'dayjs'
import { ElMessage, ElMessageBox } from 'element-plus'
import roleApi from '@/api/modules/role'
import userApi from '@/api/modules/user-management'

defineOptions({
  name: 'SystemUser',
})

interface UserFormData {
  userName: string
  password?: string
  email?: string
  phoneNumber?: string
  realName?: string
  nickName?: string
  roleIds: string[]
}

// 搜索表单
const searchForm = ref({
  keyword: '',
  roleName: '',
})

// 表格数据
const tableData = ref<Api.User.User[]>([])
const loading = ref(false)
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(20)

// 选中的行
const selectedRows = ref<Api.User.User[]>([])

// 对话框
const dialogVisible = ref(false)
const dialogTitle = ref('创建用户')
const dialogLoading = ref(false)
const editingUserId = ref<string>()
const formData = ref<UserFormData>({
  userName: '',
  password: '',
  email: '',
  phoneNumber: '',
  realName: '',
  nickName: '',
  roleIds: [],
})

// 角色选项
const roleOptions = ref<Api.Role.Role[]>([])

// 表单引用
const formRef = ref<FormInstance>()

// 表单验证规则
const rules: FormRules = {
  userName: [
    { required: true, message: '请输入用户名', trigger: 'blur' },
    { min: 3, max: 20, message: '用户名长度在 3 到 20 个字符', trigger: 'blur' },
  ],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' },
    { min: 6, max: 20, message: '密码长度在 6 到 20 个字符', trigger: 'blur' },
  ],
}

// 获取用户列表
async function getUserList() {
  loading.value = true
  try {
    const res = await userApi.getList({
      page: currentPage.value,
      pageSize: pageSize.value,
      ...searchForm.value,
    })
    tableData.value = res.data.items
    total.value = res.data.total
  }
  finally {
    loading.value = false
  }
}

// 获取角色列表
async function getRoleList() {
  const res = await roleApi.getList()
  roleOptions.value = res.data
}

// 搜索
function handleSearch() {
  currentPage.value = 1
  getUserList()
}

// 重置搜索
function handleReset() {
  searchForm.value = {
    keyword: '',
    roleName: '',
  }
  currentPage.value = 1
  getUserList()
}

// 打开创建对话框
function handleCreate() {
  dialogTitle.value = '创建用户'
  editingUserId.value = undefined
  formData.value = {
    userName: '',
    password: '123456',
    email: '',
    phoneNumber: '',
    realName: '',
    nickName: '',
    roleIds: [],
  }
  dialogVisible.value = true
}

// 打开编辑对话框
async function handleEdit(row: Api.User.User) {
  dialogTitle.value = '修改用户'
  editingUserId.value = row.id
  dialogLoading.value = true
  dialogVisible.value = true
  try {
    const res = await userApi.getDetail(row.id)
    const data = res.data
    // 用户数据中的 roles 是角色名称数组，需要根据名称找到对应的角色 ID
    const roleIds = data.roles
      ?.map((roleName: string) => {
        const role = roleOptions.value.find(r => r.name === roleName)
        return role?.id
      })
      .filter((id: string | undefined): id is string => !!id) || []

    formData.value = {
      userName: data.userName,
      email: data.email,
      phoneNumber: data.phoneNumber,
      realName: data.realName,
      nickName: data.nickName,
      roleIds,
    }
  }
  finally {
    dialogLoading.value = false
  }
}

// 删除用户
async function handleDelete(id: string) {
  await ElMessageBox.confirm('确定要删除该用户吗？', '提示', {
    type: 'warning',
  })
  await userApi.delete(id)
  ElMessage.success('删除成功')
  getUserList()
}

// 锁定/解锁用户
async function handleToggleLock(row: Api.User.User) {
  const isLocked = !!row.lockoutEnd && new Date(row.lockoutEnd) > new Date()
  try {
    if (isLocked) {
      await userApi.unlock(row.id)
      ElMessage.success('用户已解锁')
    }
    else {
      await userApi.lock(row.id)
      ElMessage.success('用户已锁定')
    }
    getUserList()
  }
  catch {
    // 失败时不做处理
  }
}

// 重置密码
async function handleResetPassword(row: Api.User.User) {
  const { value } = await ElMessageBox.prompt('请输入新密码', '重置密码', {
    inputPattern: /^.{6,20}$/,
    inputErrorMessage: '密码长度在 6 到 20 个字符',
  })
  await userApi.resetPassword(row.id, value)
  ElMessage.success('密码重置成功')
}

// 提交表单
async function handleSubmit() {
  await formRef.value?.validate()
  dialogLoading.value = true
  try {
    if (editingUserId.value) {
      // 编辑模式 - 更新用户基本信息
      await userApi.update(editingUserId.value, {
        email: formData.value.email || undefined,
        phoneNumber: formData.value.phoneNumber || undefined,
        realName: formData.value.realName || undefined,
        nickName: formData.value.nickName || undefined,
      })
      // 更新角色分配 - 将角色ID转换为角色名称
      const roleNames = formData.value.roleIds
        .map((id: string) => roleOptions.value.find(r => r.id === id)?.name)
        .filter((name): name is string => !!name)
      await userApi.assignRoles({
        userId: editingUserId.value,
        roleNames,
      })
      ElMessage.success('修改成功')
    }
    else {
      // 创建模式 - 先创建用户，再分配角色
      const createData = {
        userName: formData.value.userName,
        password: formData.value.password ?? '',
        nickName: formData.value.nickName || undefined,
        realName: formData.value.realName || undefined,
        email: formData.value.email || undefined,
        phoneNumber: formData.value.phoneNumber || undefined,
      }
      const userRes = await userApi.create(createData)

      // 如果有角色，分配角色 - 将角色ID转换为角色名称
      if (formData.value.roleIds.length > 0) {
        const roleNames = formData.value.roleIds
          .map((id: string) => roleOptions.value.find(r => r.id === id)?.name)
          .filter((name): name is string => !!name)
        await userApi.assignRoles({
          userId: userRes.data.id,
          roleNames,
        })
      }
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    getUserList()
  }
  finally {
    dialogLoading.value = false
  }
}

// 分页改变
function handlePageChange(page: number) {
  currentPage.value = page
  getUserList()
}

function handleSizeChange(size: number) {
  pageSize.value = size
  currentPage.value = 1
  getUserList()
}

// 是否锁定
function isLocked(row: Api.User.User) {
  return !!row.lockoutEnd && new Date(row.lockoutEnd) > new Date()
}

onMounted(() => {
  getUserList()
  getRoleList()
})
</script>

<template>
  <div class="system-user">
    <!-- 搜索表单 -->
    <FaCard class="search-card mb-4">
      <div class="search-header">
        <div class="search-title">
          <FaIcon name="i-heroicons-solid:magnifying-glass" class="size-5" />
          <span>筛选搜索</span>
        </div>
      </div>
      <div class="search-body">
        <div class="search-grid">
          <div class="search-field">
            <label class="search-label">关键字</label>
            <el-input
              v-model="searchForm.keyword"
              placeholder="搜索用户名/邮箱/手机号"
              clearable
            >
              <template #prefix>
                <FaIcon name="i-heroicons-solid:magnifying-glass" class="size-4 text-gray-400" />
              </template>
            </el-input>
          </div>
          <div class="search-field">
            <label class="search-label">角色名称</label>
            <el-input
              v-model="searchForm.roleName"
              placeholder="搜索角色名称"
              clearable
            >
              <template #prefix>
                <FaIcon name="i-heroicons-solid:user-group" class="size-4 text-gray-400" />
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
          创建用户
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
        <el-table-column prop="userName" label="用户名" min-width="120" />
        <el-table-column prop="realName" label="真实姓名" min-width="120" />
        <el-table-column prop="nickName" label="昵称" min-width="120" />
        <el-table-column prop="email" label="邮箱" min-width="180" />
        <el-table-column prop="phoneNumber" label="手机号" min-width="130" />
        <el-table-column label="角色" min-width="150">
          <template #default="{ row }">
            <el-tag v-for="roleName in row.roles" :key="roleName" size="small" class="mr-1">
              {{ roleName }}
            </el-tag>
            <span v-if="!row.roles?.length" class="text-gray-400">未分配</span>
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="isLocked(row) ? 'danger' : 'success'">
              {{ isLocked(row) ? '已锁定' : '正常' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="创建时间" min-width="180">
          <template #default="{ row }">
            {{ row.createdAt ? dayjs(row.createdAt).format('YYYY-MM-DD HH:mm:ss') : '-' }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="240" fixed="right">
          <template #default="{ row }">
            <FaButton variant="ghost" size="sm" @click="handleEdit(row)">
              <template #icon>
                <FaIcon name="i-iconoir:edit-pencil" />
              </template>
              编辑
            </FaButton>
            <FaButton variant="ghost" size="sm" @click="handleToggleLock(row)">
              <template #icon>
                <FaIcon :name="isLocked(row) ? 'i-iconoir:lock-open' : 'i-iconoir:lock'" />
              </template>
              {{ isLocked(row) ? '解锁' : '锁定' }}
            </FaButton>
            <FaButton variant="ghost" size="sm" @click="handleResetPassword(row)">
              <template #icon>
                <FaIcon name="i-iconoir:lock" />
              </template>
              重置密码
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

      <!-- 分页 -->
      <div class="mt-4 flex justify-end">
        <el-pagination
          :current-page="currentPage"
          :page-size="pageSize"
          :total="total"
          :page-sizes="[20, 50, 100]"
          layout="total, sizes, prev, pager, next, jumper"
          @current-change="handlePageChange"
          @size-change="handleSizeChange"
        />
      </div>
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
        <el-form-item label="用户名" prop="userName">
          <el-input v-model="formData.userName" placeholder="请输入用户名" :disabled="dialogTitle === '修改用户'" />
        </el-form-item>
        <el-form-item v-if="dialogTitle === '创建用户'" label="密码" prop="password">
          <el-input v-model="formData.password" type="password" placeholder="请输入密码" show-password />
        </el-form-item>
        <el-form-item label="真实姓名" prop="realName">
          <el-input v-model="formData.realName" placeholder="请输入真实姓名" />
        </el-form-item>
        <el-form-item label="昵称" prop="nickName">
          <el-input v-model="formData.nickName" placeholder="请输入昵称" />
        </el-form-item>
        <el-form-item label="邮箱" prop="email">
          <el-input v-model="formData.email" placeholder="请输入邮箱" />
        </el-form-item>
        <el-form-item label="手机号" prop="phoneNumber">
          <el-input v-model="formData.phoneNumber" placeholder="请输入手机号" />
        </el-form-item>
        <el-form-item label="角色" prop="roleIds">
          <el-select
            v-model="formData.roleIds"
            placeholder="请选择角色"
            multiple
            class="w-full"
            value-key="id"
          >
            <el-option
              v-for="item in roleOptions"
              :key="item.id"
              :label="item.name"
              :value="item.id"
            />
          </el-select>
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
