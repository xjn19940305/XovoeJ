<script setup lang="ts">
import type { TagProps } from 'element-plus'
import dayjs from 'dayjs'
import growthApi from '@/api/modules/growth'

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
      icon: 'i-heroicons-solid:share',
      tone: 'bg-primary/8 text-primary',
    },
    {
      title: '当前页点击量',
      value: clicks,
      icon: 'i-heroicons-solid:cursor-arrow-rays',
      tone: 'bg-sky-500/10 text-sky-600',
    },
    {
      title: '当前页注册数',
      value: signups,
      icon: 'i-heroicons-solid:user-group',
      tone: 'bg-emerald-500/10 text-emerald-600',
    },
    {
      title: '当前页奖励金额',
      value: formatAmount(rewards),
      icon: 'i-heroicons-solid:banknotes',
      tone: 'bg-amber-500/10 text-amber-600',
    },
    {
      title: '当前页首单转化',
      value: firstOrders,
      icon: 'i-heroicons-solid:chart-bar-square',
      tone: 'bg-violet-500/10 text-violet-600',
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
  return `￥${value.toFixed(2)}`
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
      <FaCard v-for="card in summaryCards" :key="card.title">
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

    <FaCard class="mb-4">
      <div class="mb-4 flex items-center gap-2 text-base font-medium">
        <FaIcon name="i-heroicons-solid:funnel" class="size-5" />
        <span>推广链接筛选</span>
      </div>
      <div class="grid gap-4 lg:grid-cols-3">
        <div>
          <div class="mb-2 text-sm text-gray-500">
            关键字
          </div>
          <el-input
            v-model="searchForm.keyword"
            placeholder="搜索链接名称、链接编码、负责人或活动名"
            clearable
            @keyup.enter="handleSearch"
          />
        </div>
        <div>
          <div class="mb-2 text-sm text-gray-500">
            投放渠道
          </div>
          <el-select v-model="searchForm.channel" placeholder="全部渠道" clearable class="w-full">
            <el-option
              v-for="item in channelOptions"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </div>
        <div>
          <div class="mb-2 text-sm text-gray-500">
            链接状态
          </div>
          <el-select v-model="searchForm.status" placeholder="全部状态" clearable class="w-full">
            <el-option label="待启用" :value="0" />
            <el-option label="生效中" :value="1" />
            <el-option label="已停用" :value="2" />
          </el-select>
        </div>
      </div>
      <div class="mt-4 flex gap-3">
        <FaButton @click="handleSearch">
          <template #icon>
            <FaIcon name="i-heroicons-solid:magnifying-glass" />
          </template>
          查询
        </FaButton>
        <FaButton @click="handleReset">
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
            <span class="ml-2 text-sm text-stone-500">第 6 阶段 / 推广链接与归因</span>
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
        <el-table-column label="操作" width="120" fixed="right">
          <template #default="{ row }">
            <FaButton variant="ghost" size="sm" @click="handleViewDetail(row)">
              详情
            </FaButton>
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
