<script setup lang="ts">
import dayjs from 'dayjs'
import { ElMessage, ElMessageBox } from 'element-plus'
import growthApi from '@/api/modules/growth'
import AdminMetricCard from '@/components/admin/AdminMetricCard.vue'

defineOptions({
  name: 'GrowthCommissionPage',
})

const searchForm = ref({
  keyword: '',
  status: undefined as number | undefined,
})

const tableData = ref<Api.Growth.CommissionRecord[]>([])
const loading = ref(false)
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(20)

const detailDialogVisible = ref(false)
const detailLoading = ref(false)
const currentCommission = ref<Api.Growth.CommissionRecord | null>(null)

const statusOptions = [
  { label: '待结算', value: 0 },
  { label: '可结算', value: 1 },
  { label: '已结算', value: 2 },
  { label: '已回退', value: 3 },
]

const statusMap: Record<number, { label: string, type: 'danger' | 'info' | 'success' | 'warning' }> = {
  0: { label: '待结算', type: 'warning' },
  1: { label: '可结算', type: 'info' },
  2: { label: '已结算', type: 'success' },
  3: { label: '已回退', type: 'danger' },
}

const summaryCards = computed(() => [
  {
    title: '佣金记录',
    value: total.value,
    description: '当前查询范围内的佣金记录总量，用于观察分销结算压力。',
    icon: 'i-heroicons-solid:banknotes',
    tone: 'blue' as const,
  },
  {
    title: '当前页预估佣金',
    value: formatAmount(tableData.value.reduce((sum, item) => sum + (item.estimatedAmount || 0), 0)),
    description: '代表本页数据的潜在待结算规模，便于预估资金占用。',
    icon: 'i-heroicons-solid:scale',
    tone: 'amber' as const,
  },
  {
    title: '当前页已结算',
    value: formatAmount(tableData.value.reduce((sum, item) => sum + (item.settledAmount || 0), 0)),
    description: '用于查看结算执行结果，判断结算进度是否顺畅。',
    icon: 'i-heroicons-solid:check-badge',
    tone: 'emerald' as const,
  },
  {
    title: '已结算笔数',
    value: tableData.value.filter(item => item.status === 2).length,
    description: '与预估佣金结合观察结算效率和积压情况。',
    icon: 'i-heroicons-solid:receipt-percent',
    tone: 'sky' as const,
  },
])

async function getCommissionList() {
  loading.value = true
  try {
    const res = await growthApi.getCommissionList({
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

function handleSearch() {
  currentPage.value = 1
  getCommissionList()
}

function handleReset() {
  searchForm.value = {
    keyword: '',
    status: undefined,
  }
  currentPage.value = 1
  getCommissionList()
}

async function handleViewDetail(row: Api.Growth.CommissionRecord) {
  detailDialogVisible.value = true
  detailLoading.value = true
  currentCommission.value = row
  try {
    const res = await growthApi.getCommissionDetail(row.id)
    currentCommission.value = res.data
  }
  finally {
    detailLoading.value = false
  }
}

async function handleSettle(row: Api.Growth.CommissionRecord) {
  await ElMessageBox.confirm(
    `确认将佣金记录 ${row.orderNo || row.id} 结算为 ${formatAmount(row.estimatedAmount || 0)} 吗？`,
    '佣金结算',
    { type: 'warning' },
  )

  await growthApi.settleCommission(row.id)
  ElMessage.success('佣金结算成功')
  await getCommissionList()
}

function handlePageChange(page: number) {
  currentPage.value = page
  getCommissionList()
}

function handleSizeChange(size: number) {
  pageSize.value = size
  currentPage.value = 1
  getCommissionList()
}

function formatAmount(value: number) {
  return `¥ ${value.toFixed(2)}`
}

function formatRate(value: number) {
  return `${value.toFixed(2)}%`
}

onMounted(() => {
  getCommissionList()
})
</script>

<template>
  <div class="growth-commission-page">
    <div class="grid mb-4 gap-4 md:grid-cols-2 xl:grid-cols-4">
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

    <FaCard class="search-card mb-4">
      <div class="search-header">
        <div class="search-title">
          <FaIcon name="i-heroicons-solid:magnifying-glass" class="size-5" />
          <span>佣金筛选</span>
        </div>
      </div>
      <div class="search-body">
        <div class="search-grid">
          <div class="search-field">
            <label class="search-label">关键词</label>
            <el-input
              v-model="searchForm.keyword"
              placeholder="搜索推广人、订单号或规则名称"
              clearable
            >
              <template #prefix>
                <FaIcon name="i-heroicons-solid:banknotes" class="size-4 text-stone-400" />
              </template>
            </el-input>
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
        <FaButton @click="handleSearch">
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

    <FaCard>
      <template #header>
        <div class="flex items-center justify-between">
          <div>
            <span class="font-medium">佣金结算</span>
            <span class="ml-2 text-sm text-stone-500">增长中心 / 佣金管理</span>
          </div>
          <FaButton variant="ghost" @click="getCommissionList">
            <template #icon>
              <FaIcon name="i-heroicons-solid:arrow-path" />
            </template>
            刷新
          </FaButton>
        </div>
      </template>

      <el-table v-loading="loading" :data="tableData">
        <el-table-column label="推广人" min-width="160" show-overflow-tooltip>
          <template #default="{ row }">
            {{ row.promoterName || '-' }}
          </template>
        </el-table-column>
        <el-table-column prop="orderNo" label="订单号" min-width="170" show-overflow-tooltip>
          <template #default="{ row }">
            {{ row.orderNo || '-' }}
          </template>
        </el-table-column>
        <el-table-column prop="ruleName" label="规则名称" min-width="160" show-overflow-tooltip>
          <template #default="{ row }">
            {{ row.ruleName || '-' }}
          </template>
        </el-table-column>
        <el-table-column prop="sourceType" label="来源类型" width="120" align="center">
          <template #default="{ row }">
            {{ row.sourceType || '-' }}
          </template>
        </el-table-column>
        <el-table-column label="佣金比例" width="100" align="right">
          <template #default="{ row }">
            {{ formatRate(row.commissionRate || 0) }}
          </template>
        </el-table-column>
        <el-table-column label="预估佣金" width="120" align="right">
          <template #default="{ row }">
            {{ formatAmount(row.estimatedAmount || 0) }}
          </template>
        </el-table-column>
        <el-table-column label="已结算佣金" width="120" align="right">
          <template #default="{ row }">
            {{ formatAmount(row.settledAmount || 0) }}
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="statusMap[row.status]?.type || 'info'" size="small">
              {{ statusMap[row.status]?.label || '未知状态' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="创建时间" width="180">
          <template #default="{ row }">
            {{ row.createdAt ? dayjs(row.createdAt).format('YYYY-MM-DD HH:mm:ss') : '-' }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120" fixed="right">
          <template #default="{ row }">
            <div class="flex flex-wrap justify-end gap-2">
              <FaButton v-if="row.status === 1" size="sm" @click="handleSettle(row)">
                结算
              </FaButton>
              <FaButton variant="ghost" size="sm" @click="handleViewDetail(row)">
                详情
              </FaButton>
            </div>
          </template>
        </el-table-column>
      </el-table>

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

    <el-dialog v-model="detailDialogVisible" title="佣金详情" width="720px">
      <div v-loading="detailLoading">
        <el-empty v-if="!currentCommission" description="暂无佣金数据" />
        <div v-else class="space-y-4">
          <el-descriptions :column="2" border>
            <el-descriptions-item label="推广人">
              {{ currentCommission.promoterName || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="订单号">
              {{ currentCommission.orderNo || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="规则名称">
              {{ currentCommission.ruleName || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="来源类型">
              {{ currentCommission.sourceType || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="佣金比例">
              {{ formatRate(currentCommission.commissionRate || 0) }}
            </el-descriptions-item>
            <el-descriptions-item label="状态">
              {{ statusMap[currentCommission.status]?.label || '未知状态' }}
            </el-descriptions-item>
            <el-descriptions-item label="预估佣金">
              {{ formatAmount(currentCommission.estimatedAmount || 0) }}
            </el-descriptions-item>
            <el-descriptions-item label="已结算佣金">
              {{ formatAmount(currentCommission.settledAmount || 0) }}
            </el-descriptions-item>
            <el-descriptions-item label="创建时间">
              {{ currentCommission.createdAt ? dayjs(currentCommission.createdAt).format('YYYY-MM-DD HH:mm:ss') : '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="结算时间">
              {{ currentCommission.settledAt ? dayjs(currentCommission.settledAt).format('YYYY-MM-DD HH:mm:ss') : '-' }}
            </el-descriptions-item>
          </el-descriptions>
        </div>
      </div>
    </el-dialog>
  </div>
</template>
