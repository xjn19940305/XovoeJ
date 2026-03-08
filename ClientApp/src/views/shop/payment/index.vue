<script setup lang="ts">
import type { TagProps } from 'element-plus'
import dayjs from 'dayjs'
import { ElMessage, ElMessageBox } from 'element-plus'
import paymentApi from '@/api/modules/payment'
import AdminMetricCard from '@/components/admin/AdminMetricCard.vue'

defineOptions({
  name: 'ShopPaymentList',
})

const loading = ref(false)
const detailLoading = ref(false)
const detailVisible = ref(false)
const tableData = ref<Api.Payment.PaymentOrder[]>([])
const detail = ref<Api.Payment.PaymentOrder | null>(null)
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(20)

const searchForm = reactive({
  keyword: '',
  status: undefined as number | undefined,
})

const statusMap: Record<number, { label: string, type: TagProps['type'] }> = {
  0: { label: '待支付', type: 'warning' },
  1: { label: '已支付', type: 'success' },
  2: { label: '已关闭', type: 'info' },
  3: { label: '部分退款', type: 'warning' },
  4: { label: '已退款', type: 'danger' },
}

const summaryCards = computed(() => [
  {
    title: '支付单总数',
    value: total.value,
    description: '当前查询范围内的支付单总量，用于判断支付链路负载。',
    icon: 'i-heroicons-solid:credit-card',
    tone: 'blue' as const,
  },
  {
    title: '当前页应付金额',
    value: formatAmount(tableData.value.reduce((sum, item) => sum + item.payableAmount, 0)),
    description: '用于快速观察本页支付单的应收规模。',
    icon: 'i-heroicons-solid:banknotes',
    tone: 'amber' as const,
  },
  {
    title: '当前页已支付',
    value: tableData.value.filter(item => item.status === 1).length,
    description: '反映当前页支付成功的支付单数量。',
    icon: 'i-heroicons-solid:check-badge',
    tone: 'emerald' as const,
  },
  {
    title: '当前页退款金额',
    value: formatAmount(tableData.value.reduce((sum, item) => sum + item.refundedAmount, 0)),
    description: '用于快速识别退款规模与支付回退压力。',
    icon: 'i-heroicons-solid:arrow-uturn-left',
    tone: 'rose' as const,
  },
])

async function getList() {
  loading.value = true
  try {
    const res = await paymentApi.getList({
      page: currentPage.value,
      pageSize: pageSize.value,
      keyword: searchForm.keyword || undefined,
      status: searchForm.status,
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
  currentPage.value = 1
  getList()
}

async function handleDetail(row: Api.Payment.PaymentOrder) {
  detailVisible.value = true
  detailLoading.value = true
  try {
    const res = await paymentApi.getDetail(row.paymentOrderNo)
    detail.value = res.data
  }
  finally {
    detailLoading.value = false
  }
}

async function handleClose(row: Api.Payment.PaymentOrder) {
  await ElMessageBox.confirm(`确定关闭支付单 ${row.paymentOrderNo} 吗？`, '关闭支付单', {
    type: 'warning',
  })
  await paymentApi.close(row.paymentOrderNo)
  ElMessage.success('支付单关闭成功')
  getList()
}

function formatAmount(value: number) {
  return `¥ ${value.toFixed(2)}`
}

function formatDateTime(value?: string) {
  return value ? dayjs(value).format('YYYY-MM-DD HH:mm:ss') : '-'
}

onMounted(() => {
  getList()
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
          <FaIcon name="i-heroicons-solid:credit-card" class="size-5" />
          <span>支付单筛选</span>
        </div>
      </div>
      <div class="search-body">
        <div class="search-grid">
          <div class="search-field">
            <label class="search-label">关键词</label>
            <el-input
              v-model="searchForm.keyword"
              placeholder="搜索支付单号、订单号、用户"
              clearable
              @keyup.enter="handleSearch"
            />
          </div>
          <div class="search-field">
            <label class="search-label">状态</label>
            <el-select v-model="searchForm.status" placeholder="全部状态" clearable class="w-full">
              <el-option v-for="(item, key) in statusMap" :key="key" :label="item.label" :value="Number(key)" />
            </el-select>
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
            <span class="font-medium">支付单</span>
            <span class="ml-2 text-sm text-stone-500">订单支付与退款追踪</span>
          </div>
        </div>
      </template>

      <el-table v-loading="loading" :data="tableData">
        <el-table-column prop="paymentOrderNo" label="支付单号" min-width="190" />
        <el-table-column prop="orderNo" label="订单号" min-width="190" />
        <el-table-column prop="userName" label="用户" width="140" />
        <el-table-column label="状态" width="110" align="center">
          <template #default="{ row }">
            <el-tag :type="statusMap[row.status]?.type">
              {{ statusMap[row.status]?.label || row.statusText }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="应付金额" width="110" align="right">
          <template #default="{ row }">{{ formatAmount(row.payableAmount) }}</template>
        </el-table-column>
        <el-table-column label="实付金额" width="110" align="right">
          <template #default="{ row }">{{ formatAmount(row.paidAmount) }}</template>
        </el-table-column>
        <el-table-column label="退款金额" width="110" align="right">
          <template #default="{ row }">{{ formatAmount(row.refundedAmount) }}</template>
        </el-table-column>
        <el-table-column prop="paymentMethod" label="支付方式" width="100" />
        <el-table-column label="创建时间" min-width="170">
          <template #default="{ row }">{{ formatDateTime(row.createdAt) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="180" fixed="right">
          <template #default="{ row }">
            <div class="flex gap-2">
              <FaButton variant="ghost" size="sm" @click="handleDetail(row)">详情</FaButton>
              <FaButton v-if="row.status === 0" variant="ghost" size="sm" class="text-red-500" @click="handleClose(row)">
                关闭
              </FaButton>
            </div>
          </template>
        </el-table-column>
      </el-table>

      <div class="mt-4 flex justify-end">
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :page-sizes="[20, 50, 100]"
          :total="total"
          layout="total, sizes, prev, pager, next, jumper"
          @current-change="getList"
          @size-change="handleSearch"
        />
      </div>
    </FaCard>

    <el-dialog v-model="detailVisible" title="支付单详情" width="720px">
      <div v-loading="detailLoading">
        <div v-if="detail" class="grid gap-3 rounded-lg border p-4 text-sm md:grid-cols-2">
          <div><span class="text-gray-500">支付单号：</span>{{ detail.paymentOrderNo }}</div>
          <div><span class="text-gray-500">订单号：</span>{{ detail.orderNo }}</div>
          <div><span class="text-gray-500">用户：</span>{{ detail.userName || detail.userId }}</div>
          <div><span class="text-gray-500">支付方式：</span>{{ detail.paymentMethod || '-' }}</div>
          <div><span class="text-gray-500">应付金额：</span>{{ formatAmount(detail.payableAmount) }}</div>
          <div><span class="text-gray-500">实付金额：</span>{{ formatAmount(detail.paidAmount) }}</div>
          <div><span class="text-gray-500">退款金额：</span>{{ formatAmount(detail.refundedAmount) }}</div>
          <div><span class="text-gray-500">状态：</span>{{ statusMap[detail.status]?.label || detail.statusText }}</div>
          <div><span class="text-gray-500">创建时间：</span>{{ formatDateTime(detail.createdAt) }}</div>
          <div><span class="text-gray-500">过期时间：</span>{{ formatDateTime(detail.expireAt) }}</div>
          <div><span class="text-gray-500">支付时间：</span>{{ formatDateTime(detail.paidAt) }}</div>
          <div><span class="text-gray-500">关闭时间：</span>{{ formatDateTime(detail.closedAt) }}</div>
          <div class="md:col-span-2"><span class="text-gray-500">失败原因：</span>{{ detail.failureReason || '-' }}</div>
        </div>
      </div>
    </el-dialog>
  </div>
</template>
