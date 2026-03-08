<script setup lang="ts">
import type { TagProps } from 'element-plus'
import permissionApi from '@/api/modules/permission'
import AdminMetricCard from '@/components/admin/AdminMetricCard.vue'

defineOptions({
  name: 'SystemPermission',
})

type PermissionNode = Api.Permission.Permission
type PermissionTreeNode = Api.Permission.PermissionTree

const searchForm = reactive<Api.Permission.PermissionListParams>({
  page: 1,
  pageSize: 20,
  name: '',
  code: '',
  type: undefined,
  status: undefined,
})

const loading = ref(false)
const treeLoading = ref(false)
const tableData = ref<PermissionNode[]>([])
const permissionTree = ref<PermissionTreeNode[]>([])
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(20)
const viewMode = ref<'list' | 'tree'>('list')
const treeKeyword = ref('')
const treeRef = ref()

const typeOptions: Array<{ label: string, value: Api.Permission.PermissionType }> = [
  { label: '菜单', value: 'menu' },
  { label: '页面', value: 'page' },
  { label: '按钮', value: 'button' },
]

const statusOptions = [
  { label: '启用', value: 1 },
  { label: '停用', value: 0 },
]

const summaryCards = computed(() => {
  const counters = {
    total: 0,
    menu: 0,
    page: 0,
    button: 0,
  }

  function walk(nodes: PermissionTreeNode[]) {
    nodes.forEach((node) => {
      counters.total += 1
      counters[node.type] += 1
      if (node.children?.length) {
        walk(node.children)
      }
    })
  }

  walk(permissionTree.value)

  return [
    {
      title: '权限总数',
      value: counters.total,
      description: '包括菜单、页面和按钮在内的全部权限节点，用于观察目录规模。',
      icon: 'i-heroicons-solid:shield-check',
      tone: 'blue' as const,
    },
    {
      title: '菜单节点',
      value: counters.menu,
      description: '反映后台菜单编排与模块入口的配置体量。',
      icon: 'i-heroicons-solid:squares-2x2',
      tone: 'sky' as const,
    },
    {
      title: '页面节点',
      value: counters.page,
      description: '用于观察页面级权限拆分是否清晰、是否过度膨胀。',
      icon: 'i-heroicons-solid:document-duplicate',
      tone: 'emerald' as const,
    },
    {
      title: '按钮节点',
      value: counters.button,
      description: '直接体现动作级权限颗粒度，便于排查授权细分情况。',
      icon: 'i-heroicons-solid:cursor-arrow-rays',
      tone: 'amber' as const,
    },
  ]
})

function getTypeMeta(type: Api.Permission.PermissionType): { label: string, tag: TagProps['type'] } {
  if (type === 'menu') {
    return { label: '菜单', tag: 'primary' }
  }
  if (type === 'page') {
    return { label: '页面', tag: 'success' }
  }
  return { label: '按钮', tag: 'warning' }
}

function getStatusMeta(status: 0 | 1): { label: string, tag: TagProps['type'] } {
  return status === 1
    ? { label: '启用', tag: 'success' }
    : { label: '停用', tag: 'info' }
}

async function getPermissionList() {
  loading.value = true
  try {
    const res = await permissionApi.getList({
      ...searchForm,
      page: currentPage.value,
      pageSize: pageSize.value,
      name: searchForm.name || undefined,
      code: searchForm.code || undefined,
    })
    tableData.value = res.data.items
    total.value = res.data.total
  }
  finally {
    loading.value = false
  }
}

async function getPermissionTree() {
  treeLoading.value = true
  try {
    const res = await permissionApi.getTree()
    permissionTree.value = res.data.items || []
  }
  finally {
    treeLoading.value = false
  }
}

function handleSearch() {
  currentPage.value = 1
  getPermissionList()
}

function handleReset() {
  searchForm.name = ''
  searchForm.code = ''
  searchForm.type = undefined
  searchForm.status = undefined
  currentPage.value = 1
  getPermissionList()
}

function handleCurrentChange(page: number) {
  currentPage.value = page
  getPermissionList()
}

function handleSizeChange(size: number) {
  pageSize.value = size
  currentPage.value = 1
  getPermissionList()
}

function filterTreeNode(value: string, data: PermissionTreeNode) {
  if (!value) {
    return true
  }

  return data.name.includes(value) || data.code.includes(value)
}

watch(treeKeyword, (value) => {
  treeRef.value?.filter(value)
})

onMounted(async () => {
  await Promise.all([
    getPermissionList(),
    getPermissionTree(),
  ])
})
</script>

<template>
  <div class="system-permission h-full flex flex-col gap-4">
    <div class="summary-grid">
      <AdminMetricCard
        v-for="card in summaryCards"
        :key="card.title"
        :title="card.title"
        :value="card.value"
        :description="card.description"
        :icon="card.icon"
        :tone="card.tone"
        variant="board"
      />
    </div>

    <FaCard class="search-card">
      <div class="search-header">
        <div class="search-title">
          <FaIcon name="i-heroicons-solid:magnifying-glass" class="size-5" />
          <span>权限目录筛选</span>
        </div>
      </div>
      <div class="search-body">
        <div class="search-grid">
          <div class="search-field">
            <label class="search-label">权限名称</label>
            <el-input v-model="searchForm.name" placeholder="请输入权限名称" clearable />
          </div>
          <div class="search-field">
            <label class="search-label">权限代码</label>
            <el-input v-model="searchForm.code" placeholder="请输入权限代码" clearable />
          </div>
          <div class="search-field">
            <label class="search-label">节点类型</label>
            <el-select v-model="searchForm.type" placeholder="全部类型" clearable class="w-full">
              <el-option
                v-for="item in typeOptions"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </el-select>
          </div>
          <div class="search-field">
            <label class="search-label">状态</label>
            <el-select v-model="searchForm.status" placeholder="全部状态" clearable class="w-full">
              <el-option
                v-for="item in statusOptions"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </el-select>
          </div>
        </div>
      </div>
      <div class="search-footer">
        <FaButton type="primary" @click="handleSearch">
          <template #icon>
            <FaIcon name="i-heroicons-solid:magnifying-glass" />
          </template>
          查询
        </FaButton>
        <FaButton class="search-reset-btn" @click="handleReset">
          <template #icon>
            <FaIcon name="i-heroicons-solid:arrow-path" />
          </template>
          重置
        </FaButton>
      </div>
    </FaCard>

    <FaCard class="min-h-0 flex-1">
      <template #header>
        <div class="flex flex-wrap items-center justify-between gap-3">
          <div>
            <div class="text-base font-medium">权限目录</div>
            <div class="text-sm text-gray-500">
              展示中文名称，保留 `code` 作为接口和权限判定依据。
            </div>
          </div>
          <div class="flex items-center gap-3">
            <el-radio-group v-model="viewMode" size="default">
              <el-radio-button value="list">列表视图</el-radio-button>
              <el-radio-button value="tree">树形视图</el-radio-button>
            </el-radio-group>
            <el-input
              v-if="viewMode === 'tree'"
              v-model="treeKeyword"
              placeholder="筛选权限树"
              clearable
              class="w-64"
            />
          </div>
        </div>
      </template>

      <div v-if="viewMode === 'list'" class="min-h-0 flex flex-1 flex-col">
        <el-table v-loading="loading" :data="tableData" stripe class="flex-1">
          <el-table-column prop="name" label="权限名称" min-width="160" />
          <el-table-column prop="code" label="权限代码" min-width="220" show-overflow-tooltip />
          <el-table-column prop="type" label="节点类型" width="110" align="center">
            <template #default="{ row }">
              <el-tag :type="getTypeMeta(row.type).tag">
                {{ getTypeMeta(row.type).label }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="path" label="路径" min-width="150" show-overflow-tooltip>
            <template #default="{ row }">{{ row.path || '-' }}</template>
          </el-table-column>
          <el-table-column prop="icon" label="图标" min-width="140" show-overflow-tooltip>
            <template #default="{ row }">{{ row.icon || '-' }}</template>
          </el-table-column>
          <el-table-column prop="sort" label="排序" width="90" align="center" />
          <el-table-column prop="status" label="状态" width="100" align="center">
            <template #default="{ row }">
              <el-tag :type="getStatusMeta(row.status).tag">
                {{ getStatusMeta(row.status).label }}
              </el-tag>
            </template>
          </el-table-column>
        </el-table>

        <div class="mt-4 flex justify-end">
          <el-pagination
            background
            layout="total, sizes, prev, pager, next, jumper"
            :current-page="currentPage"
            :page-size="pageSize"
            :page-sizes="[10, 20, 50, 100]"
            :total="total"
            @current-change="handleCurrentChange"
            @size-change="handleSizeChange"
          />
        </div>
      </div>

      <div v-else v-loading="treeLoading" class="permission-tree-panel">
        <el-tree
          ref="treeRef"
          :data="permissionTree"
          node-key="code"
          :props="{ label: 'name', children: 'children' }"
          default-expand-all
          :filter-node-method="filterTreeNode"
        >
          <template #default="{ node, data }">
            <div class="tree-node">
              <div class="tree-node-main">
                <span class="tree-node-name">{{ node.label }}</span>
                <el-tag size="small" :type="getTypeMeta(data.type).tag">
                  {{ getTypeMeta(data.type).label }}
                </el-tag>
                <el-tag size="small" effect="plain" :type="getStatusMeta(data.status).tag">
                  {{ getStatusMeta(data.status).label }}
                </el-tag>
              </div>
              <div class="tree-node-code">
                {{ data.code }}
              </div>
            </div>
          </template>
        </el-tree>
      </div>
    </FaCard>
  </div>
</template>

<style scoped>
.summary-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 16px;
}

.permission-tree-panel {
  min-height: 480px;
  max-height: calc(100vh - 330px);
  overflow: auto;
}

.tree-node {
  display: flex;
  width: 100%;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 6px 0;
}

.tree-node-main {
  display: flex;
  min-width: 0;
  align-items: center;
  gap: 8px;
}

.tree-node-name {
  font-weight: 500;
}

.tree-node-code {
  color: hsl(var(--foreground) / 0.5);
  font-size: 12px;
  font-family: 'JetBrains Mono', monospace;
}

@media (max-width: 1200px) {
  .summary-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 768px) {
  .summary-grid {
    grid-template-columns: 1fr;
  }

  .tree-node {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>
