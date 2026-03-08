<script setup lang="ts">
import { ElMessage } from 'element-plus'
import dayjs from 'dayjs'
import mallAccountApi from '@/api/modules/mall-account'
import mallCouponApi from '@/api/modules/mall-coupon'
import AdminMetricCard from '@/components/admin/AdminMetricCard.vue'

defineOptions({
  name: 'MemberCouponPage',
})

type CouponPanel = 'center' | 'wallet'

const activePanel = ref<CouponPanel>('center')
const loading = ref(false)
const claimingId = ref('')
const summary = ref<Api.Mall.AccountSummary | null>(null)

const centerQuery = reactive<Api.Mall.CouponCenterParams>({
  page: 1,
  pageSize: 12,
  keyword: '',
  couponType: undefined,
})

const walletQuery = reactive<Api.Mall.UserCouponParams>({
  page: 1,
  pageSize: 12,
  status: undefined,
})

const centerState = reactive({
  items: [] as Api.Mall.CouponCenterItem[],
  total: 0,
})

const walletState = reactive({
  items: [] as Api.Mall.UserCoupon[],
  total: 0,
})

const couponTypeOptions = [
  { label: '全部类型', value: undefined },
  { label: '满减券', value: 0 },
  { label: '折扣券', value: 1 },
  { label: '无门槛券', value: 2 },
]

const walletStatusOptions = [
  { label: '全部状态', value: undefined },
  { label: '可使用', value: 0 },
  { label: '已使用', value: 1 },
  { label: '已过期', value: 2 },
  { label: '锁定中', value: 3 },
  { label: '已作废', value: 4 },
]

const couponTypeMap: Record<number, string> = {
  0: '满减券',
  1: '折扣券',
  2: '无门槛券',
}

const walletStatusMap: Record<number, { label: string, tone: string }> = {
  0: { label: '可使用', tone: 'is-available' },
  1: { label: '已使用', tone: 'is-used' },
  2: { label: '已过期', tone: 'is-expired' },
  3: { label: '锁定中', tone: 'is-locked' },
  4: { label: '已作废', tone: 'is-expired' },
}

const summaryCards = computed(() => [
  {
    title: '可领优惠券',
    value: centerState.total,
    icon: 'i-heroicons-solid:gift',
    tone: 'sky' as const,
    description: '当前领券中心可领取的优惠券数量。',
  },
  {
    title: '我的可用券',
    value: summary.value?.availableCouponCount ?? 0,
    icon: 'i-heroicons-solid:ticket',
    tone: 'blue' as const,
    description: '当前用户券包里仍可使用的优惠券数量。',
  },
  {
    title: '已领取张数',
    value: walletState.total,
    icon: 'i-heroicons-solid:inbox-stack',
    tone: 'emerald' as const,
    description: '已经领取入券包的全部用户券数量。',
  },
  {
    title: '会员等级',
    value: summary.value?.memberLevel ?? '普通会员',
    icon: 'i-heroicons-solid:sparkles',
    tone: 'amber' as const,
    description: '当前会员等级会影响部分优惠券和奖励资格。',
  },
])

async function loadSummary() {
  const res = await mallAccountApi.getSummary()
  summary.value = res.data
}

async function loadCouponCenter() {
  const res = await mallCouponApi.getCouponCenter(centerQuery)
  centerState.items = res.data.items
  centerState.total = res.data.total
}

async function loadWallet() {
  const res = await mallCouponApi.getWallet(walletQuery)
  walletState.items = res.data.items
  walletState.total = res.data.total
}

async function loadPageData() {
  loading.value = true
  try {
    await Promise.all([loadSummary(), loadCouponCenter(), loadWallet()])
  }
  finally {
    loading.value = false
  }
}

async function claimCoupon(id: string) {
  claimingId.value = id
  try {
    await mallCouponApi.claimCoupon(id)
    ElMessage.success('领取成功')
    await Promise.all([loadCouponCenter(), loadWallet(), loadSummary()])
    activePanel.value = 'wallet'
  }
  finally {
    claimingId.value = ''
  }
}

function handleCenterSearch() {
  centerQuery.page = 1
  loadCouponCenter()
}

function handleWalletSearch() {
  walletQuery.page = 1
  loadWallet()
}

function formatDiscount(discountType: number, couponType: number, discountValue: number) {
  if (couponType === 1) {
    return `${discountValue} 折`
  }
  if (discountType === 1) {
    return `减 ${discountValue.toFixed(2)}`
  }
  return `¥ ${discountValue.toFixed(2)}`
}

function formatThreshold(amount: number) {
  return amount > 0 ? `满 ¥ ${amount.toFixed(2)} 可用` : '无门槛使用'
}

function formatDateRange(start?: string, end?: string) {
  const startText = start ? dayjs(start).format('YYYY-MM-DD') : '不限开始'
  const endText = end ? dayjs(end).format('YYYY-MM-DD') : '不限结束'
  return `${startText} 至 ${endText}`
}

function formatWalletExpire(item: Api.Mall.UserCoupon) {
  if (!item.expiredAt) {
    return '长期有效'
  }
  return `有效期至 ${dayjs(item.expiredAt).format('YYYY-MM-DD HH:mm')}`
}

onMounted(() => {
  loadPageData()
})
</script>

<template>
  <div class="member-coupon-page">
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

    <FaCard class="mb-4 coupon-hero">
      <div class="coupon-hero__header">
        <div>
          <p class="coupon-hero__eyebrow">
            会员优惠
          </p>
          <h2>领券中心与我的券包</h2>
          <p class="coupon-hero__desc">
            先领取，再到结算页使用。当前页面已经接入真实用户券模型，展示的是用户实际持有的券。
          </p>
        </div>
        <div class="coupon-hero__switcher">
          <button :class="{ active: activePanel === 'center' }" @click="activePanel = 'center'">
            领券中心
          </button>
          <button :class="{ active: activePanel === 'wallet' }" @click="activePanel = 'wallet'">
            我的券包
          </button>
        </div>
      </div>
    </FaCard>

    <template v-if="activePanel === 'center'">
      <FaCard class="mb-4">
        <div class="grid gap-4 md:grid-cols-[1.4fr_1fr_1fr]">
          <el-input v-model="centerQuery.keyword" placeholder="搜索优惠券名称或编码" clearable @keyup.enter="handleCenterSearch" />
          <el-select v-model="centerQuery.couponType" placeholder="全部类型" clearable>
            <el-option
              v-for="item in couponTypeOptions"
              :key="String(item.value)"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
          <FaButton @click="handleCenterSearch">
            <template #icon>
              <FaIcon name="i-heroicons-solid:magnifying-glass" />
            </template>
            查询可领券
          </FaButton>
        </div>
      </FaCard>

      <div v-loading="loading" class="coupon-grid">
        <FaCard v-for="item in centerState.items" :key="item.id" class="coupon-card is-center">
          <div class="coupon-card__header">
            <div>
              <p class="coupon-card__type">
                {{ couponTypeMap[item.couponType] || '优惠券' }}
              </p>
              <h3>{{ item.name }}</h3>
            </div>
            <span class="coupon-card__discount">
              {{ formatDiscount(item.discountType, item.couponType, item.discountValue) }}
            </span>
          </div>

          <div class="coupon-card__meta">
            <p>{{ formatThreshold(item.minOrderAmount) }}</p>
            <p>{{ formatDateRange(item.startTime, item.endTime) }}</p>
            <p>已领取 {{ item.claimedCount }} / 限领 {{ item.receiveLimit ?? '不限' }}</p>
          </div>

          <p class="coupon-card__description">
            {{ item.description || '领取后可在结算时抵扣订单金额。' }}
          </p>

          <FaButton
            class="w-full"
            :loading="claimingId === item.id"
            @click="claimCoupon(item.id)"
          >
            立即领取
          </FaButton>
        </FaCard>
      </div>
    </template>

    <template v-else>
      <FaCard class="mb-4">
        <div class="grid gap-4 md:grid-cols-[1fr_1fr_auto]">
          <el-select v-model="walletQuery.status" placeholder="全部状态" clearable>
            <el-option
              v-for="item in walletStatusOptions"
              :key="String(item.value)"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
          <div class="flex items-center text-sm text-stone-500">
            已领取 {{ walletState.total }} 张优惠券
          </div>
          <FaButton @click="handleWalletSearch">
            <template #icon>
              <FaIcon name="i-heroicons-solid:funnel" />
            </template>
            筛选券包
          </FaButton>
        </div>
      </FaCard>

      <div v-loading="loading" class="coupon-grid">
        <FaCard v-for="item in walletState.items" :key="item.id" class="coupon-card" :class="walletStatusMap[item.status]?.tone">
          <div class="coupon-card__header">
            <div>
              <p class="coupon-card__type">
                {{ couponTypeMap[item.couponType] || '优惠券' }}
              </p>
              <h3>{{ item.name }}</h3>
            </div>
            <span class="coupon-card__discount">
              {{ formatDiscount(item.discountType, item.couponType, item.discountValue) }}
            </span>
          </div>

          <div class="coupon-card__meta">
            <p>{{ formatThreshold(item.minOrderAmount) }}</p>
            <p>{{ formatWalletExpire(item) }}</p>
            <p>领取时间 {{ item.claimedAt ? dayjs(item.claimedAt).format('YYYY-MM-DD HH:mm') : '-' }}</p>
          </div>

          <div class="coupon-card__footer">
            <span class="coupon-status-pill" :class="walletStatusMap[item.status]?.tone">
              {{ walletStatusMap[item.status]?.label || '未知状态' }}
            </span>
            <span v-if="item.usedAt" class="text-xs text-stone-500">
              使用于 {{ dayjs(item.usedAt).format('YYYY-MM-DD HH:mm') }}
            </span>
          </div>
        </FaCard>
      </div>
    </template>
  </div>
</template>

<style scoped>
.coupon-hero {
  position: relative;
  overflow: hidden;
}

.coupon-hero::before {
  position: absolute;
  inset: 0;
  background:
    radial-gradient(circle at top left, rgb(99 102 241 / 0.18), transparent 30%),
    radial-gradient(circle at bottom right, rgb(14 165 233 / 0.16), transparent 26%);
  content: '';
}

.coupon-hero__header {
  position: relative;
  display: flex;
  justify-content: space-between;
  gap: 20px;
  align-items: center;
}

.coupon-hero__eyebrow {
  color: rgb(79 70 229);
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.14em;
  text-transform: uppercase;
}

.coupon-hero__header h2 {
  margin: 6px 0 0;
  color: rgb(28 25 23);
  font-size: 28px;
  font-weight: 700;
}

.coupon-hero__desc {
  margin-top: 10px;
  max-width: 720px;
  color: rgb(87 83 78);
  line-height: 1.7;
}

.coupon-hero__switcher {
  display: inline-flex;
  padding: 6px;
  border-radius: 9999px;
  background: rgb(245 245 244 / 0.92);
}

.coupon-hero__switcher button {
  border: 0;
  border-radius: 9999px;
  background: transparent;
  padding: 10px 16px;
  color: rgb(87 83 78);
}

.coupon-hero__switcher button.active {
  background: linear-gradient(135deg, rgb(79 70 229), rgb(14 165 233));
  color: white;
}

.coupon-grid {
  display: grid;
  gap: 16px;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
}

.coupon-card {
  border: 1px solid rgb(231 229 228);
}

.coupon-card__header {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  align-items: flex-start;
}

.coupon-card__type {
  color: rgb(120 113 108);
  font-size: 12px;
}

.coupon-card__header h3 {
  margin: 6px 0 0;
  color: rgb(28 25 23);
  font-size: 20px;
  font-weight: 700;
}

.coupon-card__discount {
  color: rgb(79 70 229);
  font-size: 24px;
  font-weight: 700;
  white-space: nowrap;
}

.coupon-card__meta {
  margin: 18px 0 16px;
  color: rgb(87 83 78);
  font-size: 13px;
  line-height: 1.8;
}

.coupon-card__description {
  min-height: 44px;
  margin-bottom: 16px;
  color: rgb(87 83 78);
  font-size: 13px;
  line-height: 1.7;
}

.coupon-card__footer {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  align-items: center;
}

.coupon-status-pill {
  display: inline-flex;
  align-items: center;
  border-radius: 9999px;
  padding: 6px 12px;
  font-size: 12px;
}

.is-available .coupon-status-pill {
  background: rgb(220 252 231);
  color: rgb(22 101 52);
}

.is-used .coupon-status-pill {
  background: rgb(226 232 240);
  color: rgb(51 65 85);
}

.is-expired .coupon-status-pill {
  background: rgb(254 226 226);
  color: rgb(153 27 27);
}

.is-locked .coupon-status-pill {
  background: rgb(254 249 195);
  color: rgb(133 77 14);
}

@media (max-width: 960px) {
  .coupon-hero__header {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>
