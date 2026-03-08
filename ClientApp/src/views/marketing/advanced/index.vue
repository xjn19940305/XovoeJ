<script setup lang="ts">
import type { TagProps } from 'element-plus'
import dayjs from 'dayjs'
import marketingApi from '@/api/modules/marketing'

defineOptions({
  name: 'MarketingAdvancedPage',
})

type AdvancedModule = 'seckill' | 'groupBuy' | 'bargain'

const activeModule = ref<AdvancedModule>('seckill')

const searchForm = reactive({
  keyword: '',
  status: undefined as number | undefined,
})

const pageState = reactive({
  seckill: {
    loading: false,
    items: [] as Api.Marketing.SeckillActivity[],
    total: 0,
    currentPage: 1,
    pageSize: 20,
  },
  groupBuy: {
    loading: false,
    items: [] as Api.Marketing.GroupBuyActivity[],
    total: 0,
    currentPage: 1,
    pageSize: 20,
  },
  bargain: {
    loading: false,
    items: [] as Api.Marketing.BargainActivity[],
    total: 0,
    currentPage: 1,
    pageSize: 20,
  },
})

const detailDialogVisible = ref(false)
const detailLoading = ref(false)
const currentSeckill = ref<Api.Marketing.SeckillActivity | null>(null)
const currentGroupBuy = ref<Api.Marketing.GroupBuyActivity | null>(null)
const currentBargain = ref<Api.Marketing.BargainActivity | null>(null)

const statusMap: Record<number, { label: string, type: TagProps['type'] }> = {
  0: { label: '预热中', type: 'warning' },
  1: { label: '进行中', type: 'success' },
  2: { label: '已结束', type: 'info' },
}

const moduleLabelMap: Record<AdvancedModule, string> = {
  seckill: '秒杀活动',
  groupBuy: '拼团活动',
  bargain: '砍价活动',
}

const currentCards = computed(() => {
  if (activeModule.value === 'seckill') {
    const items = pageState.seckill.items
    return [
      {
        title: '秒杀活动',
        value: pageState.seckill.total,
        icon: 'i-heroicons-solid:bolt',
        tone: 'bg-rose-500/10 text-rose-600',
      },
      {
        title: '当前页进行中',
        value: items.filter(item => item.status === 1).length,
        icon: 'i-heroicons-solid:fire',
        tone: 'bg-emerald-500/10 text-emerald-600',
      },
      {
        title: '当前页已售库存',
        value: items.reduce((sum, item) => sum + item.soldStock, 0),
        icon: 'i-heroicons-solid:cube',
        tone: 'bg-sky-500/10 text-sky-600',
      },
      {
        title: '当前页成交金额',
        value: formatAmount(items.reduce((sum, item) => sum + item.seckillPrice * item.orderCount, 0)),
        icon: 'i-heroicons-solid:banknotes',
        tone: 'bg-violet-500/10 text-violet-600',
      },
    ]
  }

  if (activeModule.value === 'groupBuy') {
    const items = pageState.groupBuy.items
    return [
      {
        title: '拼团活动',
        value: pageState.groupBuy.total,
        icon: 'i-heroicons-solid:users',
        tone: 'bg-primary/10 text-primary',
      },
      {
        title: '当前页成团数',
        value: items.reduce((sum, item) => sum + item.successGroupCount, 0),
        icon: 'i-heroicons-solid:user-group',
        tone: 'bg-emerald-500/10 text-emerald-600',
      },
      {
        title: '当前页开团人数',
        value: items.reduce((sum, item) => sum + item.participantCount, 0),
        icon: 'i-heroicons-solid:user-plus',
        tone: 'bg-sky-500/10 text-sky-600',
      },
      {
        title: '当前页团长奖励',
        value: formatAmount(items.reduce((sum, item) => sum + item.groupLeaderReward * item.successGroupCount, 0)),
        icon: 'i-heroicons-solid:gift-top',
        tone: 'bg-amber-500/10 text-amber-600',
      },
    ]
  }

  const items = pageState.bargain.items
  return [
    {
      title: '砍价活动',
      value: pageState.bargain.total,
      icon: 'i-heroicons-solid:scissors',
      tone: 'bg-orange-500/10 text-orange-600',
    },
    {
      title: '当前页发起人数',
      value: items.reduce((sum, item) => sum + item.participantCount, 0),
      icon: 'i-heroicons-solid:user-circle',
      tone: 'bg-sky-500/10 text-sky-600',
    },
    {
      title: '当前页助力次数',
      value: items.reduce((sum, item) => sum + item.helperCount, 0),
      icon: 'i-heroicons-solid:hand-raised',
      tone: 'bg-emerald-500/10 text-emerald-600',
    },
    {
      title: '当前页成功砍成',
      value: items.reduce((sum, item) => sum + item.successCount, 0),
      icon: 'i-heroicons-solid:trophy',
      tone: 'bg-amber-500/10 text-amber-600',
    },
  ]
})

const detailTitle = computed(() => {
  if (currentSeckill.value) {
    return '秒杀活动详情'
  }
  if (currentGroupBuy.value) {
    return '拼团活动详情'
  }
  return '砍价活动详情'
})

function getModuleState(module: AdvancedModule) {
  if (module === 'seckill') {
    return pageState.seckill
  }
  if (module === 'groupBuy') {
    return pageState.groupBuy
  }
  return pageState.bargain
}

async function getSeckillList() {
  pageState.seckill.loading = true
  try {
    const res = await marketingApi.getSeckillList({
      page: pageState.seckill.currentPage,
      pageSize: pageState.seckill.pageSize,
      keyword: searchForm.keyword || undefined,
      status: searchForm.status,
    })
    pageState.seckill.items = res.data.items
    pageState.seckill.total = res.data.total
  }
  finally {
    pageState.seckill.loading = false
  }
}

async function getGroupBuyList() {
  pageState.groupBuy.loading = true
  try {
    const res = await marketingApi.getGroupBuyList({
      page: pageState.groupBuy.currentPage,
      pageSize: pageState.groupBuy.pageSize,
      keyword: searchForm.keyword || undefined,
      status: searchForm.status,
    })
    pageState.groupBuy.items = res.data.items
    pageState.groupBuy.total = res.data.total
  }
  finally {
    pageState.groupBuy.loading = false
  }
}

async function getBargainList() {
  pageState.bargain.loading = true
  try {
    const res = await marketingApi.getBargainList({
      page: pageState.bargain.currentPage,
      pageSize: pageState.bargain.pageSize,
      keyword: searchForm.keyword || undefined,
      status: searchForm.status,
    })
    pageState.bargain.items = res.data.items
    pageState.bargain.total = res.data.total
  }
  finally {
    pageState.bargain.loading = false
  }
}

function getListByModule(module = activeModule.value) {
  if (module === 'seckill') {
    return getSeckillList()
  }
  if (module === 'groupBuy') {
    return getGroupBuyList()
  }
  return getBargainList()
}

function handleSearch() {
  getModuleState(activeModule.value).currentPage = 1
  getListByModule()
}

function handleReset() {
  searchForm.keyword = ''
  searchForm.status = undefined
  getModuleState(activeModule.value).currentPage = 1
  getListByModule()
}

function resetDetailState() {
  currentSeckill.value = null
  currentGroupBuy.value = null
  currentBargain.value = null
}

async function handleViewSeckillDetail(row: Api.Marketing.SeckillActivity) {
  detailDialogVisible.value = true
  detailLoading.value = true
  resetDetailState()
  currentSeckill.value = row
  try {
    const res = await marketingApi.getSeckillDetail(row.id)
    currentSeckill.value = res.data
  }
  finally {
    detailLoading.value = false
  }
}

async function handleViewGroupBuyDetail(row: Api.Marketing.GroupBuyActivity) {
  detailDialogVisible.value = true
  detailLoading.value = true
  resetDetailState()
  currentGroupBuy.value = row
  try {
    const res = await marketingApi.getGroupBuyDetail(row.id)
    currentGroupBuy.value = res.data
  }
  finally {
    detailLoading.value = false
  }
}

async function handleViewBargainDetail(row: Api.Marketing.BargainActivity) {
  detailDialogVisible.value = true
  detailLoading.value = true
  resetDetailState()
  currentBargain.value = row
  try {
    const res = await marketingApi.getBargainDetail(row.id)
    currentBargain.value = res.data
  }
  finally {
    detailLoading.value = false
  }
}

function handlePageChange(page: number) {
  getModuleState(activeModule.value).currentPage = page
  getListByModule()
}

function handleSizeChange(size: number) {
  const state = getModuleState(activeModule.value)
  state.pageSize = size
  state.currentPage = 1
  getListByModule()
}

function switchModule(module: AdvancedModule) {
  activeModule.value = module
  searchForm.keyword = ''
  searchForm.status = undefined
  if (!getModuleState(module).items.length) {
    getListByModule(module)
  }
}

function handleModuleChange(value: string | number | boolean | undefined) {
  if (value === 'seckill' || value === 'groupBuy' || value === 'bargain') {
    switchModule(value)
  }
}

function formatAmount(value: number) {
  return `￥${value.toFixed(2)}`
}

function formatTime(value?: string) {
  return value ? dayjs(value).format('YYYY-MM-DD HH:mm:ss') : '-'
}

function formatProgress(soldStock: number, totalStock: number) {
  if (!totalStock) {
    return '0%'
  }
  return `${Math.min(100, Math.round((soldStock / totalStock) * 100))}%`
}

onMounted(() => {
  getSeckillList()
})
</script>

<template>
  <div class="marketing-advanced-page">
    <div class="mb-4 flex items-center justify-between gap-4">
      <div>
        <div class="text-xl text-stone-900 font-semibold">
          高级营销
        </div>
        <div class="mt-1 text-sm text-stone-500">
          统一管理秒杀、拼团和砍价三类活动，后续继续在这一页扩展更深的活动配置。
        </div>
      </div>
      <el-radio-group :model-value="activeModule" @update:model-value="handleModuleChange">
        <el-radio-button label="seckill">
          秒杀活动
        </el-radio-button>
        <el-radio-button label="groupBuy">
          拼团活动
        </el-radio-button>
        <el-radio-button label="bargain">
          砍价活动
        </el-radio-button>
      </el-radio-group>
    </div>

    <div class="grid mb-4 gap-4 md:grid-cols-2 xl:grid-cols-4">
      <FaCard v-for="card in currentCards" :key="card.title">
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
        <span>{{ moduleLabelMap[activeModule] }}筛选</span>
      </div>
      <div class="grid gap-4 lg:grid-cols-2">
        <div>
          <div class="mb-2 text-sm text-gray-500">
            关键词
          </div>
          <el-input
            v-model="searchForm.keyword"
            :placeholder="`搜索${moduleLabelMap[activeModule]}名称、编码、商品名或规格`"
            clearable
            @keyup.enter="handleSearch"
          />
        </div>
        <div>
          <div class="mb-2 text-sm text-gray-500">
            活动状态
          </div>
          <el-select v-model="searchForm.status" placeholder="全部状态" clearable class="w-full">
            <el-option label="预热中" :value="0" />
            <el-option label="进行中" :value="1" />
            <el-option label="已结束" :value="2" />
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

    <FaCard class="mb-4">
      <template #header>
        <div class="flex items-center justify-between">
          <div>
            <span class="font-medium">{{ moduleLabelMap[activeModule] }}</span>
            <span class="ml-2 text-sm text-stone-500">第 6 阶段高级营销</span>
          </div>
          <FaButton variant="ghost" @click="getListByModule()">
            <template #icon>
              <FaIcon name="i-heroicons-solid:arrow-path" />
            </template>
            刷新
          </FaButton>
        </div>
      </template>

      <el-table v-if="activeModule === 'seckill'" v-loading="pageState.seckill.loading" :data="pageState.seckill.items">
        <el-table-column label="活动信息" min-width="220">
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
        <el-table-column label="秒杀商品" min-width="180" show-overflow-tooltip>
          <template #default="{ row }">
            <div class="space-y-1">
              <div>{{ row.productName || '-' }}</div>
              <div class="text-xs text-stone-500">
                {{ row.skuName || '-' }}
              </div>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="价格" width="130" align="right">
          <template #default="{ row }">
            <div class="text-rose-600 font-medium">
              {{ formatAmount(row.seckillPrice) }}
            </div>
            <div class="text-xs text-stone-400 line-through">
              {{ formatAmount(row.originalPrice) }}
            </div>
          </template>
        </el-table-column>
        <el-table-column label="库存进度" width="120" align="center">
          <template #default="{ row }">
            <div>{{ row.soldStock }}/{{ row.totalStock }}</div>
            <div class="text-xs text-stone-500">
              {{ formatProgress(row.soldStock, row.totalStock) }}
            </div>
          </template>
        </el-table-column>
        <el-table-column label="限购" width="90" align="center">
          <template #default="{ row }">
            {{ row.purchaseLimit }} 件
          </template>
        </el-table-column>
        <el-table-column label="参与/订单" width="110" align="center">
          <template #default="{ row }">
            {{ row.participantCount }}/{{ row.orderCount }}
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="statusMap[row.status]?.type || 'info'" size="small">
              {{ statusMap[row.status]?.label || '未知状态' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="活动时间" min-width="180">
          <template #default="{ row }">
            <div>{{ formatTime(row.startTime) }}</div>
            <div class="text-xs text-stone-500">
              至 {{ formatTime(row.endTime) }}
            </div>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120" fixed="right">
          <template #default="{ row }">
            <FaButton variant="ghost" size="sm" @click="handleViewSeckillDetail(row)">
              详情
            </FaButton>
          </template>
        </el-table-column>
      </el-table>

      <el-table v-else-if="activeModule === 'groupBuy'" v-loading="pageState.groupBuy.loading" :data="pageState.groupBuy.items">
        <el-table-column label="活动信息" min-width="220">
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
        <el-table-column label="拼团商品" min-width="180" show-overflow-tooltip>
          <template #default="{ row }">
            <div class="space-y-1">
              <div>{{ row.productName || '-' }}</div>
              <div class="text-xs text-stone-500">
                {{ row.skuName || '-' }}
              </div>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="拼团价" width="130" align="right">
          <template #default="{ row }">
            <div class="text-primary font-medium">
              {{ formatAmount(row.groupPrice) }}
            </div>
            <div class="text-xs text-stone-400 line-through">
              {{ formatAmount(row.originalPrice) }}
            </div>
          </template>
        </el-table-column>
        <el-table-column label="成团门槛" width="100" align="center">
          <template #default="{ row }">
            {{ row.groupSize }} 人团
          </template>
        </el-table-column>
        <el-table-column label="开团/成团" width="120" align="center">
          <template #default="{ row }">
            {{ row.participantCount }}/{{ row.successGroupCount }}
          </template>
        </el-table-column>
        <el-table-column label="失败团数" width="90" align="center">
          <template #default="{ row }">
            {{ row.failedGroupCount }}
          </template>
        </el-table-column>
        <el-table-column label="团长奖励" width="110" align="right">
          <template #default="{ row }">
            {{ formatAmount(row.groupLeaderReward) }}
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="statusMap[row.status]?.type || 'info'" size="small">
              {{ statusMap[row.status]?.label || '未知状态' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="活动时间" min-width="180">
          <template #default="{ row }">
            <div>{{ formatTime(row.startTime) }}</div>
            <div class="text-xs text-stone-500">
              至 {{ formatTime(row.endTime) }}
            </div>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120" fixed="right">
          <template #default="{ row }">
            <FaButton variant="ghost" size="sm" @click="handleViewGroupBuyDetail(row)">
              详情
            </FaButton>
          </template>
        </el-table-column>
      </el-table>

      <el-table v-else v-loading="pageState.bargain.loading" :data="pageState.bargain.items">
        <el-table-column label="活动信息" min-width="220">
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
        <el-table-column label="砍价商品" min-width="180" show-overflow-tooltip>
          <template #default="{ row }">
            <div class="space-y-1">
              <div>{{ row.productName || '-' }}</div>
              <div class="text-xs text-stone-500">
                {{ row.skuName || '-' }}
              </div>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="价格区间" width="140" align="right">
          <template #default="{ row }">
            <div class="text-orange-600 font-medium">
              {{ formatAmount(row.currentLowestPrice) }}
            </div>
            <div class="text-xs text-stone-500">
              底价 {{ formatAmount(row.floorPrice) }}
            </div>
            <div class="text-xs text-stone-400 line-through">
              原价 {{ formatAmount(row.originalPrice) }}
            </div>
          </template>
        </el-table-column>
        <el-table-column label="发起/助力" width="120" align="center">
          <template #default="{ row }">
            {{ row.participantCount }}/{{ row.helperCount }}
          </template>
        </el-table-column>
        <el-table-column label="成功砍成" width="90" align="center">
          <template #default="{ row }">
            {{ row.successCount }}
          </template>
        </el-table-column>
        <el-table-column label="限购" width="90" align="center">
          <template #default="{ row }">
            {{ row.purchaseLimit }} 件
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="statusMap[row.status]?.type || 'info'" size="small">
              {{ statusMap[row.status]?.label || '未知状态' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="活动时间" min-width="180">
          <template #default="{ row }">
            <div>{{ formatTime(row.startTime) }}</div>
            <div class="text-xs text-stone-500">
              至 {{ formatTime(row.endTime) }}
            </div>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120" fixed="right">
          <template #default="{ row }">
            <FaButton variant="ghost" size="sm" @click="handleViewBargainDetail(row)">
              详情
            </FaButton>
          </template>
        </el-table-column>
      </el-table>

      <div class="mt-4 flex justify-end">
        <el-pagination
          :current-page="getModuleState(activeModule).currentPage"
          :page-size="getModuleState(activeModule).pageSize"
          :page-sizes="[20, 50, 100]"
          :total="getModuleState(activeModule).total"
          layout="total, sizes, prev, pager, next, jumper"
          @current-change="handlePageChange"
          @size-change="handleSizeChange"
        />
      </div>
    </FaCard>

    <FaCard>
      <div class="space-y-2">
        <div class="text-base text-stone-900 font-medium">
          后续规划
        </div>
        <div class="text-sm text-stone-500">
          下一步继续在高级营销里补活动配置、关联商品选择和效果复盘，让这一页从列表查询继续推进到完整管理闭环。
        </div>
      </div>
    </FaCard>

    <el-dialog v-model="detailDialogVisible" :title="detailTitle" width="820px">
      <div v-loading="detailLoading">
        <el-empty v-if="!currentSeckill && !currentGroupBuy && !currentBargain" description="暂无活动数据" />

        <div v-else-if="currentSeckill" class="space-y-4">
          <el-descriptions :column="2" border>
            <el-descriptions-item label="活动名称">
              {{ currentSeckill.name }}
            </el-descriptions-item>
            <el-descriptions-item label="活动编码">
              {{ currentSeckill.code }}
            </el-descriptions-item>
            <el-descriptions-item label="商品名称">
              {{ currentSeckill.productName || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="规格名称">
              {{ currentSeckill.skuName || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="原价">
              {{ formatAmount(currentSeckill.originalPrice) }}
            </el-descriptions-item>
            <el-descriptions-item label="秒杀价">
              {{ formatAmount(currentSeckill.seckillPrice) }}
            </el-descriptions-item>
            <el-descriptions-item label="总库存">
              {{ currentSeckill.totalStock }}
            </el-descriptions-item>
            <el-descriptions-item label="已售库存">
              {{ currentSeckill.soldStock }}
            </el-descriptions-item>
            <el-descriptions-item label="锁定库存">
              {{ currentSeckill.lockedStock }}
            </el-descriptions-item>
            <el-descriptions-item label="每人限购">
              {{ currentSeckill.purchaseLimit }} 件
            </el-descriptions-item>
            <el-descriptions-item label="参与人数">
              {{ currentSeckill.participantCount }}
            </el-descriptions-item>
            <el-descriptions-item label="订单数">
              {{ currentSeckill.orderCount }}
            </el-descriptions-item>
            <el-descriptions-item label="预热开始">
              {{ formatTime(currentSeckill.warmupStartTime) }}
            </el-descriptions-item>
            <el-descriptions-item label="活动状态">
              {{ statusMap[currentSeckill.status]?.label || '未知状态' }}
            </el-descriptions-item>
            <el-descriptions-item label="开始时间">
              {{ formatTime(currentSeckill.startTime) }}
            </el-descriptions-item>
            <el-descriptions-item label="结束时间">
              {{ formatTime(currentSeckill.endTime) }}
            </el-descriptions-item>
          </el-descriptions>

          <FaCard>
            <div class="text-sm text-stone-500 leading-6">
              {{ currentSeckill.description || '暂无活动说明' }}
            </div>
          </FaCard>
        </div>

        <div v-else-if="currentGroupBuy" class="space-y-4">
          <el-descriptions :column="2" border>
            <el-descriptions-item label="活动名称">
              {{ currentGroupBuy.name }}
            </el-descriptions-item>
            <el-descriptions-item label="活动编码">
              {{ currentGroupBuy.code }}
            </el-descriptions-item>
            <el-descriptions-item label="商品名称">
              {{ currentGroupBuy.productName || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="规格名称">
              {{ currentGroupBuy.skuName || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="原价">
              {{ formatAmount(currentGroupBuy.originalPrice) }}
            </el-descriptions-item>
            <el-descriptions-item label="拼团价">
              {{ formatAmount(currentGroupBuy.groupPrice) }}
            </el-descriptions-item>
            <el-descriptions-item label="成团人数">
              {{ currentGroupBuy.groupSize }} 人
            </el-descriptions-item>
            <el-descriptions-item label="每人限购">
              {{ currentGroupBuy.purchaseLimit }} 件
            </el-descriptions-item>
            <el-descriptions-item label="虚拟团数">
              {{ currentGroupBuy.virtualGroupCount }}
            </el-descriptions-item>
            <el-descriptions-item label="开团人数">
              {{ currentGroupBuy.participantCount }}
            </el-descriptions-item>
            <el-descriptions-item label="成团数">
              {{ currentGroupBuy.successGroupCount }}
            </el-descriptions-item>
            <el-descriptions-item label="失败团数">
              {{ currentGroupBuy.failedGroupCount }}
            </el-descriptions-item>
            <el-descriptions-item label="团长奖励">
              {{ formatAmount(currentGroupBuy.groupLeaderReward) }}
            </el-descriptions-item>
            <el-descriptions-item label="活动状态">
              {{ statusMap[currentGroupBuy.status]?.label || '未知状态' }}
            </el-descriptions-item>
            <el-descriptions-item label="开始时间">
              {{ formatTime(currentGroupBuy.startTime) }}
            </el-descriptions-item>
            <el-descriptions-item label="结束时间">
              {{ formatTime(currentGroupBuy.endTime) }}
            </el-descriptions-item>
          </el-descriptions>

          <FaCard>
            <div class="text-sm text-stone-500 leading-6">
              {{ currentGroupBuy.description || '暂无活动说明' }}
            </div>
          </FaCard>
        </div>

        <div v-else-if="currentBargain" class="space-y-4">
          <el-descriptions :column="2" border>
            <el-descriptions-item label="活动名称">
              {{ currentBargain.name }}
            </el-descriptions-item>
            <el-descriptions-item label="活动编码">
              {{ currentBargain.code }}
            </el-descriptions-item>
            <el-descriptions-item label="商品名称">
              {{ currentBargain.productName || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="规格名称">
              {{ currentBargain.skuName || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="原价">
              {{ formatAmount(currentBargain.originalPrice) }}
            </el-descriptions-item>
            <el-descriptions-item label="底价">
              {{ formatAmount(currentBargain.floorPrice) }}
            </el-descriptions-item>
            <el-descriptions-item label="当前最低成交价">
              {{ formatAmount(currentBargain.currentLowestPrice) }}
            </el-descriptions-item>
            <el-descriptions-item label="每人限购">
              {{ currentBargain.purchaseLimit }} 件
            </el-descriptions-item>
            <el-descriptions-item label="发起人数">
              {{ currentBargain.participantCount }}
            </el-descriptions-item>
            <el-descriptions-item label="助力次数">
              {{ currentBargain.helperCount }}
            </el-descriptions-item>
            <el-descriptions-item label="成功砍成">
              {{ currentBargain.successCount }}
            </el-descriptions-item>
            <el-descriptions-item label="活动状态">
              {{ statusMap[currentBargain.status]?.label || '未知状态' }}
            </el-descriptions-item>
            <el-descriptions-item label="开始时间">
              {{ formatTime(currentBargain.startTime) }}
            </el-descriptions-item>
            <el-descriptions-item label="结束时间">
              {{ formatTime(currentBargain.endTime) }}
            </el-descriptions-item>
          </el-descriptions>

          <FaCard>
            <div class="text-sm text-stone-500 leading-6">
              {{ currentBargain.description || '暂无活动说明' }}
            </div>
          </FaCard>
        </div>
      </div>
    </el-dialog>
  </div>
</template>
