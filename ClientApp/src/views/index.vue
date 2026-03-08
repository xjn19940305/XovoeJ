<route lang="yaml">
meta:
  title: 首页
  icon: ant-design:home-twotone
</route>

<script setup lang="ts">
import dashboardApi from '@/api/modules/dashboard'
import AdminMetricCard from '@/components/admin/AdminMetricCard.vue'
import AdminPageHero from '@/components/admin/AdminPageHero.vue'
import { permissionCodes } from '@/utils/permission'

defineOptions({
  name: 'Dashboard',
})

const userStore = useUserStore()
const { auth } = useAuth()
const router = useRouter()

const loading = ref(true)
const recentOrdersLoading = ref(false)
const recentOrders = ref<Api.Order.Order[]>([])

const stats = reactive({
  todayOrders: 0,
  todayUsers: 0,
  todaySalesAmount: 0,
  monthSalesAmount: 0,
  totalOrders: 0,
  pendingPaymentOrders: 0,
  pendingShipmentOrders: 0,
  shippedOrders: 0,
  completedOrders: 0,
  cancelledOrders: 0,
  totalProducts: 0,
  onSaleProducts: 0,
  outOfStockProducts: 0,
  totalUsers: 0,
  thisWeekUsers: 0,
  thisMonthUsers: 0,
})

const overviewCards = computed(() => [
  {
    title: '今日支付订单',
    value: stats.todayOrders,
    description: '当前自然日内完成的支付订单量',
    icon: 'i-heroicons-solid:shopping-bag',
    tone: 'blue' as const,
  },
  {
    title: '今日成交额',
    value: formatAmount(stats.todaySalesAmount),
    description: '订单交易主线的即时经营结果',
    icon: 'i-heroicons-solid:banknotes',
    tone: 'teal' as const,
  },
  {
    title: '待发货订单',
    value: stats.pendingShipmentOrders,
    description: '需要优先关注的履约积压订单',
    icon: 'i-heroicons-solid:truck',
    tone: 'amber' as const,
  },
  {
    title: '售后与取消',
    value: stats.cancelledOrders,
    description: '当前订单取消与售后风险窗口',
    icon: 'i-heroicons-solid:arrow-uturn-left',
    tone: 'rose' as const,
  },
  {
    title: '会员总量',
    value: stats.totalUsers,
    description: '会员营销主线的当前用户体量',
    icon: 'i-heroicons-solid:user-group',
    tone: 'violet' as const,
  },
  {
    title: '本月成交额',
    value: formatAmount(stats.monthSalesAmount),
    description: '衡量当前经营节奏的月度结果',
    icon: 'i-heroicons-solid:chart-bar-square',
    tone: 'sky' as const,
  },
])

const orderStatusCards = computed(() => [
  { title: '待付款', value: stats.pendingPaymentOrders, tone: 'is-amber', icon: 'i-heroicons-solid:clock' },
  { title: '待发货', value: stats.pendingShipmentOrders, tone: 'is-blue', icon: 'i-heroicons-solid:cube' },
  { title: '运输中', value: stats.shippedOrders, tone: 'is-violet', icon: 'i-heroicons-solid:truck' },
  { title: '已完成', value: stats.completedOrders, tone: 'is-emerald', icon: 'i-heroicons-solid:check-circle' },
])

const memberMarketingCards = computed(() => [
  {
    title: '今日新增会员',
    value: stats.todayUsers,
    description: '观察拉新是否跟上交易节奏',
    icon: 'i-heroicons-solid:user-plus',
    tone: 'emerald' as const,
  },
  {
    title: '本周新增会员',
    value: stats.thisWeekUsers,
    description: '反映近期活动和渠道的拉新结果',
    icon: 'i-heroicons-solid:calendar',
    tone: 'teal' as const,
  },
  {
    title: '本月新增会员',
    value: stats.thisMonthUsers,
    description: '用于校验会员增长和营销节奏',
    icon: 'i-heroicons-solid:sparkles',
    tone: 'violet' as const,
  },
  {
    title: '在售商品',
    value: stats.onSaleProducts,
    description: '支撑营销投放和订单转化的商品池',
    icon: 'i-heroicons-solid:cube-transparent',
    tone: 'sky' as const,
  },
])

const quickActions = [
  {
    title: '订单管理',
    description: '查看支付、履约和异常订单',
    icon: 'i-heroicons-solid:shopping-cart',
    route: '/shop/order',
    auth: [permissionCodes.admin.order.read],
  },
  {
    title: '售后管理',
    description: '处理退款、退货与售后申请',
    icon: 'i-heroicons-solid:arrow-uturn-left',
    route: '/shop/after-sale',
    auth: [permissionCodes.admin.aftersale.read],
  },
  {
    title: '资产中心',
    description: '查看钱包、积分和账务流水',
    icon: 'i-heroicons-solid:wallet',
    route: '/shop/asset',
    auth: [permissionCodes.admin.assetWallet.read, permissionCodes.admin.assetPoints.read],
  },
  {
    title: '支付单',
    description: '跟踪订单支付状态与关闭操作',
    icon: 'i-heroicons-solid:credit-card',
    route: '/shop/payment',
    auth: [permissionCodes.admin.payment.read],
  },
  {
    title: '优惠券中心',
    description: '管理模板、批量发券和升级奖励',
    icon: 'i-heroicons-solid:ticket',
    route: '/marketing/coupon',
    auth: [permissionCodes.admin.coupon.read],
  },
  {
    title: '营销活动',
    description: '查看当前核心营销活动和状态',
    icon: 'i-heroicons-solid:megaphone',
    route: '/marketing/promotion',
    auth: [permissionCodes.admin.promotion.read],
  },
  {
    title: '高级营销',
    description: '统一收口秒杀、拼团和砍价',
    icon: 'i-heroicons-solid:fire',
    route: '/marketing/advanced',
    auth: [
      permissionCodes.admin.seckill.manage,
      permissionCodes.admin.groupBuy.manage,
      permissionCodes.admin.bargain.manage,
    ],
  },
  {
    title: '消息中心',
    description: '处理模板、发送任务和失败重试',
    icon: 'i-heroicons-solid:chat-bubble-left-right',
    route: '/message/template',
    auth: [permissionCodes.admin.messageTemplate.read],
  },
]

const availableQuickActions = computed(() => quickActions.filter(item => auth(item.auth)))

const greeting = computed(() => {
  const hour = new Date().getHours()
  if (hour < 6) {
    return '凌晨好'
  }
  if (hour < 12) {
    return '上午好'
  }
  if (hour < 18) {
    return '下午好'
  }
  return '晚上好'
})

const todayLabel = computed(() => {
  const date = new Date()
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`
})

const currentRoleLabel = computed(() => userStore.roles.join(' / ') || '未分配角色')

const recentOrderStatusMap: Record<number, { label: string, className: string }> = {
  0: { label: '待付款', className: 'is-amber' },
  1: { label: '待发货', className: 'is-blue' },
  2: { label: '运输中', className: 'is-violet' },
  3: { label: '已完成', className: 'is-emerald' },
  4: { label: '已取消', className: 'is-slate' },
}

async function getStatsData() {
  loading.value = true
  try {
    const res = await dashboardApi.getStats()
    Object.assign(stats, res.data)
  }
  finally {
    loading.value = false
  }
}

async function getRecentOrders() {
  recentOrdersLoading.value = true
  try {
    const res = await dashboardApi.getRecentOrders()
    recentOrders.value = res.data.slice(0, 6)
  }
  finally {
    recentOrdersLoading.value = false
  }
}

function go(route: string) {
  router.push(route)
}

function formatAmount(value: number) {
  return `¥ ${value.toFixed(2)}`
}

function formatOrderTime(value?: string) {
  return value || '-'
}

onMounted(async () => {
  await Promise.all([getStatsData(), getRecentOrders()])
})
</script>

<template>
  <div class="admin-page-shell dashboard-page">
    <AdminPageHero
      eyebrow="经营驾驶舱"
      title="订单交易与会员营销总览"
      description="首页只保留当前业务系统最关键的两条主线：订单交易和会员营销。先看今日经营结果，再看履约压力、会员增长和营销动作。"
    >
      <template #actions>
        <FaButton variant="ghost" @click="getStatsData">
          <template #icon>
            <FaIcon name="i-heroicons-solid:arrow-path" />
          </template>
          刷新经营数据
        </FaButton>
        <FaButton @click="go('/shop/order')">
          <template #icon>
            <FaIcon name="i-heroicons-solid:shopping-cart" />
          </template>
          进入订单管理
        </FaButton>
      </template>

      <div class="dashboard-hero-note">
        <div class="dashboard-hero-note__main">
          <span class="dashboard-hero-note__eyebrow">{{ greeting }}</span>
          <strong>{{ userStore.nickName || userStore.userName }}</strong>
          <span>今天是 {{ todayLabel }}，优先关注订单转化、发货积压和会员增长。</span>
        </div>
        <div class="dashboard-hero-note__meta">
          <span>当前角色：{{ currentRoleLabel }}</span>
          <span>在售商品：{{ stats.onSaleProducts }}</span>
          <span>缺货商品：{{ stats.outOfStockProducts }}</span>
        </div>
      </div>
    </AdminPageHero>

    <div class="admin-overview-grid admin-overview-grid--6">
      <AdminMetricCard
        v-for="card in overviewCards"
        :key="card.title"
        :title="card.title"
        :value="loading ? '--' : card.value"
        :description="card.description"
        :icon="card.icon"
        :tone="card.tone"
        variant="board"
      />
    </div>

    <div class="dashboard-main-grid">
      <div class="dashboard-primary-column">
        <FaCard class="admin-table-card">
          <template #header>
            <div class="admin-section-header">
              <div class="admin-section-header__meta">
                <span class="admin-section-header__title">订单交易面板</span>
                <span class="admin-section-header__description">先看交易结果，再看待付款、待发货和履约节奏。</span>
              </div>
              <FaButton variant="ghost" @click="go('/shop/order')">
                <template #icon>
                  <FaIcon name="i-heroicons-solid:arrow-right" />
                </template>
                查看全部订单
              </FaButton>
            </div>
          </template>

          <div class="dashboard-panel-content">
            <div class="dashboard-status-grid">
              <div
                v-for="item in orderStatusCards"
                :key="item.title"
                class="dashboard-status-card"
                :class="item.tone"
              >
                <div class="dashboard-status-card__icon">
                  <FaIcon :name="item.icon" class="size-5" />
                </div>
                <div class="dashboard-status-card__content">
                  <span class="dashboard-status-card__title">{{ item.title }}</span>
                  <strong class="dashboard-status-card__value">{{ loading ? '--' : item.value }}</strong>
                </div>
              </div>
            </div>

            <div class="dashboard-recent-orders">
              <div class="dashboard-block-title">
                <span>最近订单</span>
                <small>用于快速确认最新交易和履约状态</small>
              </div>

              <div v-if="recentOrdersLoading" class="dashboard-empty">
                正在加载最近订单
              </div>
              <div v-else-if="recentOrders.length === 0" class="dashboard-empty">
                当前没有可展示的最近订单
              </div>
              <div v-else class="dashboard-order-list">
                <div
                  v-for="order in recentOrders"
                  :key="order.id"
                  class="dashboard-order-row"
                >
                  <div class="dashboard-order-row__main">
                    <strong>{{ order.orderNo }}</strong>
                    <span>{{ order.userName || '未知用户' }}</span>
                  </div>
                  <div class="dashboard-order-row__meta">
                    <span class="dashboard-order-status" :class="recentOrderStatusMap[order.status]?.className">
                      {{ recentOrderStatusMap[order.status]?.label || '未知状态' }}
                    </span>
                    <span>{{ formatOrderTime(order.createdAt) }}</span>
                  </div>
                  <div class="dashboard-order-row__amount">
                    {{ formatAmount(order.totalAmount) }}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </FaCard>
      </div>

      <div class="dashboard-secondary-column">
        <FaCard class="admin-table-card">
          <template #header>
            <div class="admin-section-header">
              <div class="admin-section-header__meta">
                <span class="admin-section-header__title">会员营销面板</span>
                <span class="admin-section-header__description">关注会员增长、营销资源和商品供给的匹配关系。</span>
              </div>
              <FaButton variant="ghost" @click="go('/marketing/coupon')">
                <template #icon>
                  <FaIcon name="i-heroicons-solid:ticket" />
                </template>
                进入优惠券中心
              </FaButton>
            </div>
          </template>

          <div class="dashboard-panel-content">
            <div class="dashboard-member-grid">
              <AdminMetricCard
                v-for="card in memberMarketingCards"
                :key="card.title"
                :title="card.title"
                :value="loading ? '--' : card.value"
                :description="card.description"
                :icon="card.icon"
                :tone="card.tone"
                variant="board"
              />
            </div>

            <div class="dashboard-insights-grid">
              <div class="dashboard-note-card">
                <span class="dashboard-note-card__label">经营提示</span>
                <p>今日订单 {{ stats.todayOrders }} 单，待发货 {{ stats.pendingShipmentOrders }} 单，建议优先检查履约积压和售后回流。</p>
              </div>
              <div class="dashboard-note-card">
                <span class="dashboard-note-card__label">会员提示</span>
                <p>本周新增会员 {{ stats.thisWeekUsers }} 人，本月新增 {{ stats.thisMonthUsers }} 人，可结合优惠券发放和营销活动进一步拉新。</p>
              </div>
            </div>
          </div>
        </FaCard>

        <FaCard class="admin-table-card">
          <template #header>
            <div class="admin-section-header">
              <div class="admin-section-header__meta">
                <span class="admin-section-header__title">快捷入口</span>
                <span class="admin-section-header__description">保持高频操作入口收敛，避免首页沦为装饰面板。</span>
              </div>
            </div>
          </template>

          <div class="dashboard-actions-grid">
            <button
              v-for="item in availableQuickActions"
              :key="item.title"
              type="button"
              class="dashboard-action-card"
              @click="go(item.route)"
            >
              <span class="dashboard-action-card__icon">
                <FaIcon :name="item.icon" class="size-5" />
              </span>
              <span class="dashboard-action-card__title">{{ item.title }}</span>
              <span class="dashboard-action-card__description">{{ item.description }}</span>
            </button>
          </div>
        </FaCard>
      </div>
    </div>
  </div>
</template>

<style scoped>
.dashboard-page {
  gap: 24px;
}

.dashboard-hero-note {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
  border: 1px solid rgb(226 232 240 / 0.9);
  border-radius: 22px;
  background: rgb(255 255 255 / 0.78);
  padding: 18px 20px;
}

.dashboard-hero-note__main,
.dashboard-hero-note__meta {
  display: flex;
  flex-wrap: wrap;
  gap: 10px 14px;
  align-items: center;
}

.dashboard-hero-note__main {
  color: rgb(51 65 85);
  font-size: 14px;
}

.dashboard-hero-note__main strong {
  color: rgb(15 23 42);
}

.dashboard-hero-note__eyebrow {
  display: inline-flex;
  align-items: center;
  min-height: 28px;
  border-radius: 9999px;
  background: rgb(15 23 42);
  padding: 0 12px;
  color: white;
  font-size: 12px;
  font-weight: 600;
}

.dashboard-hero-note__meta {
  color: rgb(100 116 139);
  font-size: 13px;
}

.dashboard-main-grid {
  display: grid;
  grid-template-columns: minmax(0, 1.3fr) minmax(360px, 0.9fr);
  gap: 20px;
}

.dashboard-primary-column,
.dashboard-secondary-column {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.dashboard-panel-content {
  display: flex;
  flex-direction: column;
  gap: 20px;
  padding: 24px 26px 26px;
}

.dashboard-status-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 14px;
}

.dashboard-status-card {
  display: flex;
  align-items: center;
  gap: 14px;
  border: 1px solid rgb(226 232 240 / 0.9);
  border-radius: 20px;
  background: linear-gradient(180deg, rgb(255 255 255), rgb(248 250 252));
  padding: 18px;
}

.dashboard-status-card__icon {
  display: flex;
  height: 44px;
  width: 44px;
  flex-shrink: 0;
  align-items: center;
  justify-content: center;
  border-radius: 14px;
  background: rgb(255 255 255 / 0.7);
}

.dashboard-status-card__content {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.dashboard-status-card__title {
  color: rgb(100 116 139);
  font-size: 12px;
}

.dashboard-status-card__value {
  color: rgb(15 23 42);
  font-size: 24px;
  line-height: 1;
}

.dashboard-status-card.is-amber {
  background: linear-gradient(180deg, rgb(255 251 235), rgb(254 243 199));
}

.dashboard-status-card.is-blue {
  background: linear-gradient(180deg, rgb(239 246 255), rgb(219 234 254));
}

.dashboard-status-card.is-violet {
  background: linear-gradient(180deg, rgb(245 243 255), rgb(237 233 254));
}

.dashboard-status-card.is-emerald {
  background: linear-gradient(180deg, rgb(236 253 245), rgb(209 250 229));
}

.dashboard-status-card.is-slate {
  background: linear-gradient(180deg, rgb(248 250 252), rgb(226 232 240));
}

.dashboard-block-title {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.dashboard-block-title span {
  color: rgb(15 23 42);
  font-size: 16px;
  font-weight: 700;
}

.dashboard-block-title small {
  color: rgb(100 116 139);
  font-size: 13px;
}

.dashboard-order-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.dashboard-order-row {
  display: grid;
  grid-template-columns: minmax(0, 1.1fr) auto auto;
  gap: 16px;
  align-items: center;
  border: 1px solid rgb(226 232 240 / 0.9);
  border-radius: 18px;
  background: rgb(248 250 252 / 0.72);
  padding: 16px 18px;
}

.dashboard-order-row__main,
.dashboard-order-row__meta {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.dashboard-order-row__main strong {
  color: rgb(15 23 42);
  font-size: 14px;
}

.dashboard-order-row__main span,
.dashboard-order-row__meta span {
  color: rgb(100 116 139);
  font-size: 12px;
}

.dashboard-order-status {
  display: inline-flex;
  width: fit-content;
  align-items: center;
  border-radius: 9999px;
  padding: 5px 10px;
  font-size: 12px;
  font-weight: 600;
}

.dashboard-order-status.is-amber {
  background: rgb(254 243 199);
  color: rgb(180 83 9);
}

.dashboard-order-status.is-blue {
  background: rgb(219 234 254);
  color: rgb(29 78 216);
}

.dashboard-order-status.is-violet {
  background: rgb(237 233 254);
  color: rgb(109 40 217);
}

.dashboard-order-status.is-emerald {
  background: rgb(209 250 229);
  color: rgb(5 150 105);
}

.dashboard-order-status.is-slate {
  background: rgb(226 232 240);
  color: rgb(71 85 105);
}

.dashboard-order-row__amount {
  color: rgb(15 23 42);
  font-size: 15px;
  font-weight: 700;
  text-align: right;
}

.dashboard-empty {
  display: flex;
  min-height: 140px;
  align-items: center;
  justify-content: center;
  border: 1px dashed rgb(203 213 225);
  border-radius: 18px;
  color: rgb(100 116 139);
  font-size: 14px;
}

.dashboard-member-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 14px;
}

.dashboard-insights-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 14px;
}

.dashboard-note-card {
  display: flex;
  flex-direction: column;
  gap: 12px;
  border: 1px solid rgb(226 232 240 / 0.9);
  border-radius: 20px;
  background: linear-gradient(180deg, rgb(255 255 255), rgb(248 250 252));
  padding: 20px;
}

.dashboard-note-card__label {
  color: rgb(15 118 110);
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.dashboard-note-card p {
  margin: 0;
  color: rgb(51 65 85);
  font-size: 14px;
  line-height: 1.85;
}

.dashboard-actions-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 14px;
  padding: 24px 26px 26px;
}

.dashboard-action-card {
  display: flex;
  min-height: 128px;
  flex-direction: column;
  justify-content: center;
  gap: 12px;
  border: 1px solid rgb(226 232 240 / 0.9);
  border-radius: 20px;
  background: linear-gradient(180deg, rgb(255 255 255), rgb(248 250 252));
  padding: 20px;
  text-align: left;
  transition: transform 160ms ease, box-shadow 160ms ease, border-color 160ms ease;
}

.dashboard-action-card:hover {
  transform: translateY(-2px);
  border-color: rgb(15 118 110 / 0.24);
  box-shadow: 0 18px 36px rgb(15 23 42 / 0.08);
}

.dashboard-action-card__icon {
  display: flex;
  height: 42px;
  width: 42px;
  align-items: center;
  justify-content: center;
  border-radius: 14px;
  background: linear-gradient(135deg, rgb(15 23 42), rgb(15 118 110));
  color: white;
}

.dashboard-action-card__title {
  color: rgb(15 23 42);
  font-size: 15px;
  font-weight: 700;
}

.dashboard-action-card__description {
  color: rgb(100 116 139);
  font-size: 13px;
  line-height: 1.7;
}

@media (max-width: 1380px) {
  .dashboard-main-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 1080px) {
  .dashboard-status-grid,
  .dashboard-member-grid,
  .dashboard-insights-grid,
  .dashboard-actions-grid {
    grid-template-columns: 1fr 1fr;
  }
}

@media (max-width: 768px) {
  .dashboard-hero-note,
  .dashboard-order-row {
    grid-template-columns: 1fr;
    flex-direction: column;
    align-items: flex-start;
  }

  .dashboard-status-grid,
  .dashboard-member-grid,
  .dashboard-insights-grid,
  .dashboard-actions-grid {
    grid-template-columns: 1fr;
  }

  .dashboard-panel-content {
    padding: 18px;
  }
}
</style>
