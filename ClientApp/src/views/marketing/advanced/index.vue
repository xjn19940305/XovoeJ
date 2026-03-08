<script setup lang="ts">
import type { TagProps } from 'element-plus'
import dayjs from 'dayjs'
import { ElMessage, ElMessageBox } from 'element-plus'
import marketingApi from '@/api/modules/marketing'
import AdminMetricCard from '@/components/admin/AdminMetricCard.vue'
import AdminPageHero from '@/components/admin/AdminPageHero.vue'

defineOptions({ name: 'MarketingAdvancedPage' })

type AdvancedModule = 'seckill' | 'groupBuy' | 'bargain'

const activeModule = ref<AdvancedModule>('seckill')
const searchForm = reactive({ keyword: '', status: undefined as number | undefined })
const pageState = reactive({
  seckill: { loading: false, items: [] as Api.Marketing.SeckillActivity[], total: 0, currentPage: 1, pageSize: 20 },
  groupBuy: { loading: false, items: [] as Api.Marketing.GroupBuyActivity[], total: 0, currentPage: 1, pageSize: 20 },
  bargain: { loading: false, items: [] as Api.Marketing.BargainActivity[], total: 0, currentPage: 1, pageSize: 20 },
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
const moduleTabs = [
  { key: 'seckill', label: '秒杀活动', hint: '库存、价格与订单效率' },
  { key: 'groupBuy', label: '拼团活动', hint: '开团、成团与奖励结果' },
  { key: 'bargain', label: '砍价活动', hint: '发起、助力与砍成数量' },
] satisfies Array<{ key: AdvancedModule, label: string, hint: string }>

const currentCards = computed(() => {
  if (activeModule.value === 'seckill') {
    const items = pageState.seckill.items
    return [
      { title: '秒杀活动', value: pageState.seckill.total, description: '当前模块活动总数', icon: 'i-heroicons-solid:bolt', tone: 'rose' as const },
      { title: '进行中', value: items.filter(item => item.status === 1).length, description: '正在承接流量的秒杀活动', icon: 'i-heroicons-solid:fire', tone: 'emerald' as const },
      { title: '已售库存', value: items.reduce((sum, item) => sum + item.soldStock, 0), description: '观察活动热度与库存消耗', icon: 'i-heroicons-solid:cube', tone: 'sky' as const },
      { title: '当前页成交额', value: formatAmount(items.reduce((sum, item) => sum + item.seckillPrice * item.orderCount, 0)), description: '按当前页活动估算', icon: 'i-heroicons-solid:banknotes', tone: 'violet' as const },
    ]
  }
  if (activeModule.value === 'groupBuy') {
    const items = pageState.groupBuy.items
    return [
      { title: '拼团活动', value: pageState.groupBuy.total, description: '当前模块活动总数', icon: 'i-heroicons-solid:users', tone: 'blue' as const },
      { title: '成功成团', value: items.reduce((sum, item) => sum + item.successGroupCount, 0), description: '拼团最终转化结果', icon: 'i-heroicons-solid:user-group', tone: 'emerald' as const },
      { title: '开团参与', value: items.reduce((sum, item) => sum + item.participantCount, 0), description: '当前页参与人次汇总', icon: 'i-heroicons-solid:user-plus', tone: 'sky' as const },
      { title: '团长奖励', value: formatAmount(items.reduce((sum, item) => sum + item.groupLeaderReward * item.successGroupCount, 0)), description: '观察激励强度是否合理', icon: 'i-heroicons-solid:gift-top', tone: 'amber' as const },
    ]
  }
  const items = pageState.bargain.items
  return [
    { title: '砍价活动', value: pageState.bargain.total, description: '当前模块活动总数', icon: 'i-heroicons-solid:scissors', tone: 'amber' as const },
    { title: '发起人数', value: items.reduce((sum, item) => sum + item.participantCount, 0), description: '当前发起热度', icon: 'i-heroicons-solid:user-circle', tone: 'sky' as const },
    { title: '助力次数', value: items.reduce((sum, item) => sum + item.helperCount, 0), description: '传播与分享活跃度', icon: 'i-heroicons-solid:hand-raised', tone: 'emerald' as const },
    { title: '砍成数量', value: items.reduce((sum, item) => sum + item.successCount, 0), description: '最终转化情况', icon: 'i-heroicons-solid:trophy', tone: 'rose' as const },
  ]
})

const detailTitle = computed(() => currentSeckill.value ? '秒杀活动详情' : currentGroupBuy.value ? '拼团活动详情' : '砍价活动详情')

function getModuleState(module: AdvancedModule) {
  return module === 'seckill' ? pageState.seckill : module === 'groupBuy' ? pageState.groupBuy : pageState.bargain
}

async function getSeckillList() {
  pageState.seckill.loading = true
  try {
    const res = await marketingApi.getSeckillList({ page: pageState.seckill.currentPage, pageSize: pageState.seckill.pageSize, keyword: searchForm.keyword || undefined, status: searchForm.status })
    pageState.seckill.items = res.data.items
    pageState.seckill.total = res.data.total
  }
  finally { pageState.seckill.loading = false }
}

async function getGroupBuyList() {
  pageState.groupBuy.loading = true
  try {
    const res = await marketingApi.getGroupBuyList({ page: pageState.groupBuy.currentPage, pageSize: pageState.groupBuy.pageSize, keyword: searchForm.keyword || undefined, status: searchForm.status })
    pageState.groupBuy.items = res.data.items
    pageState.groupBuy.total = res.data.total
  }
  finally { pageState.groupBuy.loading = false }
}

async function getBargainList() {
  pageState.bargain.loading = true
  try {
    const res = await marketingApi.getBargainList({ page: pageState.bargain.currentPage, pageSize: pageState.bargain.pageSize, keyword: searchForm.keyword || undefined, status: searchForm.status })
    pageState.bargain.items = res.data.items
    pageState.bargain.total = res.data.total
  }
  finally { pageState.bargain.loading = false }
}

function getListByModule(module = activeModule.value) {
  return module === 'seckill' ? getSeckillList() : module === 'groupBuy' ? getGroupBuyList() : getBargainList()
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
  try { currentSeckill.value = (await marketingApi.getSeckillDetail(row.id)).data } finally { detailLoading.value = false }
}

async function handleViewGroupBuyDetail(row: Api.Marketing.GroupBuyActivity) {
  detailDialogVisible.value = true
  detailLoading.value = true
  resetDetailState()
  currentGroupBuy.value = row
  try { currentGroupBuy.value = (await marketingApi.getGroupBuyDetail(row.id)).data } finally { detailLoading.value = false }
}

async function handleViewBargainDetail(row: Api.Marketing.BargainActivity) {
  detailDialogVisible.value = true
  detailLoading.value = true
  resetDetailState()
  currentBargain.value = row
  try { currentBargain.value = (await marketingApi.getBargainDetail(row.id)).data } finally { detailLoading.value = false }
}

async function handleStatusChange(kind: AdvancedModule, id: string, name: string, status: number) {
  const actionText = status === 1 ? '开始' : status === 0 ? '切回预热' : '结束'
  await ElMessageBox.confirm(`确认${actionText}${moduleLabelMap[kind]}“${name}”吗？`, '活动状态变更', { type: 'warning' })
  if (kind === 'seckill') await marketingApi.updateSeckillStatus(id, { status })
  else if (kind === 'groupBuy') await marketingApi.updateGroupBuyStatus(id, { status })
  else await marketingApi.updateBargainStatus(id, { status })
  ElMessage.success(`${moduleLabelMap[kind]}状态更新成功`)
  await getListByModule(kind)
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
  if (!getModuleState(module).items.length) getListByModule(module)
}

function formatAmount(value: number) { return `¥ ${value.toFixed(2)}` }
function formatTime(value?: string) { return value ? dayjs(value).format('YYYY-MM-DD HH:mm:ss') : '-' }
function formatProgress(soldStock: number, totalStock: number) { return !totalStock ? '0%' : `${Math.min(100, Math.round((soldStock / totalStock) * 100))}%` }

onMounted(() => { getSeckillList() })
</script>

<template>
  <div class="admin-page-shell marketing-advanced-page">
    <AdminPageHero eyebrow="高级营销" :title="moduleLabelMap[activeModule]" :description="`统一管理${moduleLabelMap[activeModule]}，页面重点展示经营结果、活动状态和可执行操作。`">
      <template #actions>
        <FaButton variant="ghost" @click="getListByModule()">
          <template #icon><FaIcon name="i-heroicons-solid:arrow-path" /></template>
          刷新当前模块
        </FaButton>
      </template>
      <div class="module-switcher">
        <button v-for="item in moduleTabs" :key="item.key" type="button" class="module-switcher__item" :class="{ 'is-active': activeModule === item.key }" @click="switchModule(item.key)">
          <span>{{ item.label }}</span>
          <small>{{ item.hint }}</small>
        </button>
      </div>
    </AdminPageHero>

    <div class="admin-overview-grid">
      <AdminMetricCard v-for="card in currentCards" :key="card.title" :title="card.title" :value="card.value" :description="card.description" :icon="card.icon" :tone="card.tone" variant="board" />
    </div>

    <FaCard class="search-card">
      <div class="search-header"><div class="search-title"><FaIcon name="i-heroicons-solid:magnifying-glass" class="size-5" /><span>{{ moduleLabelMap[activeModule] }}筛选</span></div></div>
      <div class="search-body">
        <div class="search-grid">
          <div class="search-field">
            <label class="search-label">关键字</label>
            <el-input v-model="searchForm.keyword" :placeholder="`搜索${moduleLabelMap[activeModule]}名称、编码、商品名或规格`" clearable @keyup.enter="handleSearch" />
          </div>
          <div class="search-field">
            <label class="search-label">活动状态</label>
            <el-select v-model="searchForm.status" placeholder="全部状态" clearable class="w-full">
              <el-option label="预热中" :value="0" />
              <el-option label="进行中" :value="1" />
              <el-option label="已结束" :value="2" />
            </el-select>
          </div>
        </div>
      </div>
      <div class="search-footer">
        <FaButton @click="handleSearch"><template #icon><FaIcon name="i-heroicons-solid:magnifying-glass" /></template>查询</FaButton>
        <FaButton class="search-reset-btn" @click="handleReset"><template #icon><FaIcon name="i-heroicons-solid:arrow-path" /></template>重置</FaButton>
      </div>
    </FaCard>

    <FaCard class="admin-table-card">
      <template #header>
        <div class="admin-section-header">
          <div class="admin-section-header__meta">
            <span class="admin-section-header__title">{{ moduleLabelMap[activeModule] }}列表</span>
            <span class="admin-section-header__description">统一查看活动信息、经营结果和状态切换。</span>
          </div>
          <FaButton variant="ghost" @click="getListByModule()"><template #icon><FaIcon name="i-heroicons-solid:arrow-path" /></template>刷新</FaButton>
        </div>
      </template>

      <el-table v-if="activeModule === 'seckill'" v-loading="pageState.seckill.loading" :data="pageState.seckill.items">
        <el-table-column label="活动信息" min-width="220"><template #default="{ row }"><div class="space-y-1"><div class="text-sm text-stone-900 font-medium">{{ row.name }}</div><div class="text-xs text-stone-500">{{ row.code }}</div></div></template></el-table-column>
        <el-table-column label="秒杀商品" min-width="180" show-overflow-tooltip><template #default="{ row }"><div class="space-y-1"><div>{{ row.productName || '-' }}</div><div class="text-xs text-stone-500">{{ row.skuName || '-' }}</div></div></template></el-table-column>
        <el-table-column label="价格" width="130" align="right"><template #default="{ row }"><div class="text-rose-600 font-medium">{{ formatAmount(row.seckillPrice) }}</div><div class="text-xs text-stone-400 line-through">{{ formatAmount(row.originalPrice) }}</div></template></el-table-column>
        <el-table-column label="库存进度" width="120" align="center"><template #default="{ row }"><div>{{ row.soldStock }}/{{ row.totalStock }}</div><div class="text-xs text-stone-500">{{ formatProgress(row.soldStock, row.totalStock) }}</div></template></el-table-column>
        <el-table-column label="参与/订单" width="110" align="center"><template #default="{ row }">{{ row.participantCount }}/{{ row.orderCount }}</template></el-table-column>
        <el-table-column label="状态" width="100" align="center"><template #default="{ row }"><el-tag :type="statusMap[row.status]?.type || 'info'" size="small">{{ statusMap[row.status]?.label || '未知状态' }}</el-tag></template></el-table-column>
        <el-table-column label="活动时间" min-width="180"><template #default="{ row }"><div>{{ formatTime(row.startTime) }}</div><div class="text-xs text-stone-500">至 {{ formatTime(row.endTime) }}</div></template></el-table-column>
        <el-table-column label="操作" width="220" fixed="right"><template #default="{ row }"><div class="flex flex-wrap justify-end gap-2"><FaButton v-if="row.status !== 1" size="sm" @click="handleStatusChange('seckill', row.id, row.name, 1)">开始</FaButton><FaButton v-if="row.status !== 0" size="sm" variant="outline" @click="handleStatusChange('seckill', row.id, row.name, 0)">预热</FaButton><FaButton v-if="row.status !== 2" size="sm" variant="outline" @click="handleStatusChange('seckill', row.id, row.name, 2)">结束</FaButton><FaButton variant="ghost" size="sm" @click="handleViewSeckillDetail(row)">详情</FaButton></div></template></el-table-column>
      </el-table>

      <el-table v-else-if="activeModule === 'groupBuy'" v-loading="pageState.groupBuy.loading" :data="pageState.groupBuy.items">
        <el-table-column label="活动信息" min-width="220"><template #default="{ row }"><div class="space-y-1"><div class="text-sm text-stone-900 font-medium">{{ row.name }}</div><div class="text-xs text-stone-500">{{ row.code }}</div></div></template></el-table-column>
        <el-table-column label="拼团商品" min-width="180" show-overflow-tooltip><template #default="{ row }"><div class="space-y-1"><div>{{ row.productName || '-' }}</div><div class="text-xs text-stone-500">{{ row.skuName || '-' }}</div></div></template></el-table-column>
        <el-table-column label="拼团价" width="130" align="right"><template #default="{ row }"><div class="text-primary font-medium">{{ formatAmount(row.groupPrice) }}</div><div class="text-xs text-stone-400 line-through">{{ formatAmount(row.originalPrice) }}</div></template></el-table-column>
        <el-table-column label="开团/成团" width="120" align="center"><template #default="{ row }">{{ row.participantCount }}/{{ row.successGroupCount }}</template></el-table-column>
        <el-table-column label="团长奖励" width="110" align="right"><template #default="{ row }">{{ formatAmount(row.groupLeaderReward) }}</template></el-table-column>
        <el-table-column label="状态" width="100" align="center"><template #default="{ row }"><el-tag :type="statusMap[row.status]?.type || 'info'" size="small">{{ statusMap[row.status]?.label || '未知状态' }}</el-tag></template></el-table-column>
        <el-table-column label="活动时间" min-width="180"><template #default="{ row }"><div>{{ formatTime(row.startTime) }}</div><div class="text-xs text-stone-500">至 {{ formatTime(row.endTime) }}</div></template></el-table-column>
        <el-table-column label="操作" width="220" fixed="right"><template #default="{ row }"><div class="flex flex-wrap justify-end gap-2"><FaButton v-if="row.status !== 1" size="sm" @click="handleStatusChange('groupBuy', row.id, row.name, 1)">开始</FaButton><FaButton v-if="row.status !== 0" size="sm" variant="outline" @click="handleStatusChange('groupBuy', row.id, row.name, 0)">预热</FaButton><FaButton v-if="row.status !== 2" size="sm" variant="outline" @click="handleStatusChange('groupBuy', row.id, row.name, 2)">结束</FaButton><FaButton variant="ghost" size="sm" @click="handleViewGroupBuyDetail(row)">详情</FaButton></div></template></el-table-column>
      </el-table>

      <el-table v-else v-loading="pageState.bargain.loading" :data="pageState.bargain.items">
        <el-table-column label="活动信息" min-width="220"><template #default="{ row }"><div class="space-y-1"><div class="text-sm text-stone-900 font-medium">{{ row.name }}</div><div class="text-xs text-stone-500">{{ row.code }}</div></div></template></el-table-column>
        <el-table-column label="砍价商品" min-width="180" show-overflow-tooltip><template #default="{ row }"><div class="space-y-1"><div>{{ row.productName || '-' }}</div><div class="text-xs text-stone-500">{{ row.skuName || '-' }}</div></div></template></el-table-column>
        <el-table-column label="价格区间" width="140" align="right"><template #default="{ row }"><div class="text-orange-600 font-medium">{{ formatAmount(row.currentLowestPrice) }}</div><div class="text-xs text-stone-500">底价 {{ formatAmount(row.floorPrice) }}</div><div class="text-xs text-stone-400 line-through">原价 {{ formatAmount(row.originalPrice) }}</div></template></el-table-column>
        <el-table-column label="发起/助力" width="120" align="center"><template #default="{ row }">{{ row.participantCount }}/{{ row.helperCount }}</template></el-table-column>
        <el-table-column label="砍成数量" width="90" align="center"><template #default="{ row }">{{ row.successCount }}</template></el-table-column>
        <el-table-column label="状态" width="100" align="center"><template #default="{ row }"><el-tag :type="statusMap[row.status]?.type || 'info'" size="small">{{ statusMap[row.status]?.label || '未知状态' }}</el-tag></template></el-table-column>
        <el-table-column label="活动时间" min-width="180"><template #default="{ row }"><div>{{ formatTime(row.startTime) }}</div><div class="text-xs text-stone-500">至 {{ formatTime(row.endTime) }}</div></template></el-table-column>
        <el-table-column label="操作" width="220" fixed="right"><template #default="{ row }"><div class="flex flex-wrap justify-end gap-2"><FaButton v-if="row.status !== 1" size="sm" @click="handleStatusChange('bargain', row.id, row.name, 1)">开始</FaButton><FaButton v-if="row.status !== 0" size="sm" variant="outline" @click="handleStatusChange('bargain', row.id, row.name, 0)">预热</FaButton><FaButton v-if="row.status !== 2" size="sm" variant="outline" @click="handleStatusChange('bargain', row.id, row.name, 2)">结束</FaButton><FaButton variant="ghost" size="sm" @click="handleViewBargainDetail(row)">详情</FaButton></div></template></el-table-column>
      </el-table>

      <div class="mt-4 flex justify-end px-6 pb-6">
        <el-pagination :current-page="getModuleState(activeModule).currentPage" :page-size="getModuleState(activeModule).pageSize" :page-sizes="[20, 50, 100]" :total="getModuleState(activeModule).total" layout="total, sizes, prev, pager, next, jumper" @current-change="handlePageChange" @size-change="handleSizeChange" />
      </div>
    </FaCard>

    <el-dialog v-model="detailDialogVisible" :title="detailTitle" width="720px">
      <div v-loading="detailLoading">
        <el-empty v-if="!currentSeckill && !currentGroupBuy && !currentBargain" description="暂无活动数据" />
        <div v-else class="space-y-4">
          <el-descriptions :column="2" border>
            <template v-if="currentSeckill">
              <el-descriptions-item label="活动名称">{{ currentSeckill.name }}</el-descriptions-item>
              <el-descriptions-item label="活动编码">{{ currentSeckill.code }}</el-descriptions-item>
              <el-descriptions-item label="商品名称">{{ currentSeckill.productName || '-' }}</el-descriptions-item>
              <el-descriptions-item label="规格名称">{{ currentSeckill.skuName || '-' }}</el-descriptions-item>
              <el-descriptions-item label="秒杀价">{{ formatAmount(currentSeckill.seckillPrice) }}</el-descriptions-item>
              <el-descriptions-item label="活动状态">{{ statusMap[currentSeckill.status]?.label || '未知状态' }}</el-descriptions-item>
              <el-descriptions-item label="库存">{{ currentSeckill.soldStock }}/{{ currentSeckill.totalStock }}</el-descriptions-item>
              <el-descriptions-item label="活动时间">{{ formatTime(currentSeckill.startTime) }}</el-descriptions-item>
            </template>
            <template v-else-if="currentGroupBuy">
              <el-descriptions-item label="活动名称">{{ currentGroupBuy.name }}</el-descriptions-item>
              <el-descriptions-item label="活动编码">{{ currentGroupBuy.code }}</el-descriptions-item>
              <el-descriptions-item label="商品名称">{{ currentGroupBuy.productName || '-' }}</el-descriptions-item>
              <el-descriptions-item label="规格名称">{{ currentGroupBuy.skuName || '-' }}</el-descriptions-item>
              <el-descriptions-item label="拼团价">{{ formatAmount(currentGroupBuy.groupPrice) }}</el-descriptions-item>
              <el-descriptions-item label="活动状态">{{ statusMap[currentGroupBuy.status]?.label || '未知状态' }}</el-descriptions-item>
              <el-descriptions-item label="开团/成团">{{ currentGroupBuy.participantCount }}/{{ currentGroupBuy.successGroupCount }}</el-descriptions-item>
              <el-descriptions-item label="活动时间">{{ formatTime(currentGroupBuy.startTime) }}</el-descriptions-item>
            </template>
            <template v-else-if="currentBargain">
              <el-descriptions-item label="活动名称">{{ currentBargain.name }}</el-descriptions-item>
              <el-descriptions-item label="活动编码">{{ currentBargain.code }}</el-descriptions-item>
              <el-descriptions-item label="商品名称">{{ currentBargain.productName || '-' }}</el-descriptions-item>
              <el-descriptions-item label="规格名称">{{ currentBargain.skuName || '-' }}</el-descriptions-item>
              <el-descriptions-item label="最低价">{{ formatAmount(currentBargain.currentLowestPrice) }}</el-descriptions-item>
              <el-descriptions-item label="活动状态">{{ statusMap[currentBargain.status]?.label || '未知状态' }}</el-descriptions-item>
              <el-descriptions-item label="发起/助力">{{ currentBargain.participantCount }}/{{ currentBargain.helperCount }}</el-descriptions-item>
              <el-descriptions-item label="活动时间">{{ formatTime(currentBargain.startTime) }}</el-descriptions-item>
            </template>
          </el-descriptions>

          <FaCard class="admin-table-card">
            <p class="admin-dialog-note">
              {{ currentSeckill?.description || currentGroupBuy?.description || currentBargain?.description || '暂无活动说明' }}
            </p>
          </FaCard>
        </div>
      </div>
    </el-dialog>
  </div>
</template>

<style scoped>
.module-switcher {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 12px;
}

.module-switcher__item {
  border: 1px solid rgb(226 232 240 / 0.95);
  border-radius: 20px;
  background: rgb(255 255 255 / 0.84);
  padding: 18px;
  text-align: left;
}

.module-switcher__item span {
  display: block;
  color: rgb(15 23 42);
  font-size: 15px;
  font-weight: 700;
}

.module-switcher__item small {
  display: block;
  margin-top: 8px;
  color: rgb(100 116 139);
  font-size: 12px;
  line-height: 1.7;
}

.module-switcher__item.is-active {
  border-color: rgb(14 165 233 / 0.45);
  background: linear-gradient(180deg, rgb(240 249 255), rgb(240 253 250));
  box-shadow: 0 18px 36px rgb(14 165 233 / 0.12);
}

@media (max-width: 960px) {
  .module-switcher {
    grid-template-columns: 1fr;
  }
}
</style>
