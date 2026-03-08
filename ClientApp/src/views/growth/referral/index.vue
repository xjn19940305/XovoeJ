<script setup lang="ts">
import dayjs from 'dayjs'
import { ElMessage, ElMessageBox } from 'element-plus'
import growthApi from '@/api/modules/growth'
import AdminMetricCard from '@/components/admin/AdminMetricCard.vue'

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
    description: '当前页记录的邀请链路总量，用于判断归因沉淀是否稳定。',
    icon: 'i-heroicons-solid:user-plus',
    tone: 'blue' as const,
  },
  {
    title: '已完成绑定',
    value: tableData.value.filter(item => item.status === 1).length,
    description: '已经完成归因绑定的邀请关系，后续可继续观察转化结果。',
    icon: 'i-heroicons-solid:link',
    tone: 'emerald' as const,
  },
  {
    title: '待确认归因',
    value: tableData.value.filter(item => item.status === 0).length,
    description: '仍需继续确认来源和绑定状态的邀请关系数量。',
    icon: 'i-heroicons-solid:clock',
    tone: 'amber' as const,
  },
  {
    title: '累计奖励金额',
    value: formatAmount(tableData.value.reduce((sum, item) => sum + (item.totalRewardAmount || 0), 0)),
    description: '当前列表范围内沉淀的邀请奖励结果，便于联动佣金和分销分析。',
    icon: 'i-heroicons-solid:gift',
    tone: 'sky' as const,
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

async function handleUpdateStatus(row: Api.Growth.ReferralRelation, status: number) {
  const actionText = status === 1 ? '恢复为已绑定' : status === 2 ? '设为失效' : '设为待绑定'
  await ElMessageBox.confirm(`确认将邀请关系“${row.inviterName || '-'} -> ${row.inviteeName || '-'}”${actionText}吗？`, '状态变更', {
    type: 'warning',
  })

  await growthApi.updateDistributionStatus(row.id, { status })
  ElMessage.success('邀请关系状态更新成功')
  await getDistributionList()
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
  return `¥ ${value.toFixed(2)}`
}

onMounted(() => {
  getDistributionList()
})
</script>

<template>
  <div class="growth-referral-page">
    <div class="growth-referral-page__cards">
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
        <el-table-column label="操作" width="240" fixed="right">
          <template #default="{ row }">
            <div class="flex flex-wrap justify-end gap-2">
              <FaButton
                v-if="row.status !== 1"
                size="sm"
                @click="handleUpdateStatus(row, 1)"
              >
                设为已绑定
              </FaButton>
              <FaButton
                v-if="row.status !== 0"
                size="sm"
                variant="outline"
                @click="handleUpdateStatus(row, 0)"
              >
                设为待绑定
              </FaButton>
              <FaButton
                v-if="row.status !== 2"
                size="sm"
                variant="outline"
                @click="handleUpdateStatus(row, 2)"
              >
                失效
              </FaButton>
              <FaButton variant="ghost" size="sm" @click="handleViewDetail(row)">
                <template #icon>
                  <FaIcon name="i-iconoir:eye" />
                </template>
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

<style scoped>
.growth-referral-page__cards {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 16px;
  margin-bottom: 16px;
}

@media (max-width: 1200px) {
  .growth-referral-page__cards {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 768px) {
  .growth-referral-page__cards {
    grid-template-columns: 1fr;
  }
}
</style>
