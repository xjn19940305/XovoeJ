<script setup lang="ts">
import dayjs from 'dayjs'
import assetApi from '@/api/modules/asset'
import AdminMetricCard from '@/components/admin/AdminMetricCard.vue'

defineOptions({
  name: 'ShopAssetCenter',
})

type TabKey = 'wallets' | 'walletLogs' | 'pointsAccounts' | 'pointsLogs'

const activeTab = ref<TabKey>('wallets')
const keyword = ref('')
const loading = ref(false)

const wallets = reactive({ items: [] as Api.Asset.WalletAccount[], total: 0, page: 1, pageSize: 20 })
const walletLogs = reactive({ items: [] as Api.Asset.WalletTransaction[], total: 0, page: 1, pageSize: 20 })
const pointsAccounts = reactive({ items: [] as Api.Asset.PointsAccount[], total: 0, page: 1, pageSize: 20 })
const pointsLogs = reactive({ items: [] as Api.Asset.PointsLog[], total: 0, page: 1, pageSize: 20 })

const summaryCards = computed(() => [
  {
    title: '钱包账户',
    value: wallets.total,
    description: '用于观察当前系统已初始化的钱包账户覆盖规模。',
    icon: 'i-heroicons-solid:wallet',
    tone: 'blue' as const,
  },
  {
    title: '当前页钱包余额',
    value: formatAmount(wallets.items.reduce((sum, item) => sum + item.availableBalance, 0)),
    description: '按当前页钱包账户汇总，便于快速观察余额分布。',
    icon: 'i-heroicons-solid:banknotes',
    tone: 'emerald' as const,
  },
  {
    title: '积分账户',
    value: pointsAccounts.total,
    description: '反映积分账户初始化和会员资产覆盖情况。',
    icon: 'i-heroicons-solid:star',
    tone: 'amber' as const,
  },
  {
    title: '当前页可用积分',
    value: pointsAccounts.items.reduce((sum, item) => sum + item.availablePoints, 0),
    description: '用于观察当前页会员可用积分的总体规模。',
    icon: 'i-heroicons-solid:sparkles',
    tone: 'sky' as const,
  },
])

async function loadCurrentTab() {
  loading.value = true
  try {
    if (activeTab.value === 'wallets') {
      const res = await assetApi.getWallets({ page: wallets.page, pageSize: wallets.pageSize, keyword: keyword.value || undefined })
      wallets.items = res.data.items
      wallets.total = res.data.total
      return
    }
    if (activeTab.value === 'walletLogs') {
      const res = await assetApi.getWalletTransactions({ page: walletLogs.page, pageSize: walletLogs.pageSize, keyword: keyword.value || undefined })
      walletLogs.items = res.data.items
      walletLogs.total = res.data.total
      return
    }
    if (activeTab.value === 'pointsAccounts') {
      const res = await assetApi.getPointsAccounts({ page: pointsAccounts.page, pageSize: pointsAccounts.pageSize, keyword: keyword.value || undefined })
      pointsAccounts.items = res.data.items
      pointsAccounts.total = res.data.total
      return
    }

    const res = await assetApi.getPointsLogs({ page: pointsLogs.page, pageSize: pointsLogs.pageSize, keyword: keyword.value || undefined })
    pointsLogs.items = res.data.items
    pointsLogs.total = res.data.total
  }
  finally {
    loading.value = false
  }
}

function handleSearch() {
  wallets.page = 1
  walletLogs.page = 1
  pointsAccounts.page = 1
  pointsLogs.page = 1
  loadCurrentTab()
}

function handleReset() {
  keyword.value = ''
  handleSearch()
}

function handleTabChange() {
  loadCurrentTab()
}

function formatAmount(value: number) {
  return `¥ ${value.toFixed(2)}`
}

function formatDateTime(value?: string) {
  return value ? dayjs(value).format('YYYY-MM-DD HH:mm:ss') : '-'
}

onMounted(() => {
  loadCurrentTab()
})
</script>

<template>
  <div class="space-y-4">
    <div class="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
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
          <FaIcon name="i-heroicons-solid:wallet" class="size-5" />
          <span>资产中心筛选</span>
        </div>
      </div>
      <div class="search-body">
        <div class="search-grid">
          <div class="search-field">
            <label class="search-label">关键词</label>
            <el-input
              v-model="keyword"
              placeholder="搜索用户、业务单号"
              clearable
              @keyup.enter="handleSearch"
            />
          </div>
        </div>
      </div>
      <div class="search-footer">
        <FaButton @click="handleSearch">查询</FaButton>
        <FaButton class="search-reset-btn" @click="handleReset">重置</FaButton>
      </div>
    </FaCard>

    <FaCard>
      <template #header>
        <div class="flex items-center justify-between">
          <div>
            <span class="font-medium">资产中心</span>
            <span class="ml-2 text-sm text-stone-500">钱包、积分与流水总览</span>
          </div>
        </div>
      </template>
      <el-tabs v-model="activeTab" @tab-change="handleTabChange">
        <el-tab-pane label="钱包账户" name="wallets">
          <el-table v-loading="loading" :data="wallets.items">
            <el-table-column prop="userName" label="用户" width="160" />
            <el-table-column prop="userId" label="用户ID" min-width="180" />
            <el-table-column label="可用余额" width="120" align="right">
              <template #default="{ row }">{{ formatAmount(row.availableBalance) }}</template>
            </el-table-column>
            <el-table-column label="冻结余额" width="120" align="right">
              <template #default="{ row }">{{ formatAmount(row.frozenBalance) }}</template>
            </el-table-column>
            <el-table-column label="累计收入" width="120" align="right">
              <template #default="{ row }">{{ formatAmount(row.totalIncome) }}</template>
            </el-table-column>
            <el-table-column label="累计支出" width="120" align="right">
              <template #default="{ row }">{{ formatAmount(row.totalExpense) }}</template>
            </el-table-column>
            <el-table-column label="最后变更" min-width="170">
              <template #default="{ row }">{{ formatDateTime(row.lastChangedAt) }}</template>
            </el-table-column>
          </el-table>
        </el-tab-pane>

        <el-tab-pane label="钱包流水" name="walletLogs">
          <el-table v-loading="loading" :data="walletLogs.items">
            <el-table-column prop="userName" label="用户" width="140" />
            <el-table-column prop="businessType" label="业务类型" width="140" />
            <el-table-column prop="businessNo" label="业务单号" min-width="180" />
            <el-table-column prop="direction" label="方向" width="90" />
            <el-table-column label="变动值" width="110" align="right">
              <template #default="{ row }">{{ formatAmount(row.changeAvailableBalance) }}</template>
            </el-table-column>
            <el-table-column label="变动前" width="110" align="right">
              <template #default="{ row }">{{ formatAmount(row.beforeAvailableBalance) }}</template>
            </el-table-column>
            <el-table-column label="变动后" width="110" align="right">
              <template #default="{ row }">{{ formatAmount(row.afterAvailableBalance) }}</template>
            </el-table-column>
            <el-table-column prop="remark" label="备注" min-width="180" />
            <el-table-column label="创建时间" min-width="170">
              <template #default="{ row }">{{ formatDateTime(row.createdAt) }}</template>
            </el-table-column>
          </el-table>
        </el-tab-pane>

        <el-tab-pane label="积分账户" name="pointsAccounts">
          <el-table v-loading="loading" :data="pointsAccounts.items">
            <el-table-column prop="userName" label="用户" width="160" />
            <el-table-column prop="userId" label="用户ID" min-width="180" />
            <el-table-column prop="availablePoints" label="可用积分" width="120" align="right" />
            <el-table-column prop="frozenPoints" label="冻结积分" width="120" align="right" />
            <el-table-column prop="totalEarnedPoints" label="累计获得" width="120" align="right" />
            <el-table-column prop="totalSpentPoints" label="累计消耗" width="120" align="right" />
            <el-table-column label="最后变更" min-width="170">
              <template #default="{ row }">{{ formatDateTime(row.lastChangedAt) }}</template>
            </el-table-column>
          </el-table>
        </el-tab-pane>

        <el-tab-pane label="积分流水" name="pointsLogs">
          <el-table v-loading="loading" :data="pointsLogs.items">
            <el-table-column prop="userName" label="用户" width="140" />
            <el-table-column prop="businessType" label="业务类型" width="140" />
            <el-table-column prop="businessNo" label="业务单号" min-width="180" />
            <el-table-column prop="direction" label="方向" width="90" />
            <el-table-column prop="changeAvailablePoints" label="变动值" width="100" align="right" />
            <el-table-column prop="beforeAvailablePoints" label="变动前" width="100" align="right" />
            <el-table-column prop="afterAvailablePoints" label="变动后" width="100" align="right" />
            <el-table-column prop="remark" label="备注" min-width="180" />
            <el-table-column label="创建时间" min-width="170">
              <template #default="{ row }">{{ formatDateTime(row.createdAt) }}</template>
            </el-table-column>
          </el-table>
        </el-tab-pane>
      </el-tabs>
    </FaCard>
  </div>
</template>
