<script setup lang="ts">
import dayjs from 'dayjs'
import messageApi from '@/api/modules/message'

defineOptions({
  name: 'MessageRecordPage',
})

const searchForm = ref({
  keyword: '',
  channel: undefined as string | undefined,
  status: undefined as number | undefined,
})

const tableData = ref<Api.Message.MessageRecord[]>([])
const loading = ref(false)
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(20)

const detailDialogVisible = ref(false)
const detailLoading = ref(false)
const currentRecord = ref<Api.Message.MessageRecord | null>(null)

const channelOptions = [
  { label: '站内信', value: 'inApp' },
  { label: '短信', value: 'sms' },
  { label: '邮件', value: 'email' },
  { label: '推送', value: 'push' },
  { label: '微信', value: 'wechat' },
]

const statusOptions = [
  { label: '待发送', value: 0 },
  { label: '已送达', value: 1 },
  { label: '失败', value: 2 },
]

const channelMap: Record<string, string> = {
  inApp: '站内信',
  sms: '短信',
  email: '邮件',
  push: '推送',
  wechat: '微信',
}

const businessTypeMap: Record<string, string> = {
  order: '订单',
  shipping: '发货',
  marketing: '营销',
}

const statusMap: Record<number, { label: string, type: 'info' | 'success' | 'danger' }> = {
  0: { label: '待发送', type: 'info' },
  1: { label: '已送达', type: 'success' },
  2: { label: '失败', type: 'danger' },
}

const summaryCards = computed(() => [
  {
    title: '记录总数',
    value: total.value,
    icon: 'i-heroicons-solid:clipboard-document-list',
    tone: 'bg-primary/8 text-primary',
  },
  {
    title: '当前页已送达',
    value: tableData.value.filter(item => item.status === 1).length,
    icon: 'i-heroicons-solid:check-circle',
    tone: 'bg-emerald-500/10 text-emerald-600',
  },
  {
    title: '当前页失败',
    value: tableData.value.filter(item => item.status === 2).length,
    icon: 'i-heroicons-solid:exclamation-triangle',
    tone: 'bg-rose-500/10 text-rose-600',
  },
  {
    title: '当前页邮件记录',
    value: tableData.value.filter(item => item.channel === 'email').length,
    icon: 'i-heroicons-solid:at-symbol',
    tone: 'bg-sky-500/10 text-sky-600',
  },
])

async function getRecordList() {
  loading.value = true
  try {
    const res = await messageApi.getRecordList({
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
  getRecordList()
}

function handleReset() {
  searchForm.value = {
    keyword: '',
    channel: undefined,
    status: undefined,
  }
  currentPage.value = 1
  getRecordList()
}

async function handleViewDetail(row: Api.Message.MessageRecord) {
  detailDialogVisible.value = true
  detailLoading.value = true
  currentRecord.value = row
  try {
    const res = await messageApi.getRecordDetail(row.id)
    currentRecord.value = res.data
  }
  finally {
    detailLoading.value = false
  }
}

function handlePageChange(page: number) {
  currentPage.value = page
  getRecordList()
}

function handleSizeChange(size: number) {
  pageSize.value = size
  currentPage.value = 1
  getRecordList()
}

onMounted(() => {
  getRecordList()
})
</script>

<template>
  <div class="message-record-page">
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
          <span>记录筛选</span>
        </div>
      </div>
      <div class="search-body">
        <div class="search-grid">
          <div class="search-field">
            <label class="search-label">关键字</label>
            <el-input
              v-model="searchForm.keyword"
              placeholder="搜索接收人、模板或追踪号"
              clearable
            >
              <template #prefix>
                <FaIcon name="i-heroicons-solid:clipboard-document-list" class="size-4 text-stone-400" />
              </template>
            </el-input>
          </div>
          <div class="search-field">
            <label class="search-label">渠道</label>
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
            <span class="font-medium">发送记录</span>
            <span class="ml-2 text-sm text-stone-500">审计轨迹与送达结果</span>
          </div>
          <FaButton variant="ghost" @click="getRecordList">
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
        <el-table-column prop="templateName" label="模板名称" min-width="160" show-overflow-tooltip>
          <template #default="{ row }">
            {{ row.templateName || '-' }}
          </template>
        </el-table-column>
        <el-table-column prop="taskName" label="任务名称" min-width="160" show-overflow-tooltip>
          <template #default="{ row }">
            {{ row.taskName || '-' }}
          </template>
        </el-table-column>
        <el-table-column label="渠道" width="110" align="center">
          <template #default="{ row }">
            <el-tag size="small" type="info">
              {{ channelMap[row.channel] || row.channel || '-' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="recipient" label="接收人" min-width="180" show-overflow-tooltip>
          <template #default="{ row }">
            {{ row.recipient || '-' }}
          </template>
        </el-table-column>
        <el-table-column prop="businessType" label="业务类型" min-width="140" show-overflow-tooltip>
          <template #default="{ row }">
            {{ businessTypeMap[row.businessType || ''] || row.businessType || '-' }}
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="statusMap[row.status]?.type || 'info'" size="small">
              {{ statusMap[row.status]?.label || '未知' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="traceId" label="追踪号" min-width="180" show-overflow-tooltip>
          <template #default="{ row }">
            {{ row.traceId || '-' }}
          </template>
        </el-table-column>
        <el-table-column label="发送时间" width="180">
          <template #default="{ row }">
            {{ row.sentAt ? dayjs(row.sentAt).format('YYYY-MM-DD HH:mm:ss') : '-' }}
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
      title="记录详情"
      width="760px"
    >
      <div v-loading="detailLoading">
        <el-empty v-if="!currentRecord" description="暂无记录数据" />
        <div v-else class="space-y-4">
          <el-descriptions :column="2" border>
            <el-descriptions-item label="模板名称">
              {{ currentRecord.templateName || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="任务名称">
              {{ currentRecord.taskName || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="渠道">
              {{ channelMap[currentRecord.channel] || currentRecord.channel || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="状态">
              {{ statusMap[currentRecord.status]?.label || '未知' }}
            </el-descriptions-item>
            <el-descriptions-item label="接收人">
              {{ currentRecord.recipient || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="业务类型">
              {{ businessTypeMap[currentRecord.businessType || ''] || currentRecord.businessType || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="追踪号" :span="2">
              {{ currentRecord.traceId || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="发送时间">
              {{ currentRecord.sentAt ? dayjs(currentRecord.sentAt).format('YYYY-MM-DD HH:mm:ss') : '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="创建时间">
              {{ currentRecord.createdAt ? dayjs(currentRecord.createdAt).format('YYYY-MM-DD HH:mm:ss') : '-' }}
            </el-descriptions-item>
          </el-descriptions>

          <FaCard>
            <p class="mb-2 text-sm text-stone-500">
              错误信息
            </p>
            <p class="whitespace-pre-wrap text-sm text-stone-700 leading-6">
              {{ currentRecord.errorMessage || '暂无错误信息' }}
            </p>
          </FaCard>
        </div>
      </div>
    </el-dialog>
  </div>
</template>
