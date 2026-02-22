<script setup lang="ts">
import { ElMessage } from 'element-plus'
import roleApi from '@/api/modules/role'

defineOptions({
  name: 'SystemRoleAuth',
})

// 角色列表
const roleList = ref<Api.Role.Role[]>([])
const selectedRoleId = ref<string>()

// 选中的权限（权限代码列表）
const selectedPermissionCodes = ref<string[]>([])

// 权限树数据
const permissionTree = ref<PermissionTreeNode[]>([])

// 权限树组件引用
const permissionTreeRef = ref()

// 保存加载状态
const saveLoading = ref(false)
const loading = ref(false)

// 权限树节点接口
interface PermissionTreeNode {
  id: string
  code: string
  label: string
  type: 'menu' | 'page' | 'button'
  children?: PermissionTreeNode[]
}

// 权限类型映射
const permissionTypeMap: Record<string, { label: string; color: any }> = {
  menu: { label: '菜单', color: 'primary' },
  page: { label: '页面', color: 'success' },
  button: { label: '按钮', color: 'warning' },
}

// 模拟权限树数据（实际应该从后端获取）
const mockPermissionTree: PermissionTreeNode[] = [
  {
    id: '1',
    code: 'system',
    label: '系统管理',
    type: 'menu',
    children: [
      {
        id: '1-1',
        code: 'system:user',
        label: '用户管理',
        type: 'page',
        children: [
          { id: '1-1-1', code: 'system:user:view', label: '查看用户', type: 'button' },
          { id: '1-1-2', code: 'system:user:create', label: '创建用户', type: 'button' },
          { id: '1-1-3', code: 'system:user:edit', label: '编辑用户', type: 'button' },
          { id: '1-1-4', code: 'system:user:delete', label: '删除用户', type: 'button' },
        ],
      },
      {
        id: '1-2',
        code: 'system:role',
        label: '角色管理',
        type: 'page',
        children: [
          { id: '1-2-1', code: 'system:role:view', label: '查看角色', type: 'button' },
          { id: '1-2-2', code: 'system:role:create', label: '创建角色', type: 'button' },
          { id: '1-2-3', code: 'system:role:edit', label: '编辑角色', type: 'button' },
          { id: '1-2-4', code: 'system:role:delete', label: '删除角色', type: 'button' },
          { id: '1-2-5', code: 'system:role:assign-permissions', label: '角色授权', type: 'button' },
        ],
      },
    ],
  },
  {
    id: '2',
    code: 'shop',
    label: '商城管理',
    type: 'menu',
    children: [
      {
        id: '2-1',
        code: 'shop:product',
        label: '商品管理',
        type: 'page',
        children: [
          { id: '2-1-1', code: 'shop:product:view', label: '查看商品', type: 'button' },
          { id: '2-1-2', code: 'shop:product:create', label: '创建商品', type: 'button' },
          { id: '2-1-3', code: 'shop:product:edit', label: '编辑商品', type: 'button' },
          { id: '2-1-4', code: 'shop:product:delete', label: '删除商品', type: 'button' },
        ],
      },
      {
        id: '2-2',
        code: 'shop:category',
        label: '商品分类',
        type: 'page',
        children: [
          { id: '2-2-1', code: 'shop:category:view', label: '查看分类', type: 'button' },
          { id: '2-2-2', code: 'shop:category:create', label: '创建分类', type: 'button' },
          { id: '2-2-3', code: 'shop:category:edit', label: '编辑分类', type: 'button' },
          { id: '2-2-4', code: 'shop:category:delete', label: '删除分类', type: 'button' },
        ],
      },
      {
        id: '2-3',
        code: 'shop:order',
        label: '订单管理',
        type: 'page',
        children: [
          { id: '2-3-1', code: 'shop:order:view', label: '查看订单', type: 'button' },
          { id: '2-3-2', code: 'shop:order:create', label: '创建订单', type: 'button' },
          { id: '2-3-3', code: 'shop:order:edit', label: '编辑订单', type: 'button' },
          { id: '2-3-4', code: 'shop:order:delete', label: '删除订单', type: 'button' },
        ],
      },
    ],
  },
  {
    id: '3',
    code: 'content',
    label: '内容管理',
    type: 'menu',
    children: [
      {
        id: '3-1',
        code: 'content:banner',
        label: '轮播图管理',
        type: 'page',
        children: [
          { id: '3-1-1', code: 'content:banner:view', label: '查看轮播图', type: 'button' },
          { id: '3-1-2', code: 'content:banner:create', label: '创建轮播图', type: 'button' },
          { id: '3-1-3', code: 'content:banner:edit', label: '编辑轮播图', type: 'button' },
          { id: '3-1-4', code: 'content:banner:delete', label: '删除轮播图', type: 'button' },
        ],
      },
    ],
  },
]

// 获取角色列表
async function getRoleList() {
  loading.value = true
  try {
    const res = await roleApi.getList()
    roleList.value = res.data
  }
  finally {
    loading.value = false
  }
}

// 获取角色权限
async function getRolePermissions() {
  if (!selectedRoleId.value)
    return
  loading.value = true
  try {
    const res = await roleApi.getPermissions(selectedRoleId.value)
    selectedPermissionCodes.value = res.data
    // 手动设置树的选中状态
    nextTick(() => {
      permissionTreeRef.value?.setCheckedKeys(res.data)
    })
  }
  finally {
    loading.value = false
  }
}

// 角色切换
async function handleRoleClick(role: Api.Role.Role) {
  selectedRoleId.value = role.id
  selectedPermissionCodes.value = []
  await getRolePermissions()
}

// 树节点选中变化
function handleCheckChange(_data: any, checked: any) {
  selectedPermissionCodes.value = checked.checkedKeys
}

// 保存权限
async function handleSave() {
  if (!selectedRoleId.value) {
    ElMessage.warning('请先选择角色')
    return
  }

  saveLoading.value = true
  try {
    await roleApi.assignPermissions({
      roleId: selectedRoleId.value,
      permissionCodes: selectedPermissionCodes.value,
    })
    ElMessage.success('保存成功')
  }
  finally {
    saveLoading.value = false
  }
}

// 全部展开/折叠
const expandAll = ref(false)
function toggleExpand() {
  expandAll.value = !expandAll.value
}

// 全选/取消全选
function toggleCheckAll() {
  const allCodes = getAllPermissionCodes(permissionTree.value)
  if (selectedPermissionCodes.value.length >= allCodes.length) {
    selectedPermissionCodes.value = []
  }
  else {
    selectedPermissionCodes.value = allCodes
  }
}

// 获取所有权限代码
function getAllPermissionCodes(nodes: PermissionTreeNode[]): string[] {
  const codes: string[] = []
  function traverse(list: PermissionTreeNode[]) {
    list.forEach((node) => {
      codes.push(node.code)
      if (node.children?.length) {
        traverse(node.children)
      }
    })
  }
  traverse(nodes)
  return codes
}

onMounted(() => {
  getRoleList()
  // 使用模拟权限树数据
  permissionTree.value = mockPermissionTree
})
</script>

<template>
  <div class="system-role-auth h-full flex flex-col">
    <!-- 权限说明 -->
    <FaCard class="mb-4">
      <div class="text-sm text-gray-600">
        <div class="flex items-center gap-4">
          <div class="flex items-center gap-2 font-medium">
            <FaIcon name="i-iconoir:info" class="mr-1" />
            权限说明
          </div>
          <span class="text-gray-400">|</span>
          <span>权限类型：</span>
          <el-tag size="small" type="primary">菜单</el-tag>
          <el-tag size="small" type="success">页面</el-tag>
          <el-tag size="small" type="warning">按钮</el-tag>
        </div>
      </div>
    </FaCard>

    <!-- 主体内容区 -->
    <div class="flex flex-1 gap-4 min-h-0">
      <!-- 左侧：角色列表 -->
      <div class="w-64 flex-shrink-0">
        <FaCard class="h-full">
          <template #header>
            <div class="flex items-center justify-between">
              <span class="font-medium">角色列表</span>
              <el-tag size="small">{{ roleList.length }}</el-tag>
            </div>
          </template>
          <div class="role-list">
            <div
              v-for="role in roleList"
              :key="role.id"
              class="role-item"
              :class="{ active: selectedRoleId === role.id }"
              @click="handleRoleClick(role)"
            >
              <div class="role-name">{{ role.name }}</div>
              <div class="role-description">{{ role.description || '暂无描述' }}</div>
            </div>
          </div>
        </FaCard>
      </div>

      <!-- 右侧：权限树 -->
      <div class="flex-1 min-w-0">
        <FaCard class="h-full">
          <template #header>
            <div class="flex items-center justify-between">
              <span class="font-medium">
                {{ selectedRoleId ? '权限配置' : '请选择角色' }}
              </span>
              <div class="flex gap-2" v-if="selectedRoleId">
                <FaButton size="sm" variant="outline" @click="toggleExpand">
                  <template #icon>
                    <FaIcon :name="expandAll ? 'i-iconoir:nav-arrow-up' : 'i-iconoir:nav-arrow-down'" />
                  </template>
                  {{ expandAll ? '收起' : '展开' }}
                </FaButton>
                <FaButton size="sm" variant="outline" @click="toggleCheckAll">
                  <template #icon>
                    <FaIcon name="i-iconoir:check" />
                  </template>
                  全选
                </FaButton>
                <FaButton size="sm" type="primary" :loading="saveLoading" @click="handleSave">
                  <template #icon>
                    <FaIcon name="i-iconoir:save-floppy-disk" />
                  </template>
                  保存
                </FaButton>
              </div>
            </div>
          </template>
          <div class="permission-tree-content" v-loading="loading">
            <el-empty v-if="!selectedRoleId" description="请先在左侧选择角色" />
            <el-tree
              v-else
              ref="permissionTreeRef"
              :data="permissionTree"
              :props="{
                label: 'label',
                children: 'children',
              }"
              :default-checked-keys="selectedPermissionCodes"
              node-key="code"
              show-checkbox
              :default-expand-all="!expandAll"
              @check="handleCheckChange"
            >
              <template #default="{ node, data }">
                <span class="flex items-center gap-2">
                  <span>{{ node.label }}</span>
                  <el-tag
                    v-if="data.type"
                    size="small"
                    :type="permissionTypeMap[data.type]?.color"
                  >
                    {{ permissionTypeMap[data.type]?.label }}
                  </el-tag>
                  <span class="text-xs text-gray-400">({{ data.code }})</span>
                </span>
              </template>
            </el-tree>
          </div>
        </FaCard>
      </div>
    </div>
  </div>
</template>

<style scoped>
.role-list {
  max-height: calc(100vh - 300px);
  overflow-y: auto;
}

.role-item {
  padding: 12px;
  margin-bottom: 8px;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
  border: 1px solid transparent;
}

.role-item:hover {
  background-color: var(--el-bg-color-page);
  border-color: var(--el-border-color);
}

.role-item.active {
  background-color: var(--el-color-primary-light-9);
  border-color: var(--el-color-primary);
  color: var(--el-color-primary);
}

.role-name {
  font-weight: 500;
  margin-bottom: 4px;
}

.role-description {
  font-size: 12px;
  color: var(--el-text-color-secondary);
}

.permission-tree-content {
  max-height: calc(100vh - 300px);
  overflow-y: auto;
}
</style>
