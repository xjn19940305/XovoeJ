<script setup lang="ts">
import type { FormInstance, FormRules, TagProps } from 'element-plus'
import { ElMessage, ElMessageBox } from 'element-plus'
import dayjs from 'dayjs'
import marketingApi from '@/api/modules/marketing'
import AdminMetricCard from '@/components/admin/AdminMetricCard.vue'

defineOptions({
  name: 'MarketingCouponPage',
})

type CouponPanel = 'templates' | 'issueBatches' | 'rewardRules'

const activePanel = ref<CouponPanel>('templates')

const templateQuery = reactive({
  keyword: '',
  status: undefined as number | undefined,
  couponType: undefined as number | undefined,
})

const templateState = reactive({
  loading: false,
  items: [] as Api.Marketing.CouponTemplate[],
  total: 0,
  currentPage: 1,
  pageSize: 20,
})

const batchState = reactive({
  loading: false,
  items: [] as Api.Marketing.CouponIssueBatch[],
  total: 0,
  currentPage: 1,
  pageSize: 10,
})

const rewardRuleState = reactive({
  loading: false,
  items: [] as Api.Marketing.MemberLevelRewardRule[],
})

const couponTemplateOptions = ref<Api.Marketing.CouponTemplate[]>([])

const detailDialogVisible = ref(false)
const detailLoading = ref(false)
const currentCoupon = ref<Api.Marketing.CouponTemplate | null>(null)

const issueDialogVisible = ref(false)
const issueSubmitting = ref(false)
const issueFormRef = ref<FormInstance>()
const issueTargetUserText = ref('')
const issueForm = reactive<Api.Marketing.CreateCouponIssueBatchRequest>({
  targetType: 'all-users',
  couponTemplateIds: [],
  targetUserIds: [],
  targetMemberLevelCodes: [],
})

const rewardDialogVisible = ref(false)
const rewardSubmitting = ref(false)
const rewardFormRef = ref<FormInstance>()
const rewardDialogMode = ref<'create' | 'edit'>('create')
const rewardForm = reactive<Api.Marketing.SaveMemberLevelRewardRuleRequest>({
  levelCode: 'silver',
  levelName: '白银会员',
  couponTemplateIds: [],
  status: 1,
  sort: 100,
  description: '',
})

const panelOptions: Array<{ key: CouponPanel, title: string, description: string }> = [
  { key: 'templates', title: '模板池', description: '查看当前可用券模板与库存使用情况。' },
  { key: 'issueBatches', title: '发券批次', description: '群发、指定用户和按等级发券统一在这里处理。' },
  { key: 'rewardRules', title: '升级奖励', description: '配置会员升级后自动发放的券包。' },
]

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

const issueTargetOptions = [
  { label: '全量用户', value: 'all-users' },
  { label: '指定用户', value: 'specified-users' },
  { label: '会员等级', value: 'member-levels' },
]

const memberLevelOptions = [
  { label: '普通会员', value: 'normal' },
  { label: '白银会员', value: 'silver' },
  { label: '黄金会员', value: 'gold' },
  { label: '铂金会员', value: 'platinum' },
]

const couponTypeMap: Record<number, string> = {
  0: '满减券',
  1: '折扣券',
  2: '无门槛券',
}

const couponStatusMap: Record<number, { label: string, type: TagProps['type'] }> = {
  0: { label: '草稿', type: 'info' },
  1: { label: '进行中', type: 'success' },
  2: { label: '已暂停', type: 'warning' },
  3: { label: '已结束', type: 'danger' },
}

const issueStatusMap: Record<string, { label: string, type: TagProps['type'] }> = {
  pending: { label: '执行中', type: 'warning' },
  completed: { label: '已完成', type: 'success' },
}

const rewardStatusMap: Record<number, { label: string, type: TagProps['type'] }> = {
  0: { label: '停用', type: 'info' },
  1: { label: '启用', type: 'success' },
}

const issueRules: FormRules = {
  couponTemplateIds: [{ required: true, type: 'array', min: 1, message: '请至少选择一个优惠券模板', trigger: 'change' }],
  targetType: [{ required: true, message: '请选择发券范围', trigger: 'change' }],
  targetUserIds: [{
    validator: (_rule, value: string[]) => issueForm.targetType !== 'specified-users' || value.length > 0
      ? Promise.resolve()
      : Promise.reject(new Error('请填写至少一个用户 ID')),
    trigger: 'blur',
  }],
  targetMemberLevelCodes: [{
    validator: (_rule, value: string[]) => issueForm.targetType !== 'member-levels' || value.length > 0
      ? Promise.resolve()
      : Promise.reject(new Error('请至少选择一个会员等级')),
    trigger: 'change',
  }],
}

const rewardRules: FormRules = {
  levelCode: [{ required: true, message: '请选择会员等级', trigger: 'change' }],
  couponTemplateIds: [{ required: true, type: 'array', min: 1, message: '请至少选择一个奖励券模板', trigger: 'change' }],
}

const templateNameMap = computed(() => new Map(couponTemplateOptions.value.map(item => [item.id, item.name])))

const summaryCards = computed(() => [
  {
    title: '模板总数',
    value: templateState.total,
    icon: 'i-heroicons-solid:ticket',
    tone: 'blue' as const,
    description: '模板中心当前可管理的优惠券模板总量。',
  },
  {
    title: '当前页已发放',
    value: templateState.items.reduce((sum, item) => sum + item.issuedQuantity, 0),
    icon: 'i-heroicons-solid:paper-airplane',
    tone: 'amber' as const,
    description: '当前页模板累计发放量，用于观察券触达规模。',
  },
  {
    title: '发券批次',
    value: batchState.total,
    icon: 'i-heroicons-solid:users',
    tone: 'sky' as const,
    description: '后台批量发券任务总量，便于追踪群发节奏。',
  },
  {
    title: '启用奖励规则',
    value: rewardRuleState.items.filter(item => item.status === 1).length,
    icon: 'i-heroicons-solid:sparkles',
    tone: 'emerald' as const,
    description: '当前启用的会员升级奖励规则数量。',
  },
])

const activePanelMeta = computed(() => panelOptions.find(item => item.key === activePanel.value) ?? panelOptions[0])

watch(() => rewardForm.levelCode, (levelCode) => {
  const currentLevel = memberLevelOptions.find(item => item.value === levelCode)
  if (currentLevel) {
    rewardForm.levelName = currentLevel.label
  }
})

watch(() => issueForm.targetType, (targetType) => {
  if (targetType !== 'specified-users') {
    issueTargetUserText.value = ''
    issueForm.targetUserIds = []
  }
  if (targetType !== 'member-levels') {
    issueForm.targetMemberLevelCodes = []
  }
})

async function getCouponList() {
  templateState.loading = true
  try {
    const res = await marketingApi.getCouponList({
      page: templateState.currentPage,
      pageSize: templateState.pageSize,
      ...templateQuery,
    })
    templateState.items = res.data.items
    templateState.total = res.data.total
  }
  finally {
    templateState.loading = false
  }
}

async function getCouponTemplateOptions() {
  const res = await marketingApi.getCouponList({ page: 1, pageSize: 200 })
  couponTemplateOptions.value = res.data.items
}

async function getIssueBatchList() {
  batchState.loading = true
  try {
    const res = await marketingApi.getCouponIssueBatchList({
      page: batchState.currentPage,
      pageSize: batchState.pageSize,
    })
    batchState.items = res.data.items
    batchState.total = res.data.total
  }
  finally {
    batchState.loading = false
  }
}

async function getRewardRuleList() {
  rewardRuleState.loading = true
  try {
    const res = await marketingApi.getMemberLevelRewardRules()
    rewardRuleState.items = res.data
  }
  finally {
    rewardRuleState.loading = false
  }
}

async function loadPageData() {
  await Promise.all([
    getCouponList(),
    getCouponTemplateOptions(),
    getIssueBatchList(),
    getRewardRuleList(),
  ])
}

function handleTemplateSearch() {
  templateState.currentPage = 1
  getCouponList()
}

function handleTemplateReset() {
  templateQuery.keyword = ''
  templateQuery.status = undefined
  templateQuery.couponType = undefined
  templateState.currentPage = 1
  getCouponList()
}

function refreshCurrentPanel() {
  if (activePanel.value === 'templates') {
    return getCouponList()
  }
  if (activePanel.value === 'issueBatches') {
    return getIssueBatchList()
  }
  return getRewardRuleList()
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

async function handleUpdateCouponStatus(row: Api.Marketing.CouponTemplate, status: number) {
  const actionText = status === 1 ? '启用' : status === 2 ? '暂停' : status === 3 ? '结束' : '设为草稿'
  await ElMessageBox.confirm(`确认${actionText}优惠券模板“${row.name}”吗？`, '模板状态变更', {
    type: 'warning',
  })

  await marketingApi.updateCouponStatus(row.id, { status })
  ElMessage.success(`优惠券模板已${actionText}`)
  await getTemplateList()
  await loadCouponTemplateOptions()
}

function handleTemplatePageChange(page: number) {
  templateState.currentPage = page
  getCouponList()
}

function handleTemplateSizeChange(size: number) {
  templateState.pageSize = size
  templateState.currentPage = 1
  getCouponList()
}

function handleBatchPageChange(page: number) {
  batchState.currentPage = page
  getIssueBatchList()
}

function handleBatchSizeChange(size: number) {
  batchState.pageSize = size
  batchState.currentPage = 1
  getIssueBatchList()
}

function openIssueDialog() {
  issueDialogVisible.value = true
  issueForm.targetType = 'all-users'
  issueForm.couponTemplateIds = []
  issueForm.targetUserIds = []
  issueForm.targetMemberLevelCodes = []
  issueTargetUserText.value = ''
  issueFormRef.value?.clearValidate()
}

async function submitIssueBatch() {
  issueForm.targetUserIds = parseMultiLineInput(issueTargetUserText.value)
  const valid = await issueFormRef.value?.validate().catch(() => false)
  if (!valid) {
    return
  }

  issueSubmitting.value = true
  try {
    await marketingApi.createCouponIssueBatch({
      targetType: issueForm.targetType,
      couponTemplateIds: issueForm.couponTemplateIds,
      targetUserIds: issueForm.targetType === 'specified-users' ? issueForm.targetUserIds : undefined,
      targetMemberLevelCodes: issueForm.targetType === 'member-levels' ? issueForm.targetMemberLevelCodes : undefined,
    })
    ElMessage.success('发券批次已创建')
    issueDialogVisible.value = false
    await Promise.all([getIssueBatchList(), getCouponList(), getCouponTemplateOptions()])
  }
  finally {
    issueSubmitting.value = false
  }
}

function openRewardDialog(rule?: Api.Marketing.MemberLevelRewardRule) {
  rewardDialogVisible.value = true
  rewardDialogMode.value = rule ? 'edit' : 'create'
  rewardForm.levelCode = rule?.levelCode ?? 'silver'
  rewardForm.levelName = rule?.levelName ?? '白银会员'
  rewardForm.couponTemplateIds = rule ? [...rule.couponTemplateIds] : []
  rewardForm.status = rule?.status ?? 1
  rewardForm.sort = rule?.sort ?? 100
  rewardForm.description = rule?.description ?? ''
  rewardFormRef.value?.clearValidate()
}

async function submitRewardRule() {
  const valid = await rewardFormRef.value?.validate().catch(() => false)
  if (!valid) {
    return
  }

  rewardSubmitting.value = true
  try {
    await marketingApi.saveMemberLevelRewardRule({
      levelCode: rewardForm.levelCode,
      levelName: rewardForm.levelName,
      couponTemplateIds: rewardForm.couponTemplateIds,
      status: rewardForm.status,
      sort: rewardForm.sort,
      description: rewardForm.description?.trim() || undefined,
    })
    ElMessage.success(rewardDialogMode.value === 'create' ? '奖励规则已创建' : '奖励规则已更新')
    rewardDialogVisible.value = false
    await getRewardRuleList()
  }
  finally {
    rewardSubmitting.value = false
  }
}

function formatDiscount(row: Api.Marketing.CouponTemplate) {
  if (row.couponType === 1) {
    return `${row.discountValue} 折`
  }
  if (row.discountType === 1) {
    return `减 ${row.discountValue.toFixed(2)}`
  }
  return `¥ ${row.discountValue.toFixed(2)}`
}

function formatValidity(row: Pick<Api.Marketing.CouponTemplate, 'startTime' | 'endTime'>) {
  if (!row.startTime && !row.endTime) {
    return '不限开始至不限结束'
  }
  const start = row.startTime ? dayjs(row.startTime).format('YYYY-MM-DD HH:mm') : '不限开始'
  const end = row.endTime ? dayjs(row.endTime).format('YYYY-MM-DD HH:mm') : '不限结束'
  return `${start} 至 ${end}`
}

function formatDateTime(value?: string) {
  return value ? dayjs(value).format('YYYY-MM-DD HH:mm:ss') : '-'
}

function formatIssueTarget(batch: Api.Marketing.CouponIssueBatch) {
  if (batch.targetType === 'all-users') {
    return '全部商城用户'
  }
  if (batch.targetType === 'specified-users') {
    return `${batch.targetUserIds.length} 个指定用户`
  }
  return batch.targetMemberLevelCodes.map(resolveMemberLevelName).join('、')
}

function formatIssueTargetTag(targetType: string) {
  return issueTargetOptions.find(item => item.value === targetType)?.label ?? targetType
}

function parseMultiLineInput(value: string) {
  return value.split(/[\n,，;；\s]+/).map(item => item.trim()).filter(Boolean)
}

function resolveTemplateNames(ids: string[]) {
  return ids.map(id => templateNameMap.value.get(id) ?? `模板 ${id.slice(0, 8)}`)
}

function resolveMemberLevelName(levelCode: string) {
  return memberLevelOptions.find(item => item.value === levelCode)?.label ?? levelCode
}

onMounted(() => {
  loadPageData()
})
</script>

<template>
  <div class="marketing-coupon-page">
    <div class="grid mb-4 gap-4 md:grid-cols-2 xl:grid-cols-4">
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

    <FaCard class="mb-4 hero-card">
      <div class="hero-card__content">
        <div>
          <p class="hero-card__eyebrow">
            会员与优惠券闭环
          </p>
          <div class="hero-card__heading">
            <h2>优惠券中心</h2>
            <p>{{ activePanelMeta.description }}</p>
          </div>
        </div>

        <div class="hero-card__actions">
          <FaButton variant="ghost" @click="refreshCurrentPanel">
            <template #icon>
              <FaIcon name="i-heroicons-solid:arrow-path" />
            </template>
            刷新当前面板
          </FaButton>
          <FaButton v-if="activePanel === 'issueBatches'" @click="openIssueDialog">
            <template #icon>
              <FaIcon name="i-heroicons-solid:paper-airplane" />
            </template>
            新建发券批次
          </FaButton>
          <FaButton v-if="activePanel === 'rewardRules'" @click="openRewardDialog()">
            <template #icon>
              <FaIcon name="i-heroicons-solid:sparkles" />
            </template>
            新建升级奖励
          </FaButton>
        </div>
      </div>

      <div class="panel-switcher">
        <button
          v-for="item in panelOptions"
          :key="item.key"
          type="button"
          class="panel-switcher__item"
          :class="{ 'is-active': activePanel === item.key }"
          @click="activePanel = item.key"
        >
          <span>{{ item.title }}</span>
          <small>{{ item.description }}</small>
        </button>
      </div>
    </FaCard>

    <template v-if="activePanel === 'templates'">
      <FaCard class="search-card mb-4">
        <div class="search-header">
          <div class="search-title">
            <FaIcon name="i-heroicons-solid:magnifying-glass" class="size-5" />
            <span>模板筛选</span>
          </div>
        </div>
        <div class="search-body">
          <div class="search-grid">
            <div class="search-field">
              <label class="search-label">关键字</label>
              <el-input v-model="templateQuery.keyword" placeholder="搜索模板名称或编码" clearable>
                <template #prefix>
                  <FaIcon name="i-heroicons-solid:ticket" class="size-4 text-stone-400" />
                </template>
              </el-input>
            </div>
            <div class="search-field">
              <label class="search-label">优惠券类型</label>
              <el-select v-model="templateQuery.couponType" placeholder="全部类型" clearable class="w-full">
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
              <el-select v-model="templateQuery.status" placeholder="全部状态" clearable class="w-full">
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
          <FaButton @click="handleTemplateSearch">
            <template #icon>
              <FaIcon name="i-heroicons-solid:magnifying-glass" />
            </template>
            查询
          </FaButton>
          <FaButton class="search-reset-btn" @click="handleTemplateReset">
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
              <span class="ml-2 text-sm text-stone-500">查看库存、有效期与当前投放情况</span>
            </div>
          </div>
        </template>

        <el-table v-loading="templateState.loading" :data="templateState.items">
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
              ¥ {{ row.minOrderAmount.toFixed(2) }}
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
              <el-tag :type="couponStatusMap[row.status]?.type || 'info'" size="small">
                {{ couponStatusMap[row.status]?.label || '未知' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="创建时间" width="180">
            <template #default="{ row }">
              {{ formatDateTime(row.createdAt) }}
            </template>
          </el-table-column>
          <el-table-column label="操作" width="260" fixed="right">
            <template #default="{ row }">
              <div class="flex flex-wrap justify-end gap-2">
                <FaButton v-if="row.status !== 1" size="sm" @click="handleUpdateCouponStatus(row, 1)">
                  启用
                </FaButton>
                <FaButton v-if="row.status !== 2" size="sm" variant="outline" @click="handleUpdateCouponStatus(row, 2)">
                  暂停
                </FaButton>
                <FaButton v-if="row.status !== 3" size="sm" variant="outline" @click="handleUpdateCouponStatus(row, 3)">
                  结束
                </FaButton>
                <FaButton v-if="row.status !== 0" size="sm" variant="outline" @click="handleUpdateCouponStatus(row, 0)">
                  草稿
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
            :current-page="templateState.currentPage"
            :page-size="templateState.pageSize"
            :total="templateState.total"
            :page-sizes="[20, 50, 100]"
            layout="total, sizes, prev, pager, next, jumper"
            @current-change="handleTemplatePageChange"
            @size-change="handleTemplateSizeChange"
          />
        </div>
      </FaCard>
    </template>

    <template v-else-if="activePanel === 'issueBatches'">
      <FaCard>
        <template #header>
          <div class="flex items-center justify-between">
            <div>
              <span class="font-medium">发券批次</span>
              <span class="ml-2 text-sm text-stone-500">支持全量用户、指定用户和按会员等级群发</span>
            </div>
          </div>
        </template>

        <el-table v-loading="batchState.loading" :data="batchState.items">
          <el-table-column label="发券范围" width="120" align="center">
            <template #default="{ row }">
              <el-tag size="small" type="info">
                {{ formatIssueTargetTag(row.targetType) }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="目标范围" min-width="180" show-overflow-tooltip>
            <template #default="{ row }">
              {{ formatIssueTarget(row) }}
            </template>
          </el-table-column>
          <el-table-column label="发放模板" min-width="240">
            <template #default="{ row }">
              <div class="chip-list">
                <span
                  v-for="name in resolveTemplateNames(row.couponTemplateIds)"
                  :key="`${row.id}-${name}`"
                  class="chip-list__item"
                >
                  {{ name }}
                </span>
              </div>
            </template>
          </el-table-column>
          <el-table-column label="请求 / 成功 / 失败" width="170" align="center">
            <template #default="{ row }">
              {{ row.requestedCount }} / {{ row.succeededCount }} / {{ row.failedCount }}
            </template>
          </el-table-column>
          <el-table-column label="状态" width="100" align="center">
            <template #default="{ row }">
              <el-tag :type="issueStatusMap[row.status]?.type || 'info'" size="small">
                {{ issueStatusMap[row.status]?.label || row.status }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="createdBy" label="创建人" min-width="140" show-overflow-tooltip />
          <el-table-column label="创建时间" width="180">
            <template #default="{ row }">
              {{ formatDateTime(row.createdAt) }}
            </template>
          </el-table-column>
        </el-table>

        <div class="mt-4 flex justify-end">
          <el-pagination
            :current-page="batchState.currentPage"
            :page-size="batchState.pageSize"
            :total="batchState.total"
            :page-sizes="[10, 20, 50]"
            layout="total, sizes, prev, pager, next, jumper"
            @current-change="handleBatchPageChange"
            @size-change="handleBatchSizeChange"
          />
        </div>
      </FaCard>
    </template>

    <template v-else>
      <FaCard>
        <template #header>
          <div class="flex items-center justify-between">
            <div>
              <span class="font-medium">会员升级奖励</span>
              <span class="ml-2 text-sm text-stone-500">支付完成后若发生升级，将自动发放这里配置的券包</span>
            </div>
          </div>
        </template>

        <el-table v-loading="rewardRuleState.loading" :data="rewardRuleState.items">
          <el-table-column label="会员等级" min-width="180">
            <template #default="{ row }">
              <div class="space-y-1">
                <div class="font-medium text-stone-900">
                  {{ row.levelName }}
                </div>
                <div class="text-xs text-stone-500">
                  {{ row.levelCode }}
                </div>
              </div>
            </template>
          </el-table-column>
          <el-table-column label="奖励模板" min-width="240">
            <template #default="{ row }">
              <div class="chip-list">
                <span
                  v-for="name in resolveTemplateNames(row.couponTemplateIds)"
                  :key="`${row.id}-${name}`"
                  class="chip-list__item"
                >
                  {{ name }}
                </span>
              </div>
            </template>
          </el-table-column>
          <el-table-column label="状态" width="100" align="center">
            <template #default="{ row }">
              <el-tag :type="rewardStatusMap[row.status]?.type || 'info'" size="small">
                {{ rewardStatusMap[row.status]?.label || '未知' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="sort" label="排序" width="90" align="center" />
          <el-table-column prop="description" label="规则说明" min-width="220" show-overflow-tooltip />
          <el-table-column label="更新时间" width="180">
            <template #default="{ row }">
              {{ formatDateTime(row.updatedAt || row.createdAt) }}
            </template>
          </el-table-column>
          <el-table-column label="操作" width="120" fixed="right">
            <template #default="{ row }">
              <FaButton variant="ghost" size="sm" @click="openRewardDialog(row)">
                <template #icon>
                  <FaIcon name="i-heroicons-solid:pencil-square" />
                </template>
                编辑
              </FaButton>
            </template>
          </el-table-column>
        </el-table>
      </FaCard>
    </template>

    <el-dialog v-model="detailDialogVisible" title="优惠券详情" width="720px">
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
              {{ couponStatusMap[currentCoupon.status]?.label || '未知' }}
            </el-descriptions-item>
            <el-descriptions-item label="优惠力度">
              {{ formatDiscount(currentCoupon) }}
            </el-descriptions-item>
            <el-descriptions-item label="使用门槛">
              ¥ {{ currentCoupon.minOrderAmount.toFixed(2) }}
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

    <el-dialog v-model="issueDialogVisible" title="新建发券批次" width="680px">
      <el-form ref="issueFormRef" :model="issueForm" :rules="issueRules" label-width="110px">
        <el-form-item label="发券范围" prop="targetType">
          <el-radio-group v-model="issueForm.targetType">
            <el-radio-button v-for="item in issueTargetOptions" :key="item.value" :label="item.value">
              {{ item.label }}
            </el-radio-button>
          </el-radio-group>
        </el-form-item>

        <el-form-item label="优惠券模板" prop="couponTemplateIds">
          <el-select
            v-model="issueForm.couponTemplateIds"
            multiple
            filterable
            collapse-tags
            collapse-tags-tooltip
            placeholder="选择要发放的优惠券模板"
            class="w-full"
          >
            <el-option
              v-for="item in couponTemplateOptions"
              :key="item.id"
              :label="`${item.name} (${item.code})`"
              :value="item.id"
            />
          </el-select>
        </el-form-item>

        <el-form-item v-if="issueForm.targetType === 'specified-users'" label="用户 ID" prop="targetUserIds">
          <el-input
            v-model="issueTargetUserText"
            type="textarea"
            :rows="5"
            placeholder="请输入用户 ID，支持换行、逗号或空格分隔"
          />
        </el-form-item>

        <el-form-item v-if="issueForm.targetType === 'member-levels'" label="会员等级" prop="targetMemberLevelCodes">
          <el-select v-model="issueForm.targetMemberLevelCodes" multiple placeholder="选择会员等级" class="w-full">
            <el-option v-for="item in memberLevelOptions" :key="item.value" :label="item.label" :value="item.value" />
          </el-select>
        </el-form-item>
      </el-form>

      <template #footer>
        <div class="dialog-footer">
          <FaButton variant="ghost" @click="issueDialogVisible = false">
            取消
          </FaButton>
          <FaButton type="primary" :loading="issueSubmitting" @click="submitIssueBatch">
            创建批次
          </FaButton>
        </div>
      </template>
    </el-dialog>

    <el-dialog v-model="rewardDialogVisible" :title="rewardDialogMode === 'create' ? '新建升级奖励' : '编辑升级奖励'" width="680px">
      <el-form ref="rewardFormRef" :model="rewardForm" :rules="rewardRules" label-width="110px">
        <el-form-item label="会员等级" prop="levelCode">
          <el-select v-model="rewardForm.levelCode" placeholder="选择会员等级" class="w-full">
            <el-option v-for="item in memberLevelOptions" :key="item.value" :label="item.label" :value="item.value" />
          </el-select>
        </el-form-item>

        <el-form-item label="奖励券模板" prop="couponTemplateIds">
          <el-select
            v-model="rewardForm.couponTemplateIds"
            multiple
            filterable
            collapse-tags
            collapse-tags-tooltip
            placeholder="选择升级后发放的券模板"
            class="w-full"
          >
            <el-option
              v-for="item in couponTemplateOptions"
              :key="item.id"
              :label="`${item.name} (${item.code})`"
              :value="item.id"
            />
          </el-select>
        </el-form-item>

        <div class="grid gap-4 md:grid-cols-2">
          <el-form-item label="状态">
            <el-radio-group v-model="rewardForm.status">
              <el-radio-button :label="1">
                启用
              </el-radio-button>
              <el-radio-button :label="0">
                停用
              </el-radio-button>
            </el-radio-group>
          </el-form-item>

          <el-form-item label="排序">
            <el-input-number v-model="rewardForm.sort" :min="0" :max="9999" class="w-full" />
          </el-form-item>
        </div>

        <el-form-item label="规则说明">
          <el-input
            v-model="rewardForm.description"
            type="textarea"
            :rows="4"
            placeholder="例如：升级到黄金会员后赠送两张满减券和一张免邮券"
          />
        </el-form-item>
      </el-form>

      <template #footer>
        <div class="dialog-footer">
          <FaButton variant="ghost" @click="rewardDialogVisible = false">
            取消
          </FaButton>
          <FaButton type="primary" :loading="rewardSubmitting" @click="submitRewardRule">
            保存规则
          </FaButton>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.hero-card {
  position: relative;
  overflow: hidden;
}

.hero-card::before {
  position: absolute;
  inset: 0;
  background:
    radial-gradient(circle at top right, rgb(14 165 233 / 0.18), transparent 34%),
    radial-gradient(circle at left center, rgb(16 185 129 / 0.12), transparent 28%);
  content: '';
  pointer-events: none;
}

.hero-card__content {
  position: relative;
  display: flex;
  gap: 16px;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 18px;
}

.hero-card__eyebrow {
  margin-bottom: 10px;
  color: rgb(8 145 178);
  font-size: 12px;
  font-weight: 600;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}

.hero-card__heading h2 {
  margin: 0;
  color: rgb(28 25 23);
  font-size: 28px;
  font-weight: 700;
  letter-spacing: -0.03em;
}

.hero-card__heading p {
  margin: 8px 0 0;
  max-width: 720px;
  color: rgb(87 83 78);
  font-size: 14px;
  line-height: 1.7;
}

.hero-card__actions,
.dialog-footer {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}

.hero-card__actions {
  justify-content: flex-end;
}

.panel-switcher {
  position: relative;
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(3, minmax(0, 1fr));
}

.panel-switcher__item {
  border: 1px solid rgb(231 229 228);
  border-radius: 18px;
  background: linear-gradient(180deg, rgb(255 255 255 / 0.9), rgb(250 250 249 / 0.95));
  padding: 16px 18px;
  text-align: left;
  transition: transform 160ms ease, border-color 160ms ease, box-shadow 160ms ease;
}

.panel-switcher__item span {
  display: block;
  color: rgb(28 25 23);
  font-size: 15px;
  font-weight: 600;
}

.panel-switcher__item small {
  display: block;
  margin-top: 6px;
  color: rgb(120 113 108);
  font-size: 12px;
  line-height: 1.6;
}

.panel-switcher__item.is-active {
  border-color: rgb(14 165 233 / 0.55);
  background: linear-gradient(180deg, rgb(240 249 255), rgb(240 253 250));
  box-shadow: 0 18px 36px rgb(14 165 233 / 0.12);
}

.chip-list {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.chip-list__item {
  display: inline-flex;
  align-items: center;
  border: 1px solid rgb(224 231 255);
  border-radius: 9999px;
  background: rgb(238 242 255);
  padding: 4px 10px;
  color: rgb(67 56 202);
  font-size: 12px;
  line-height: 1.4;
}

.dialog-footer {
  justify-content: flex-end;
}

@media (max-width: 960px) {
  .hero-card__content {
    flex-direction: column;
  }

  .hero-card__actions {
    justify-content: flex-start;
  }

  .panel-switcher {
    grid-template-columns: 1fr;
  }
}
</style>
