<script setup lang="ts">
import dayjs from 'dayjs'
import marketingApi from '@/api/modules/marketing'

defineOptions({
  name: 'MarketingPromotionPage',
})

const searchForm = ref({
  keyword: '',
  status: undefined as number | undefined,
  type: undefined as number | undefined,
})

const tableData = ref<Api.Marketing.PromotionActivity[]>([])
const loading = ref(false)
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(20)

const detailDialogVisible = ref(false)
const detailLoading = ref(false)
const currentPromotion = ref<Api.Marketing.PromotionActivity | null>(null)

const promotionTypeOptions = [
  { label: '满减活动', value: 0 },
  { label: '限时折扣', value: 1 },
  { label: '买赠活动', value: 2 },
  { label: '组合套餐', value: 3 },
]

const promotionTypeMap: Record<number, string> = {
  0: '满减活动',
  1: '限时折扣',
  2: '买赠活动',
  3: '组合套餐',
}

const statusOptions = [
  { label: '待开始', value: 0 },
  { label: '进行中', value: 1 },
  { label: '已暂停', value: 2 },
  { label: '已结束', value: 3 },
]

const statusMap: Record<number, { label: string, type: 'danger' | 'info' | 'primary' | 'success' | 'warning' }> = {
  0: { label: '待开始', type: 'info' },
  1: { label: '进行中', type: 'success' },
  2: { label: '已暂停', type: 'warning' },
  3: { label: '已结束', type: 'danger' },
}

const summaryCards = computed(() => [
  {
    title: '活动总数',
    value: total.value,
    icon: 'i-heroicons-solid:megaphone',
    tone: 'bg-primary/8 text-primary',
  },
  {
    title: '当前页进行中',
    value: tableData.value.filter(item => item.status === 1).length,
    icon: 'i-heroicons-solid:bolt',
    tone: 'bg-emerald-500/10 text-emerald-600',
  },
  {
    title: '当前页参与人数',
    value: tableData.value.reduce((sum, item) => sum + item.participantCount, 0),
    icon: 'i-heroicons-solid:users',
    tone: 'bg-amber-500/10 text-amber-600',
  },
  {
    title: '当前页订单数',
    value: tableData.value.reduce((sum, item) => sum + item.orderCount, 0),
    icon: 'i-heroicons-solid:shopping-bag',
    tone: 'bg-sky-500/10 text-sky-600',
  },
])

async function getPromotionList() {
  loading.value = true
  try {
    const params: Api.Marketing.PromotionListParams = {
      page: currentPage.value,
      pageSize: pageSize.value,
      ...searchForm.value,
    }
    const res = await marketingApi.getPromotionList(params)
    tableData.value = res.data.items
    total.value = res.data.total
  }
  finally {
    loading.value = false
  }
}

function handleSearch() {
  currentPage.value = 1
  getPromotionList()
}

function handleReset() {
  searchForm.value = {
    keyword: '',
    status: undefined,
    type: undefined,
  }
  currentPage.value = 1
  getPromotionList()
}

async function handleViewDetail(row: Api.Marketing.PromotionActivity) {
  detailDialogVisible.value = true
  detailLoading.value = true
  currentPromotion.value = row
  try {
    const res = await marketingApi.getPromotionDetail(row.id)
    currentPromotion.value = res.data
  }
  finally {
    detailLoading.value = false
  }
}

function handlePageChange(page: number) {
  currentPage.value = page
  getPromotionList()
}

function handleSizeChange(size: number) {
  pageSize.value = size
  currentPage.value = 1
  getPromotionList()
}

function formatValidity(row: Api.Marketing.PromotionActivity) {
  if (!row.startTime && !row.endTime) {
    return '暂无'
  }
  const start = row.startTime ? dayjs(row.startTime).format('YYYY-MM-DD HH:mm') : '不限开始'
  const end = row.endTime ? dayjs(row.endTime).format('YYYY-MM-DD HH:mm') : '不限结束'
  return `${start} 至 ${end}`
}

onMounted(() => {
  getPromotionList()
})
</script>

<template>
  <div class="marketing-promotion-page">
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
          <span>活动筛选</span>
        </div>
      </div>
      <div class="search-body">
        <div class="search-grid">
          <div class="search-field">
            <label class="search-label">关键字</label>
            <el-input
              v-model="searchForm.keyword"
              placeholder="搜索活动名称"
              clearable
            >
              <template #prefix>
                <FaIcon name="i-heroicons-solid:sparkles" class="size-4 text-stone-400" />
              </template>
            </el-input>
          </div>
          <div class="search-field">
            <label class="search-label">活动类型</label>
            <el-select v-model="searchForm.type" placeholder="全部类型" clearable class="w-full">
              <el-option
                v-for="item in promotionTypeOptions"
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
            <span class="font-medium">核心营销活动</span>
            <span class="ml-2 text-sm text-stone-500">统一价格规则与参与范围</span>
          </div>
          <FaButton variant="ghost" @click="getPromotionList">
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
        <el-table-column prop="name" label="活动名称" min-width="200" show-overflow-tooltip />
        <el-table-column label="活动类型" width="120" align="center">
          <template #default="{ row }">
            <el-tag size="small" type="info">
              {{ promotionTypeMap[row.type] || '未知' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="scopeText" label="适用范围" min-width="160" show-overflow-tooltip>
          <template #default="{ row }">
            {{ row.scopeText || '-' }}
          </template>
        </el-table-column>
        <el-table-column prop="priority" label="优先级" width="90" align="center" />
        <el-table-column label="叠加规则" width="90" align="center">
          <template #default="{ row }">
            <el-tag :type="row.stackable ? 'success' : 'info'" size="small">
              {{ row.stackable ? '可叠加' : '不可叠加' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="参与人数 / 订单数" width="150" align="center">
          <template #default="{ row }">
            {{ row.participantCount }} / {{ row.orderCount }}
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="statusMap[row.status]?.type || 'info'" size="small">
              {{ statusMap[row.status]?.label || '未知' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="有效期" min-width="220">
          <template #default="{ row }">
            {{ formatValidity(row) }}
          </template>
        </el-table-column>
        <el-table-column label="创建时间" width="180">
          <template #default="{ row }">
            {{ row.createdAt ? dayjs(row.createdAt).format('YYYY-MM-DD HH:mm:ss') : '-' }}
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
      title="活动详情"
      width="720px"
    >
      <div v-loading="detailLoading">
        <el-empty v-if="!currentPromotion" description="暂无详情数据" />
        <div v-else class="space-y-4">
          <el-descriptions :column="2" border>
            <el-descriptions-item label="活动名称">
              {{ currentPromotion.name }}
            </el-descriptions-item>
            <el-descriptions-item label="活动类型">
              {{ promotionTypeMap[currentPromotion.type] || '未知' }}
            </el-descriptions-item>
            <el-descriptions-item label="优先级">
              {{ currentPromotion.priority }}
            </el-descriptions-item>
            <el-descriptions-item label="状态">
              {{ statusMap[currentPromotion.status]?.label || '未知' }}
            </el-descriptions-item>
            <el-descriptions-item label="适用范围" :span="2">
              {{ currentPromotion.scopeText || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="参与人数">
              {{ currentPromotion.participantCount }}
            </el-descriptions-item>
            <el-descriptions-item label="订单数">
              {{ currentPromotion.orderCount }}
            </el-descriptions-item>
            <el-descriptions-item label="叠加规则">
              {{ currentPromotion.stackable ? '可与其他规则叠加' : '仅单独生效' }}
            </el-descriptions-item>
            <el-descriptions-item label="有效期">
              {{ formatValidity(currentPromotion) }}
            </el-descriptions-item>
          </el-descriptions>

          <FaCard>
            <p class="mb-2 text-sm text-stone-500">
              活动说明
            </p>
            <p class="text-sm text-stone-700 leading-6">
              {{ currentPromotion.description || '暂无说明' }}
            </p>
          </FaCard>
        </div>
      </div>
    </el-dialog>
  </div>
</template>
