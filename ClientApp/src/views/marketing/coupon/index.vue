<script setup lang="ts">
import dayjs from 'dayjs'
import marketingApi from '@/api/modules/marketing'

defineOptions({
  name: 'MarketingCouponPage',
})

const searchForm = ref({
  keyword: '',
  status: undefined as number | undefined,
  couponType: undefined as number | undefined,
})

const tableData = ref<Api.Marketing.CouponTemplate[]>([])
const loading = ref(false)
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(20)

const detailDialogVisible = ref(false)
const detailLoading = ref(false)
const currentCoupon = ref<Api.Marketing.CouponTemplate | null>(null)

const couponTypeOptions = [
  { label: '满减券', value: 0 },
  { label: '折扣券', value: 1 },
  { label: '无门槛券', value: 2 },
]

const statusOptions = [
  { label: '草稿', value: 0 },
  { label: '进行中', value: 1 },
  { label: '已暂停', value: 2 },
  { label: '已结束', value: 3 },
]

const couponTypeMap: Record<number, string> = {
  0: '满减券',
  1: '折扣券',
  2: '无门槛券',
}

const statusMap: Record<number, { label: string, type: 'danger' | 'info' | 'primary' | 'success' | 'warning' }> = {
  0: { label: '草稿', type: 'info' },
  1: { label: '进行中', type: 'success' },
  2: { label: '已暂停', type: 'warning' },
  3: { label: '已结束', type: 'danger' },
}

const summaryCards = computed(() => [
  {
    title: '模板总数',
    value: total.value,
    icon: 'i-heroicons-solid:ticket',
    tone: 'bg-primary/8 text-primary',
  },
  {
    title: '当前页进行中',
    value: tableData.value.filter(item => item.status === 1).length,
    icon: 'i-heroicons-solid:play-circle',
    tone: 'bg-emerald-500/10 text-emerald-600',
  },
  {
    title: '当前页已发放',
    value: tableData.value.reduce((sum, item) => sum + item.issuedQuantity, 0),
    icon: 'i-heroicons-solid:paper-airplane',
    tone: 'bg-amber-500/10 text-amber-600',
  },
  {
    title: '当前页已使用',
    value: tableData.value.reduce((sum, item) => sum + item.usedQuantity, 0),
    icon: 'i-heroicons-solid:check-badge',
    tone: 'bg-sky-500/10 text-sky-600',
  },
])

async function getCouponList() {
  loading.value = true
  try {
    const params: Api.Marketing.CouponListParams = {
      page: currentPage.value,
      pageSize: pageSize.value,
      ...searchForm.value,
    }
    const res = await marketingApi.getCouponList(params)
    tableData.value = res.data.items
    total.value = res.data.total
  }
  finally {
    loading.value = false
  }
}

function handleSearch() {
  currentPage.value = 1
  getCouponList()
}

function handleReset() {
  searchForm.value = {
    keyword: '',
    status: undefined,
    couponType: undefined,
  }
  currentPage.value = 1
  getCouponList()
}

async function handleViewDetail(row: Api.Marketing.CouponTemplate) {
  detailDialogVisible.value = true
  detailLoading.value = true
  currentCoupon.value = row
  try {
    const res = await marketingApi.getCouponDetail(row.id)
    currentCoupon.value = res.data
  }
  finally {
    detailLoading.value = false
  }
}

function handlePageChange(page: number) {
  currentPage.value = page
  getCouponList()
}

function handleSizeChange(size: number) {
  pageSize.value = size
  currentPage.value = 1
  getCouponList()
}

function formatDiscount(row: Api.Marketing.CouponTemplate) {
  if (row.couponType === 1) {
    return `${row.discountValue} 折`
  }
  if (row.discountType === 1) {
    return `减 ￥${row.discountValue.toFixed(2)}`
  }
  return `￥${row.discountValue.toFixed(2)}`
}

function formatValidity(row: Api.Marketing.CouponTemplate) {
  if (!row.startTime && !row.endTime) {
    return '不限开始 至 不限结束'
  }
  const start = row.startTime ? dayjs(row.startTime).format('YYYY-MM-DD HH:mm') : '不限开始'
  const end = row.endTime ? dayjs(row.endTime).format('YYYY-MM-DD HH:mm') : '不限结束'
  return `${start} 至 ${end}`
}

onMounted(() => {
  getCouponList()
})
</script>

<template>
  <div class="marketing-coupon-page">
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
          <span>优惠券筛选</span>
        </div>
      </div>
      <div class="search-body">
        <div class="search-grid">
          <div class="search-field">
            <label class="search-label">关键字</label>
            <el-input
              v-model="searchForm.keyword"
              placeholder="搜索模板名称或编码"
              clearable
            >
              <template #prefix>
                <FaIcon name="i-heroicons-solid:ticket" class="size-4 text-stone-400" />
              </template>
            </el-input>
          </div>
          <div class="search-field">
            <label class="search-label">类型</label>
            <el-select v-model="searchForm.couponType" placeholder="全部类型" clearable class="w-full">
              <el-option
                v-for="item in couponTypeOptions"
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
            <span class="font-medium">优惠券模板</span>
            <span class="ml-2 text-sm text-stone-500">第 3 阶段核心营销</span>
          </div>
          <FaButton variant="ghost" @click="getCouponList">
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
        <el-table-column prop="name" label="模板名称" min-width="180" show-overflow-tooltip />
        <el-table-column prop="code" label="模板编码" min-width="160" show-overflow-tooltip />
        <el-table-column label="类型" width="110" align="center">
          <template #default="{ row }">
            <el-tag size="small" type="info">
              {{ couponTypeMap[row.couponType] || '未知' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="优惠力度" width="120" align="right">
          <template #default="{ row }">
            {{ formatDiscount(row) }}
          </template>
        </el-table-column>
        <el-table-column label="使用门槛" width="120" align="right">
          <template #default="{ row }">
            ￥{{ row.minOrderAmount.toFixed(2) }}
          </template>
        </el-table-column>
        <el-table-column label="发放 / 使用" width="140" align="center">
          <template #default="{ row }">
            {{ row.issuedQuantity }} / {{ row.usedQuantity }}
          </template>
        </el-table-column>
        <el-table-column label="有效期" min-width="220">
          <template #default="{ row }">
            {{ formatValidity(row) }}
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="statusMap[row.status]?.type || 'info'" size="small">
              {{ statusMap[row.status]?.label || '未知' }}
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
      title="优惠券详情"
      width="720px"
    >
      <div v-loading="detailLoading">
        <el-empty v-if="!currentCoupon" description="暂无详情数据" />
        <div v-else class="space-y-4">
          <el-descriptions :column="2" border>
            <el-descriptions-item label="模板名称">
              {{ currentCoupon.name }}
            </el-descriptions-item>
            <el-descriptions-item label="模板编码">
              {{ currentCoupon.code }}
            </el-descriptions-item>
            <el-descriptions-item label="类型">
              {{ couponTypeMap[currentCoupon.couponType] || '未知' }}
            </el-descriptions-item>
            <el-descriptions-item label="状态">
              {{ statusMap[currentCoupon.status]?.label || '未知' }}
            </el-descriptions-item>
            <el-descriptions-item label="优惠力度">
              {{ formatDiscount(currentCoupon) }}
            </el-descriptions-item>
            <el-descriptions-item label="使用门槛">
              ￥{{ currentCoupon.minOrderAmount.toFixed(2) }}
            </el-descriptions-item>
            <el-descriptions-item label="总量">
              {{ currentCoupon.totalQuantity }}
            </el-descriptions-item>
            <el-descriptions-item label="每人限领">
              {{ currentCoupon.receiveLimit ?? '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="已发放">
              {{ currentCoupon.issuedQuantity }}
            </el-descriptions-item>
            <el-descriptions-item label="已使用">
              {{ currentCoupon.usedQuantity }}
            </el-descriptions-item>
            <el-descriptions-item label="有效期" :span="2">
              {{ formatValidity(currentCoupon) }}
            </el-descriptions-item>
          </el-descriptions>

          <FaCard>
            <p class="mb-2 text-sm text-stone-500">
              说明
            </p>
            <p class="text-sm text-stone-700 leading-6">
              {{ currentCoupon.description || '暂无说明' }}
            </p>
          </FaCard>
        </div>
      </div>
    </el-dialog>
  </div>
</template>
