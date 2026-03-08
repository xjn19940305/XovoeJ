<script setup lang="ts">
import { ElMessage } from 'element-plus'
import permissionApi from '@/api/modules/permission'
import roleApi from '@/api/modules/role'

defineOptions({
  name: 'SystemRoleAuth',
})

type PermissionTreeNode = Api.Permission.PermissionTree

const roleList = ref<Api.Role.Role[]>([])
const selectedRoleId = ref<string>()
const selectedPermissionCodes = ref<string[]>([])
const permissionTree = ref<PermissionTreeNode[]>([])
const permissionTreeRef = ref()
const loading = ref(false)
const saveLoading = ref(false)
const expandAll = ref(true)
const treeRenderKey = ref(0)
const preserveWildcardPermission = ref(false)

const permissionTypeMap: Record<Api.Permission.PermissionType, { label: string, color: 'primary' | 'success' | 'warning' | 'danger' | 'info' }> = {
  menu: { label: '菜单', color: 'primary' },
  page: { label: '页面', color: 'success' },
  button: { label: '按钮', color: 'warning' },
}

async function getRoleList() {
  const res = await roleApi.getList()
  roleList.value = [...res.data].sort((a, b) => (a.sort || 0) - (b.sort || 0))
}

async function getPermissionTree() {
  const res = await permissionApi.getTree()
  permissionTree.value = res.data.items || []
}

async function syncCheckedKeys(codes: string[]) {
  selectedPermissionCodes.value = [...new Set(codes)]
  await nextTick()
  permissionTreeRef.value?.setCheckedKeys(selectedPermissionCodes.value)
}

async function getRolePermissions() {
  if (!selectedRoleId.value) {
    return
  }

  loading.value = true
  try {
    const res = await roleApi.getPermissions(selectedRoleId.value)
    const codes = res.data || []
    preserveWildcardPermission.value = codes.includes('*')
    const checkedCodes = preserveWildcardPermission.value
      ? getAllPermissionCodes(permissionTree.value)
      : codes
    await syncCheckedKeys(checkedCodes)
  }
  finally {
    loading.value = false
  }
}

async function handleRoleClick(role: Api.Role.Role) {
  selectedRoleId.value = role.id
  await getRolePermissions()
}

function handleCheck(_data: PermissionTreeNode, checked: { checkedKeys: Array<string | number> }) {
  preserveWildcardPermission.value = false
  selectedPermissionCodes.value = checked.checkedKeys.map(key => String(key))
}

async function rerenderTree() {
  treeRenderKey.value += 1
  await syncCheckedKeys(selectedPermissionCodes.value)
}

async function toggleExpand() {
  expandAll.value = !expandAll.value
  await rerenderTree()
}

async function toggleCheckAll() {
  const allCodes = getAllPermissionCodes(permissionTree.value)
  const nextCodes = selectedPermissionCodes.value.length >= allCodes.length ? [] : allCodes
  preserveWildcardPermission.value = false
  await syncCheckedKeys(nextCodes)
}

async function handleSave() {
  if (!selectedRoleId.value) {
    ElMessage.warning('请先选择角色')
    return
  }

  saveLoading.value = true
  try {
    const allCodes = getAllPermissionCodes(permissionTree.value)
    const permissionCodes = preserveWildcardPermission.value && selectedPermissionCodes.value.length === allCodes.length
      ? ['*']
      : selectedPermissionCodes.value

    await roleApi.assignPermissions({
      roleId: selectedRoleId.value,
      permissionCodes,
    })
    ElMessage.success('权限保存成功')
  }
  finally {
    saveLoading.value = false
  }
}

function getAllPermissionCodes(nodes: PermissionTreeNode[]): string[] {
  const codes: string[] = []

  function walk(items: PermissionTreeNode[]) {
    items.forEach((item) => {
      codes.push(item.code)
      if (item.children?.length) {
        walk(item.children)
      }
    })
  }

  walk(nodes)
  return codes
}

function getPermissionTypeConfig(type: string | undefined) {
  return permissionTypeMap[(type as Api.Permission.PermissionType) || 'button'] || permissionTypeMap.button
}

onMounted(async () => {
  loading.value = true
  try {
    await Promise.all([
      getRoleList(),
      getPermissionTree(),
    ])
  }
  finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="system-role-auth h-full flex flex-col">
    <div class="auth-info-card mb-4">
      <div class="auth-info-header">
        <FaIcon name="i-heroicons-solid:information-circle" class="size-5" />
        <span class="auth-info-title">权限说明</span>
      </div>
      <div class="auth-info-body">
        <div class="auth-info-section">
          <span class="auth-info-label">节点类型</span>
          <div class="auth-info-tags">
            <span class="auth-tag auth-tag-menu">
              <FaIcon name="i-heroicons-solid:bars-3" class="size-3.5" />
              菜单
            </span>
            <span class="auth-tag auth-tag-page">
              <FaIcon name="i-heroicons-solid:document-text" class="size-3.5" />
              页面
            </span>
            <span class="auth-tag auth-tag-button">
              <FaIcon name="i-heroicons-solid:cursor-arrow-rays" class="size-3.5" />
              按钮
            </span>
          </div>
        </div>
        <div class="auth-info-section">
          <span class="auth-info-label">使用说明</span>
          <span class="auth-info-hint">先在左侧选择角色，再在右侧勾选需要授权的权限节点。</span>
        </div>
      </div>
    </div>

    <div class="min-h-0 flex flex-1 gap-4">
      <div class="w-64 flex-shrink-0">
        <FaCard class="h-full">
          <template #header>
            <div class="flex items-center justify-between">
              <span class="font-medium">角色列表</span>
              <el-tag size="small">
                {{ roleList.length }}
              </el-tag>
            </div>
          </template>
          <div v-loading="loading" class="role-list">
            <div
              v-for="role in roleList"
              :key="role.id"
              class="role-item"
              :class="{ active: selectedRoleId === role.id }"
              @click="handleRoleClick(role)"
            >
              <div class="role-name">
                {{ role.name }}
              </div>
              <div class="role-description">
                {{ role.description || '暂无描述' }}
              </div>
            </div>
          </div>
        </FaCard>
      </div>

      <div class="min-w-0 flex-1">
        <FaCard class="h-full">
          <template #header>
            <div class="flex items-center justify-between">
              <span class="font-medium">
                {{ selectedRoleId ? '权限树' : '请选择角色' }}
              </span>
              <div v-if="selectedRoleId" class="flex gap-2">
                <FaButton size="sm" variant="outline" @click="toggleExpand">
                  <template #icon>
                    <FaIcon :name="expandAll ? 'i-iconoir:nav-arrow-up' : 'i-iconoir:nav-arrow-down'" />
                  </template>
                  {{ expandAll ? '全部折叠' : '全部展开' }}
                </FaButton>
                <FaButton size="sm" variant="outline" @click="toggleCheckAll">
                  <template #icon>
                    <FaIcon name="i-iconoir:check" />
                  </template>
                  全选/取消
                </FaButton>
                <FaButton size="sm" type="primary" :loading="saveLoading" @click="handleSave">
                  <template #icon>
                    <FaIcon name="i-iconoir:save-floppy-disk" />
                  </template>
                  保存授权
                </FaButton>
              </div>
            </div>
          </template>
          <div v-loading="loading" class="permission-tree-content">
            <el-empty v-if="!selectedRoleId" description="请选择一个角色后再配置权限" />
            <el-tree
              v-else
              :key="treeRenderKey"
              ref="permissionTreeRef"
              :data="permissionTree"
              :props="{ label: 'name', children: 'children' }"
              node-key="code"
              show-checkbox
              :default-expand-all="expandAll"
              @check="handleCheck"
            >
              <template #default="{ node, data }">
                <span class="flex items-center gap-2">
                  <span>{{ node.label }}</span>
                  <el-tag
                    v-if="data.type"
                    size="small"
                    :type="getPermissionTypeConfig(data.type).color"
                  >
                    {{ getPermissionTypeConfig(data.type).label }}
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
.auth-info-card {
  border-radius: 12px;
  overflow: hidden;
  background: hsl(var(--muted) / 0.12);
  border: 1px solid hsl(var(--border));
  box-shadow: 0 1px 3px hsl(var(--foreground) / 0.05);
}

.auth-info-header {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 14px 20px;
  background: linear-gradient(90deg, hsl(var(--primary) / 0.08) 0%, hsl(var(--muted) / 0.12) 100%);
  border-bottom: 1px solid hsl(var(--border));
  color: hsl(var(--primary));
  font-weight: 600;
  font-size: 14px;
}

.auth-info-title {
  font-size: 14px;
}

.auth-info-body {
  padding: 16px 20px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  background: hsl(var(--muted) / 0.12);
}

.auth-info-section {
  display: flex;
  align-items: center;
  gap: 12px;
}

.auth-info-label {
  font-size: 13px;
  font-weight: 500;
  color: hsl(var(--foreground) / 0.7);
  min-width: 80px;
}

.auth-info-tags {
  display: flex;
  gap: 10px;
}

.auth-tag {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  border-radius: 8px;
  font-size: 12px;
  font-weight: 500;
}

.auth-tag-menu {
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
  color: white;
}

.auth-tag-page {
  background: linear-gradient(135deg, #10b981 0%, #059669 100%);
  color: white;
}

.auth-tag-button {
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
  color: white;
}

.auth-info-hint {
  font-size: 13px;
  color: hsl(var(--foreground) / 0.6);
}

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
