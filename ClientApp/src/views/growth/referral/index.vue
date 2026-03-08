<script setup lang="ts">
import dayjs from 'dayjs'
import growthApi from '@/api/modules/growth'

defineOptions({
  name: 'GrowthReferralPage',
})

const searchForm = ref({
  keyword: '',
  status: undefined as number | undefined,
})

const tableData = ref<Api.Growth.ReferralRelation[]>([])
const loading = ref(false)
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(20)

const detailDialogVisible = ref(false)
const detailLoading = ref(false)
const currentRelation = ref<Api.Growth.ReferralRelation | null>(null)

const statusOptions = [
  { label: '待绑定', value: 0 },
  { label: '已绑定', value: 1 },
  { label: '已失效', value: 2 },
]

const channelMap: Record<string, string> = {
  direct: '直接邀请',
  inviteCode: '邀请码',
  shareLink: '分享链接',
  campaign: '活动归因',
}

const statusMap: Record<number, { label: string, type: 'info' | 'success' | 'warning' }> = {
  0: { label: '待绑定', type: 'warning' },
  1: { label: '已绑定', type: 'success' },
  2: { label: '已失效', type: 'info' },
}

const summaryCards = computed(() => [
  {
    title: '邀请关系',
    value: total.value,
    icon: 'i-heroicons-solid:user-plus',
    tone: 'bg-primary/8 text-primary',
  },
  {
    title: '当前页已绑定',
    value: tableData.value.filter(item => item.status === 1).length,
    icon: 'i-heroicons-solid:link',
    tone: 'bg-emerald-500/10 text-emerald-600',
  },
  {
    title: '当前页待绑定',
    value: tableData.value.filter(item => item.status === 0).length,
    icon: 'i-heroicons-solid:clock',
    tone: 'bg-amber-500/10 text-amber-600',
  },
  {
    title: '当前页奖励金额',
    value: formatAmount(tableData.value.reduce((sum, item) => sum + (item.totalRewardAmount || 0), 0)),
    icon: 'i-heroicons-solid:gift',
    tone: 'bg-sky-500/10 text-sky-600',
  },
])

async function getDistributionList() {
  loading.value = true
  try {
    const res = await growthApi.getDistributionList({
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
  getDistributionList()
}

function handleReset() {
  searchForm.value = {
    keyword: '',
    status: undefined,
  }
  currentPage.value = 1
  getDistributionList()
}

async function handleViewDetail(row: Api.Growth.ReferralRelation) {
  detailDialogVisible.value = true
  detailLoading.value = true
  currentRelation.value = row
  try {
    const res = await growthApi.getDistributionDetail(row.id)
    currentRelation.value = res.data
  }
  finally {
    detailLoading.value = false
  }
}

function handlePageChange(page: number) {
  currentPage.value = page
  getDistributionList()
}

function handleSizeChange(size: number) {
  pageSize.value = size
  currentPage.value = 1
  getDistributionList()
}

function formatAmount(value: number) {
  return `￥${value.toFixed(2)}`
}

onMounted(() => {
  getDistributionList()
})
</script>

<template>
  <div class="growth-referral-page">
    <div class="grid mb-4 gap-4 md:grid-cols-2 xl:grid-cols-4">
      <FaCard
        v-for="card in summaryCards"
        :key="card.title"
      >
        <div class="flex items-center justify-between gap-4">
          <div class="space-y-1">
            <p class="text-sm text-stone-500">
              {{ card.title }}
            </p>
            <p class="text-2xl text-stone-900 font-semibold">
              {{ card.value }}
            </p>
          </div>
          <div class="size-12 flex items-center justify-center rounded-2xl" :class="card.tone">
            <FaIcon :name="card.icon" class="size-6" />
          </div>
        </div>
      </FaCard>
    </div>

    <FaCard class="search-card mb-4">
      <div class="search-header">
        <div class="search-title">
          <FaIcon name="i-heroicons-solid:magnifying-glass" class="size-5" />
          <span>邀请筛选</span>
        </div>
      </div>
      <div class="search-body">
        <div class="search-grid">
          <div class="search-field">
            <label class="search-label">关键字</label>
            <el-input
              v-model="searchForm.keyword"
              placeholder="搜索邀请人、被邀请人或邀请码"
              clearable
            >
              <template #prefix>
                <FaIcon name="i-heroicons-solid:user-plus" class="size-4 text-stone-400" />
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
            <span class="font-medium">邀请关系</span>
            <span class="ml-2 text-sm text-stone-500">第 6 阶段增长中心</span>
          </div>
          <FaButton variant="ghost" @click="getDistributionList">
            <template #icon>
              <FaIcon name="i-heroicons-solid:arrow-path" />
            </template>
            刷新
          </FaButton>
        </div>
      </template>

      <el-table
        v-loading="loading"
        :data="tableData"
      >
        <el-table-column label="邀请关系" min-width="220">
          <template #default="{ row }">
            <div class="space-y-1">
              <div class="text-sm text-stone-900 font-medium">
                {{ row.inviterName || '-' }} -> {{ row.inviteeName || '-' }}
              </div>
              <div class="text-xs text-stone-500">
                {{ row.referralCode || '暂无邀请码' }}
              </div>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="来源渠道" width="120" align="center">
          <template #default="{ row }">
            <el-tag size="small" type="info">
              {{ channelMap[row.channel || ''] || row.channel || '-' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="attributionSource" label="归因来源" min-width="140" show-overflow-tooltip>
          <template #default="{ row }">
            {{ row.attributionSource || '-' }}
          </template>
        </el-table-column>
        <el-table-column label="订单数" width="100" align="center">
          <template #default="{ row }">
            {{ row.totalOrders }}
          </template>
        </el-table-column>
        <el-table-column label="奖励金额" width="120" align="right">
          <template #default="{ row }">
            {{ formatAmount(row.totalRewardAmount || 0) }}
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="statusMap[row.status]?.type || 'info'" size="small">
              {{ statusMap[row.status]?.label || '未知' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="邀请时间" width="180">
          <template #default="{ row }">
            {{ row.invitedAt ? dayjs(row.invitedAt).format('YYYY-MM-DD HH:mm:ss') : '-' }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120" fixed="right">
          <template #default="{ row }">
            <FaButton variant="ghost" size="sm" @click="handleViewDetail(row)">
              <template #icon>
                <FaIcon name="i-iconoir:eye" />
              </template>
              详情
            </FaButton>
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

    <el-dialog
      v-model="detailDialogVisible"
      title="邀请详情"
      width="720px"
    >
      <div v-loading="detailLoading">
        <el-empty v-if="!currentRelation" description="暂无邀请数据" />
        <div v-else class="space-y-4">
          <el-descriptions :column="2" border>
            <el-descriptions-item label="邀请人">
              {{ currentRelation.inviterName || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="被邀请人">
              {{ currentRelation.inviteeName || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="邀请码">
              {{ currentRelation.referralCode || '暂无邀请码' }}
            </el-descriptions-item>
            <el-descriptions-item label="状态">
              {{ statusMap[currentRelation.status]?.label || '未知' }}
            </el-descriptions-item>
            <el-descriptions-item label="来源渠道">
              {{ channelMap[currentRelation.channel || ''] || currentRelation.channel || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="归因来源">
              {{ currentRelation.attributionSource || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="订单数">
              {{ currentRelation.totalOrders }}
            </el-descriptions-item>
            <el-descriptions-item label="奖励金额">
              {{ formatAmount(currentRelation.totalRewardAmount || 0) }}
            </el-descriptions-item>
            <el-descriptions-item label="邀请时间">
              {{ currentRelation.invitedAt ? dayjs(currentRelation.invitedAt).format('YYYY-MM-DD HH:mm:ss') : '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="首单时间">
              {{ currentRelation.firstOrderAt ? dayjs(currentRelation.firstOrderAt).format('YYYY-MM-DD HH:mm:ss') : '-' }}
            </el-descriptions-item>
          </el-descriptions>
        </div>
      </div>
    </el-dialog>
  </div>
</template>
