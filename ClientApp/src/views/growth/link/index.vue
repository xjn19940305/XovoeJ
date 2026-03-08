<script setup lang="ts">
import type { TagProps } from 'element-plus'
import dayjs from 'dayjs'
import { ElMessage, ElMessageBox } from 'element-plus'
import growthApi from '@/api/modules/growth'
import AdminMetricCard from '@/components/admin/AdminMetricCard.vue'

defineOptions({
  name: 'GrowthReferralLinkPage',
})

const loading = ref(false)
const tableData = ref<Api.Growth.ReferralLink[]>([])
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(20)

const searchForm = reactive({
  keyword: '',
  status: undefined as number | undefined,
  channel: undefined as string | undefined,
})

const detailDialogVisible = ref(false)
const detailLoading = ref(false)
const currentLink = ref<Api.Growth.ReferralLink | null>(null)

const channelOptions = [
  { label: '分享链接', value: 'shareLink' },
  { label: '邀请码', value: 'inviteCode' },
  { label: '活动归因', value: 'campaign' },
  { label: '直接投放', value: 'direct' },
]

const channelMap: Record<string, string> = {
  shareLink: '分享链接',
  inviteCode: '邀请码',
  campaign: '活动归因',
  direct: '直接投放',
}

const statusMap: Record<number, { label: string, type: TagProps['type'] }> = {
  0: { label: '待启用', type: 'warning' },
  1: { label: '生效中', type: 'success' },
  2: { label: '已停用', type: 'info' },
}

const summaryCards = computed(() => {
  const clicks = tableData.value.reduce((sum, item) => sum + item.clickCount, 0)
  const signups = tableData.value.reduce((sum, item) => sum + item.signupCount, 0)
  const firstOrders = tableData.value.reduce((sum, item) => sum + item.firstOrderCount, 0)
  const rewards = tableData.value.reduce((sum, item) => sum + item.rewardAmount, 0)

  return [
    {
      title: '推广链接',
      value: total.value,
      description: '当前筛选范围内的推广链接总量，便于观察渠道铺设覆盖面。',
      icon: 'i-heroicons-solid:share',
      tone: 'blue' as const,
    },
    {
      title: '当前页点击量',
      value: clicks,
      description: '用于判断链接曝光后的点击反馈，衡量投放吸引力。',
      icon: 'i-heroicons-solid:cursor-arrow-rays',
      tone: 'sky' as const,
    },
    {
      title: '当前页注册数',
      value: signups,
      description: '直接反映推广链接带来的有效注册规模。',
      icon: 'i-heroicons-solid:user-group',
      tone: 'emerald' as const,
    },
    {
      title: '当前页奖励金额',
      value: formatAmount(rewards),
      description: '按当前列表范围汇总奖励支出，便于联动渠道成本判断。',
      icon: 'i-heroicons-solid:banknotes',
      tone: 'amber' as const,
    },
    {
      title: '当前页首单转化',
      value: firstOrders,
      description: '首单转化能更直接反映该批链接的业务承接质量。',
      icon: 'i-heroicons-solid:chart-bar-square',
      tone: 'violet' as const,
    },
  ]
})

async function getList() {
  loading.value = true
  try {
    const res = await growthApi.getReferralLinkList({
      page: currentPage.value,
      pageSize: pageSize.value,
      keyword: searchForm.keyword || undefined,
      status: searchForm.status,
      channel: searchForm.channel,
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
  getList()
}

function handleReset() {
  searchForm.keyword = ''
  searchForm.status = undefined
  searchForm.channel = undefined
  currentPage.value = 1
  getList()
}

async function handleViewDetail(row: Api.Growth.ReferralLink) {
  detailDialogVisible.value = true
  detailLoading.value = true
  currentLink.value = row
  try {
    const res = await growthApi.getReferralLinkDetail(row.id)
    currentLink.value = res.data
  }
  finally {
    detailLoading.value = false
  }
}

async function handleUpdateStatus(row: Api.Growth.ReferralLink, status: number) {
  const actionText = status === 1 ? '启用' : status === 2 ? '停用' : '设为待启用'
  await ElMessageBox.confirm(`确认${actionText}推广链接“${row.name}”吗？`, '状态变更', {
    type: 'warning',
  })

  await growthApi.updateReferralLinkStatus(row.id, { status })
  ElMessage.success(`推广链接已${actionText}`)
  await getList()
}

function handlePageChange(page: number) {
  currentPage.value = page
  getList()
}

function handleSizeChange(size: number) {
  pageSize.value = size
  currentPage.value = 1
  getList()
}

function formatAmount(value: number) {
  return `¥ ${value.toFixed(2)}`
}

function formatTime(value?: string) {
  return value ? dayjs(value).format('YYYY-MM-DD HH:mm:ss') : '-'
}

onMounted(() => {
  getList()
})
</script>

<template>
  <div class="growth-referral-link-page">
    <div class="grid mb-4 gap-4 md:grid-cols-2 xl:grid-cols-5">
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
          <span>推广链接筛选</span>
        </div>
      </div>
      <div class="search-body">
        <div class="search-grid">
          <div class="search-field">
            <label class="search-label">关键词</label>
            <el-input
              v-model="searchForm.keyword"
              placeholder="搜索链接名称、链接编码、负责人或活动名"
              clearable
              @keyup.enter="handleSearch"
            />
          </div>
          <div class="search-field">
            <label class="search-label">投放渠道</label>
            <el-select v-model="searchForm.channel" placeholder="全部渠道" clearable class="w-full">
              <el-option
                v-for="item in channelOptions"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </el-select>
          </div>
          <div class="search-field">
            <label class="search-label">链接状态</label>
            <el-select v-model="searchForm.status" placeholder="全部状态" clearable class="w-full">
              <el-option label="待启用" :value="0" />
              <el-option label="生效中" :value="1" />
              <el-option label="已停用" :value="2" />
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
            <span class="font-medium">推广链接</span>
            <span class="ml-2 text-sm text-stone-500">增长中心 / 渠道归因</span>
          </div>
          <FaButton variant="ghost" @click="getList">
            <template #icon>
              <FaIcon name="i-heroicons-solid:arrow-path" />
            </template>
            刷新
          </FaButton>
        </div>
      </template>

      <el-table v-loading="loading" :data="tableData">
        <el-table-column label="链接信息" min-width="220">
          <template #default="{ row }">
            <div class="space-y-1">
              <div class="text-sm text-stone-900 font-medium">
                {{ row.name }}
              </div>
              <div class="text-xs text-stone-500">
                {{ row.code }}
              </div>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="负责人" width="100" align="center">
          <template #default="{ row }">
            {{ row.ownerName || '-' }}
          </template>
        </el-table-column>
        <el-table-column label="渠道" width="110" align="center">
          <template #default="{ row }">
            <el-tag size="small" type="info">
              {{ channelMap[row.channel || ''] || row.channel || '-' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="campaignName" label="活动名称" min-width="140" show-overflow-tooltip />
        <el-table-column prop="attributionSource" label="归因来源" min-width="140" show-overflow-tooltip />
        <el-table-column label="点击/注册/首单" width="150" align="center">
          <template #default="{ row }">
            {{ row.clickCount }}/{{ row.signupCount }}/{{ row.firstOrderCount }}
          </template>
        </el-table-column>
        <el-table-column label="奖励金额" width="120" align="right">
          <template #default="{ row }">
            {{ formatAmount(row.rewardAmount) }}
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="statusMap[row.status]?.type || 'info'" size="small">
              {{ statusMap[row.status]?.label || '未知状态' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="最近访问" width="180">
          <template #default="{ row }">
            {{ formatTime(row.lastVisitAt) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="220" fixed="right">
          <template #default="{ row }">
            <div class="flex flex-wrap justify-end gap-2">
              <FaButton
                v-if="row.status !== 1"
                size="sm"
                @click="handleUpdateStatus(row, 1)"
              >
                启用
              </FaButton>
              <FaButton
                v-if="row.status !== 2"
                size="sm"
                variant="outline"
                @click="handleUpdateStatus(row, 2)"
              >
                停用
              </FaButton>
              <FaButton
                v-if="row.status !== 0"
                size="sm"
                variant="outline"
                @click="handleUpdateStatus(row, 0)"
              >
                待启用
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
          :page-sizes="[20, 50, 100]"
          :total="total"
          layout="total, sizes, prev, pager, next, jumper"
          @current-change="handlePageChange"
          @size-change="handleSizeChange"
        />
      </div>
    </FaCard>

    <el-dialog v-model="detailDialogVisible" title="推广链接详情" width="760px">
      <div v-loading="detailLoading">
        <el-empty v-if="!currentLink" description="暂无推广链接数据" />
        <div v-else class="space-y-4">
          <el-descriptions :column="2" border>
            <el-descriptions-item label="链接名称">
              {{ currentLink.name }}
            </el-descriptions-item>
            <el-descriptions-item label="链接编码">
              {{ currentLink.code }}
            </el-descriptions-item>
            <el-descriptions-item label="负责人">
              {{ currentLink.ownerName || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="投放渠道">
              {{ channelMap[currentLink.channel || ''] || currentLink.channel || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="活动名称">
              {{ currentLink.campaignName || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="归因来源">
              {{ currentLink.attributionSource || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="落地路径">
              {{ currentLink.landingPath || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="完整链接">
              <span class="break-all">{{ currentLink.fullUrl || '-' }}</span>
            </el-descriptions-item>
            <el-descriptions-item label="点击量">
              {{ currentLink.clickCount }}
            </el-descriptions-item>
            <el-descriptions-item label="注册数">
              {{ currentLink.signupCount }}
            </el-descriptions-item>
            <el-descriptions-item label="首单数">
              {{ currentLink.firstOrderCount }}
            </el-descriptions-item>
            <el-descriptions-item label="奖励金额">
              {{ formatAmount(currentLink.rewardAmount) }}
            </el-descriptions-item>
            <el-descriptions-item label="状态">
              {{ statusMap[currentLink.status]?.label || '未知状态' }}
            </el-descriptions-item>
            <el-descriptions-item label="失效时间">
              {{ formatTime(currentLink.expireAt) }}
            </el-descriptions-item>
            <el-descriptions-item label="最近访问">
              {{ formatTime(currentLink.lastVisitAt) }}
            </el-descriptions-item>
            <el-descriptions-item label="创建时间">
              {{ formatTime(currentLink.createdAt) }}
            </el-descriptions-item>
          </el-descriptions>
        </div>
      </div>
    </el-dialog>
  </div>
</template>
