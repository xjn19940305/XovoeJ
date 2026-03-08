<script setup lang="ts">
import dayjs from 'dayjs'
import messageApi from '@/api/modules/message'

defineOptions({
  name: 'MessageTaskPage',
})

const searchForm = ref({
  keyword: '',
  channel: undefined as string | undefined,
  status: undefined as number | undefined,
})

const tableData = ref<Api.Message.MessageTask[]>([])
const loading = ref(false)
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(20)

const detailDialogVisible = ref(false)
const detailLoading = ref(false)
const currentTask = ref<Api.Message.MessageTask | null>(null)

const channelOptions = [
  { label: '站内信', value: 'inApp' },
  { label: '短信', value: 'sms' },
  { label: '邮件', value: 'email' },
  { label: '推送', value: 'push' },
  { label: '微信', value: 'wechat' },
]

const statusOptions = [
  { label: '待处理', value: 0 },
  { label: '进行中', value: 1 },
  { label: '已成功', value: 2 },
  { label: '失败', value: 3 },
]

const channelMap: Record<string, string> = {
  inApp: '站内信',
  sms: '短信',
  email: '邮件',
  push: '推送',
  wechat: '微信',
}

const triggerTypeMap: Record<string, string> = {
  order_paid: '订单支付',
  order_shipped: '订单发货',
}

const statusMap: Record<number, { label: string, type: 'info' | 'primary' | 'success' | 'danger' }> = {
  0: { label: '待处理', type: 'info' },
  1: { label: '进行中', type: 'primary' },
  2: { label: '已成功', type: 'success' },
  3: { label: '失败', type: 'danger' },
}

const summaryCards = computed(() => [
  {
    title: '任务总数',
    value: total.value,
    icon: 'i-heroicons-solid:paper-airplane',
    tone: 'bg-primary/8 text-primary',
  },
  {
    title: '当前页进行中',
    value: tableData.value.filter(item => item.status === 1).length,
    icon: 'i-heroicons-solid:bolt',
    tone: 'bg-amber-500/10 text-amber-600',
  },
  {
    title: '当前页已成功',
    value: tableData.value.filter(item => item.status === 2).length,
    icon: 'i-heroicons-solid:check-circle',
    tone: 'bg-emerald-500/10 text-emerald-600',
  },
  {
    title: '当前页接收人数',
    value: tableData.value.reduce((sum, item) => sum + item.recipientCount, 0),
    icon: 'i-heroicons-solid:users',
    tone: 'bg-sky-500/10 text-sky-600',
  },
])

async function getTaskList() {
  loading.value = true
  try {
    const res = await messageApi.getTaskList({
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
  getTaskList()
}

function handleReset() {
  searchForm.value = {
    keyword: '',
    channel: undefined,
    status: undefined,
  }
  currentPage.value = 1
  getTaskList()
}

async function handleViewDetail(row: Api.Message.MessageTask) {
  detailDialogVisible.value = true
  detailLoading.value = true
  currentTask.value = row
  try {
    const res = await messageApi.getTaskDetail(row.id)
    currentTask.value = res.data
  }
  finally {
    detailLoading.value = false
  }
}

function handlePageChange(page: number) {
  currentPage.value = page
  getTaskList()
}

function handleSizeChange(size: number) {
  pageSize.value = size
  currentPage.value = 1
  getTaskList()
}

onMounted(() => {
  getTaskList()
})
</script>

<template>
  <div class="message-task-page">
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
          <span>任务筛选</span>
        </div>
      </div>
      <div class="search-body">
        <div class="search-grid">
          <div class="search-field">
            <label class="search-label">关键字</label>
            <el-input
              v-model="searchForm.keyword"
              placeholder="搜索任务名或模板名"
              clearable
            >
              <template #prefix>
                <FaIcon name="i-heroicons-solid:paper-airplane" class="size-4 text-stone-400" />
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
            <span class="font-medium">消息任务</span>
            <span class="ml-2 text-sm text-stone-500">发送队列与重试管理</span>
          </div>
          <FaButton variant="ghost" @click="getTaskList">
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
        <el-table-column prop="name" label="任务名称" min-width="180" show-overflow-tooltip />
        <el-table-column prop="templateName" label="模板名称" min-width="160" show-overflow-tooltip>
          <template #default="{ row }">
            {{ row.templateName || '-' }}
          </template>
        </el-table-column>
        <el-table-column label="渠道" width="110" align="center">
          <template #default="{ row }">
            <el-tag size="small" type="info">
              {{ channelMap[row.channel] || row.channel || '-' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="triggerType" label="触发方式" width="120" align="center">
          <template #default="{ row }">
            {{ triggerTypeMap[row.triggerType || ''] || row.triggerType || '-' }}
          </template>
        </el-table-column>
        <el-table-column label="接收人数" width="110" align="right">
          <template #default="{ row }">
            {{ row.recipientCount }}
          </template>
        </el-table-column>
        <el-table-column label="成功 / 失败" width="140" align="center">
          <template #default="{ row }">
            {{ row.successCount }} / {{ row.failedCount }}
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="statusMap[row.status]?.type || 'info'" size="small">
              {{ statusMap[row.status]?.label || '未知' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="计划发送时间" width="180">
          <template #default="{ row }">
            {{ row.scheduledAt ? dayjs(row.scheduledAt).format('YYYY-MM-DD HH:mm:ss') : '-' }}
          </template>
        </el-table-column>
        <el-table-column label="实际发送时间" width="180">
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
      title="任务详情"
      width="720px"
    >
      <div v-loading="detailLoading">
        <el-empty v-if="!currentTask" description="暂无任务数据" />
        <div v-else class="space-y-4">
          <el-descriptions :column="2" border>
            <el-descriptions-item label="任务名称">
              {{ currentTask.name }}
            </el-descriptions-item>
            <el-descriptions-item label="模板名称">
              {{ currentTask.templateName || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="渠道">
              {{ channelMap[currentTask.channel] || currentTask.channel || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="状态">
              {{ statusMap[currentTask.status]?.label || '未知' }}
            </el-descriptions-item>
            <el-descriptions-item label="触发方式">
              {{ triggerTypeMap[currentTask.triggerType || ''] || currentTask.triggerType || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="接收人数">
              {{ currentTask.recipientCount }}
            </el-descriptions-item>
            <el-descriptions-item label="成功数">
              {{ currentTask.successCount }}
            </el-descriptions-item>
            <el-descriptions-item label="失败数">
              {{ currentTask.failedCount }}
            </el-descriptions-item>
            <el-descriptions-item label="计划发送时间">
              {{ currentTask.scheduledAt ? dayjs(currentTask.scheduledAt).format('YYYY-MM-DD HH:mm:ss') : '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="实际发送时间">
              {{ currentTask.sentAt ? dayjs(currentTask.sentAt).format('YYYY-MM-DD HH:mm:ss') : '-' }}
            </el-descriptions-item>
          </el-descriptions>
        </div>
      </div>
    </el-dialog>
  </div>
</template>
