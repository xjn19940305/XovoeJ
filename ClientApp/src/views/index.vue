<route lang="yaml">
meta:
  title: 主页
  icon: ant-design:home-twotone
</route>

<script setup lang="ts">
import dashboardApi from '@/api/modules/dashboard'
import { permissionCodes } from '@/utils/permission'

defineOptions({
  name: 'Dashboard',
})

// 统计卡片数据
const statsCards = ref([
  {
    title: '今日订单',
    value: 0,
    icon: 'i-heroicons-solid:shopping-bag',
    gradient: 'from-blue-500 to-blue-600',
    iconBg: 'bg-white/20',
    loading: true,
    trend: 0,
  },
  {
    title: '今日用户',
    value: 0,
    icon: 'i-heroicons-solid:user-group',
    gradient: 'from-emerald-500 to-emerald-600',
    iconBg: 'bg-white/20',
    loading: true,
    trend: 0,
  },
  {
    title: '今日销售额',
    value: 0,
    icon: 'i-heroicons-solid:banknotes',
    gradient: 'from-orange-500 to-orange-600',
    iconBg: 'bg-white/20',
    loading: true,
    prefix: '¥',
    trend: 0,
  },
  {
    title: '本月销售额',
    value: 0,
    icon: 'i-heroicons-solid:chart-bar-square',
    gradient: 'from-violet-500 to-violet-600',
    iconBg: 'bg-white/20',
    loading: true,
    prefix: '¥',
    trend: 0,
  },
])

// 订单统计
const orderStats = ref({
  total: 0,
  pendingPayment: 0,
  pendingShipment: 0,
  shipped: 0,
  completed: 0,
  cancelled: 0,
  loading: true,
})

// 商品统计
const productStats = ref({
  total: 0,
  onSale: 0,
  outOfStock: 0,
  lowStock: 0,
  loading: true,
})

// 用户统计
const userStats = ref({
  total: 0,
  today: 0,
  thisWeek: 0,
  thisMonth: 0,
  loading: true,
})

// 快捷操作
const quickActions = [
  { title: '创建订单', icon: 'i-heroicons-solid:document-plus', route: '/shop/order/create', color: 'from-blue-500 to-blue-600', auth: [permissionCodes.admin.order.create] },
  { title: '添加商品', icon: 'i-heroicons-solid:plus-circle', route: '/shop/product/create', color: 'from-emerald-500 to-emerald-600', auth: [permissionCodes.admin.product.create] },
  { title: '用户管理', icon: 'i-heroicons-solid:users', route: '/system/user', color: 'from-violet-500 to-violet-600', auth: [permissionCodes.admin.user.read] },
  { title: '角色管理', icon: 'i-heroicons-solid:shield-check', route: '/system/role', color: 'from-orange-500 to-orange-600', auth: [permissionCodes.admin.role.read] },
]

const userStore = useUserStore()
const { auth } = useAuth()

// 过滤有权限的快捷操作
const filteredQuickActions = computed(() => {
  return quickActions.filter((action) => {
    if (!action.auth || action.auth.length === 0) {
      return true
    }
    return auth(action.auth)
  })
})

// 跳转路由
const router = useRouter()
function handleNavigate(route: string) {
  router.push(route)
}

// 获取统计数据
async function getStatsData() {
  try {
    const res = await dashboardApi.getStats()
    const data = res.data

    // 更新今日统计
    statsCards.value[0].value = data.todayOrders || 0
    statsCards.value[0].loading = false

    statsCards.value[1].value = data.todayUsers || 0
    statsCards.value[1].loading = false

    statsCards.value[2].value = data.todaySalesAmount || 0
    statsCards.value[2].loading = false

    statsCards.value[3].value = data.monthSalesAmount || 0
    statsCards.value[3].loading = false

    // 更新订单统计
    orderStats.value = {
      total: data.totalOrders || 0,
      pendingPayment: data.pendingPaymentOrders || 0,
      pendingShipment: data.pendingShipmentOrders || 0,
      shipped: data.shippedOrders || 0,
      completed: data.completedOrders || 0,
      cancelled: data.cancelledOrders || 0,
      loading: false,
    }

    // 更新商品统计
    productStats.value = {
      total: data.totalProducts || 0,
      onSale: data.onSaleProducts || 0,
      outOfStock: data.outOfStockProducts || 0,
      lowStock: 0,
      loading: false,
    }

    // 更新用户统计
    userStats.value = {
      total: data.totalUsers || 0,
      today: data.todayUsers || 0,
      thisWeek: data.thisWeekUsers || 0,
      thisMonth: data.thisMonthUsers || 0,
      loading: false,
    }
  }
  catch {
    // 使用模拟数据
    statsCards.value.forEach((card) => {
      card.loading = false
    })
    orderStats.value.loading = false
    productStats.value.loading = false
    userStats.value.loading = false
  }
}

// 最近订单
const recentOrders = ref<Api.Order.Order[]>([])
const recentOrdersLoading = ref(false)

async function getRecentOrders() {
  recentOrdersLoading.value = true
  try {
    const res = await dashboardApi.getRecentOrders()
    recentOrders.value = res.data.slice(0, 6)
  }
  catch {
    recentOrders.value = []
  }
  finally {
    recentOrdersLoading.value = false
  }
}

// 订单状态配置
const orderStatusConfig: Record<number, { label: string, color: string, bgColor: string, icon: string }> = {
  0: { label: '待付款', color: 'text-amber-600', bgColor: 'bg-amber-50', icon: 'i-heroicons-solid:clock' },
  1: { label: '待发货', color: 'text-blue-600', bgColor: 'bg-blue-50', icon: 'i-heroicons-solid:cube' },
  2: { label: '待收货', color: 'text-violet-600', bgColor: 'bg-violet-50', icon: 'i-heroicons-solid:truck' },
  3: { label: '已完成', color: 'text-emerald-600', bgColor: 'bg-emerald-50', icon: 'i-heroicons-solid:check-circle' },
  4: { label: '已取消', color: 'text-gray-500', bgColor: 'bg-gray-50', icon: 'i-heroicons-solid:x-circle' },
}

// 格式化金额
function formatAmount(amount: number) {
  return `¥${amount.toFixed(2)}`
}

// 获取问候语
/* eslint-disable style/max-statements-per-line */
const greeting = computed(() => {
  const hour = new Date().getHours()
  if (hour < 6) { return '凌晨好' }
  if (hour < 9) { return '早上好' }
  if (hour < 12) { return '上午好' }
  if (hour < 14) { return '中午好' }
  if (hour < 18) { return '下午好' }
  if (hour < 22) { return '晚上好' }
  return '夜深了'
})
/* eslint-enable style/max-statements-per-line */

// 获取星期几
const weekDay = computed(() => {
  const days = ['周日', '周一', '周二', '周三', '周四', '周五', '周六']
  return days[new Date().getDay()]
})

// 当前日期
const currentDate = computed(() => {
  const now = new Date()
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`
})

onMounted(() => {
  getStatsData()
  getRecentOrders()
})
</script>

<template>
  <div class="dashboard">
    <!-- 欢迎区域 -->
    <div class="welcome-section">
      <div class="welcome-content">
        <div class="welcome-greeting">
          <h1 class="welcome-title">
            {{ greeting }}，{{ userStore.nickName || userStore.userName }}
            <span class="welcome-emoji">👋</span>
          </h1>
          <p class="welcome-subtitle">
            欢迎回来，开始您今天的工作吧！
          </p>
        </div>
        <div class="welcome-date">
          <div class="date-item">
            <FaIcon name="i-heroicons-solid:calendar-days" class="size-5" />
            <span>{{ currentDate }}</span>
          </div>
          <div class="date-item">
            <FaIcon name="i-heroicons-solid:clock" class="size-5" />
            <span>{{ weekDay }}</span>
          </div>
        </div>
      </div>
      <div class="welcome-decoration">
        <div class="decoration-dot decoration-dot-1" />
        <div class="decoration-dot decoration-dot-2" />
        <div class="decoration-dot decoration-dot-3" />
      </div>
    </div>

    <!-- 统计卡片 -->
    <div class="stats-grid">
      <div
        v-for="(card, index) in statsCards"
        :key="index"
        class="stat-card" :class="[`gradient-${index}`]"
      >
        <div class="stat-card-bg" :class="card.gradient" />
        <div class="stat-card-content">
          <div class="stat-icon" :class="card.iconBg">
            <FaIcon :name="card.icon" class="size-7" />
          </div>
          <div class="stat-info">
            <p class="stat-label">
              {{ card.title }}
            </p>
            <p class="stat-value">
              <span v-if="card.loading" class="stat-skeleton">--</span>
              <span v-else class="stat-number">
                {{ card.prefix }}{{ card.value.toLocaleString() }}
              </span>
            </p>
          </div>
        </div>
        <div class="stat-card-decoration">
          <FaIcon name="i-heroicons-solid:chart-bar" class="size-16 opacity-10" />
        </div>
      </div>
    </div>

    <div class="main-grid">
      <!-- 左侧区域 -->
      <div class="left-section">
        <!-- 订单状态统计 -->
        <div class="card order-stats-card">
          <div class="card-header">
            <div class="card-title">
              <FaIcon name="i-heroicons-solid:shopping-cart" class="size-5 text-blue-500" />
              <span>订单状态统计</span>
            </div>
            <FaButton variant="ghost" size="sm" class="view-all-btn" @click="handleNavigate('/shop/order')">
              查看全部
              <FaIcon name="i-heroicons-solid:arrow-right" class="size-4" />
            </FaButton>
          </div>
          <div v-loading="orderStats.loading" class="order-stats-grid">
            <div v-for="(value, key) in orderStats" :key="key" class="order-stat-item" :class="`stat-${key}`">
              <div v-if="key !== 'loading'" class="order-stat-content">
                <div class="order-stat-value">
                  {{ value }}
                </div>
                <div class="order-stat-label">
                  {{
                    key === 'total' ? '总订单'
                    : key === 'pendingPayment' ? '待付款'
                      : key === 'pendingShipment' ? '待发货'
                        : key === 'shipped' ? '已发货'
                          : key === 'completed' ? '已完成'
                            : '已取消'
                  }}
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 用户统计 -->
        <div class="card user-stats-card">
          <div class="card-header">
            <div class="card-title">
              <FaIcon name="i-heroicons-solid:users" class="size-5 text-emerald-500" />
              <span>用户增长趋势</span>
            </div>
            <FaButton variant="ghost" size="sm" class="view-all-btn" @click="handleNavigate('/system/user')">
              查看全部
              <FaIcon name="i-heroicons-solid:arrow-right" class="size-4" />
            </FaButton>
          </div>
          <div v-loading="userStats.loading" class="user-stats-grid">
            <div class="user-stat-card primary">
              <div class="user-stat-icon">
                <FaIcon name="i-heroicons-solid:user-group" class="size-6" />
              </div>
              <div class="user-stat-info">
                <div class="user-stat-value">
                  {{ userStats.total }}
                </div>
                <div class="user-stat-label">
                  总用户数
                </div>
              </div>
            </div>
            <div class="user-stat-card success">
              <div class="user-stat-icon">
                <FaIcon name="i-heroicons-solid:sun" class="size-6" />
              </div>
              <div class="user-stat-info">
                <div class="user-stat-value">
                  {{ userStats.today }}
                </div>
                <div class="user-stat-label">
                  今日新增
                </div>
              </div>
            </div>
            <div class="user-stat-card info">
              <div class="user-stat-icon">
                <FaIcon name="i-heroicons-solid:calendar" class="size-6" />
              </div>
              <div class="user-stat-info">
                <div class="user-stat-value">
                  {{ userStats.thisWeek }}
                </div>
                <div class="user-stat-label">
                  本周新增
                </div>
              </div>
            </div>
            <div class="user-stat-card warning">
              <div class="user-stat-icon">
                <FaIcon name="i-heroicons-solid:calendar-days" class="size-6" />
              </div>
              <div class="user-stat-info">
                <div class="user-stat-value">
                  {{ userStats.thisMonth }}
                </div>
                <div class="user-stat-label">
                  本月新增
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 最近订单 -->
        <div class="card recent-orders-card">
          <div class="card-header">
            <div class="card-title">
              <FaIcon name="i-heroicons-solid:receipt" class="size-5 text-violet-500" />
              <span>最近订单</span>
            </div>
            <FaButton variant="ghost" size="sm" class="view-all-btn" @click="handleNavigate('/shop/order')">
              查看全部
              <FaIcon name="i-heroicons-solid:arrow-right" class="size-4" />
            </FaButton>
          </div>
          <div v-loading="recentOrdersLoading" class="recent-orders-content">
            <div v-if="recentOrders.length === 0" class="empty-state">
              <FaIcon name="i-heroicons-solid:inbox" class="size-12" />
              <p>暂无订单数据</p>
            </div>
            <div v-else class="orders-list">
              <div
                v-for="order in recentOrders"
                :key="order.id"
                class="order-item"
              >
                <div class="order-item-left">
                  <div class="order-no">
                    {{ order.orderNo }}
                  </div>
                  <div class="order-user">
                    {{ order.userName || '未知用户' }}
                  </div>
                </div>
                <div class="order-item-center">
                  <div class="order-status" :class="orderStatusConfig[order.status]?.color">
                    <FaIcon :name="orderStatusConfig[order.status]?.icon" class="size-4" />
                    {{ orderStatusConfig[order.status]?.label }}
                  </div>
                  <div class="order-time">
                    {{ order.createdAt }}
                  </div>
                </div>
                <div class="order-item-right">
                  <div class="order-amount">
                    {{ formatAmount(order.totalAmount) }}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 右侧区域 -->
      <div class="right-section">
        <!-- 商品统计 -->
        <div class="card product-stats-card">
          <div class="card-header">
            <div class="card-title">
              <FaIcon name="i-heroicons-solid:cube" class="size-5 text-orange-500" />
              <span>商品统计</span>
            </div>
            <FaButton variant="ghost" size="sm" class="view-all-btn" @click="handleNavigate('/shop/product')">
              查看全部
              <FaIcon name="i-heroicons-solid:arrow-right" class="size-4" />
            </FaButton>
          </div>
          <div v-loading="productStats.loading" class="product-stats-content">
            <div class="product-stat-item">
              <div class="product-stat-header">
                <div class="product-stat-icon blue">
                  <FaIcon name="i-heroicons-solid:cube" class="size-5" />
                </div>
                <div class="product-stat-info">
                  <div class="product-stat-value">
                    {{ productStats.total }}
                  </div>
                  <div class="product-stat-label">
                    商品总数
                  </div>
                </div>
              </div>
              <div class="product-stat-progress">
                <div class="progress-bar">
                  <div class="progress-fill blue" style="width: 100%" />
                </div>
              </div>
            </div>
            <div class="product-stat-item">
              <div class="product-stat-header">
                <div class="product-stat-icon green">
                  <FaIcon name="i-heroicons-solid:check-circle" class="size-5" />
                </div>
                <div class="product-stat-info">
                  <div class="product-stat-value">
                    {{ productStats.onSale }}
                  </div>
                  <div class="product-stat-label">
                    在售商品
                  </div>
                </div>
              </div>
              <div class="product-stat-progress">
                <div class="progress-bar">
                  <div class="progress-fill green" :style="{ width: productStats.total ? `${productStats.onSale / productStats.total * 100}%` : '0%' }" />
                </div>
              </div>
            </div>
            <div class="product-stat-item">
              <div class="product-stat-header">
                <div class="product-stat-icon orange">
                  <FaIcon name="i-heroicons-solid:exclamation-triangle" class="size-5" />
                </div>
                <div class="product-stat-info">
                  <div class="product-stat-value">
                    {{ productStats.lowStock }}
                  </div>
                  <div class="product-stat-label">
                    库存预警
                  </div>
                </div>
              </div>
              <div class="product-stat-progress">
                <div class="progress-bar">
                  <div class="progress-fill orange" :style="{ width: productStats.total ? `${productStats.lowStock / productStats.total * 100}%` : '0%' }" />
                </div>
              </div>
            </div>
            <div class="product-stat-item">
              <div class="product-stat-header">
                <div class="product-stat-icon red">
                  <FaIcon name="i-heroicons-solid:x-circle" class="size-5" />
                </div>
                <div class="product-stat-info">
                  <div class="product-stat-value">
                    {{ productStats.outOfStock }}
                  </div>
                  <div class="product-stat-label">
                    缺货商品
                  </div>
                </div>
              </div>
              <div class="product-stat-progress">
                <div class="progress-bar">
                  <div class="progress-fill red" :style="{ width: productStats.total ? `${productStats.outOfStock / productStats.total * 100}%` : '0%' }" />
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 快捷操作 -->
        <div class="card quick-actions-card">
          <div class="card-header">
            <div class="card-title">
              <FaIcon name="i-heroicons-solid:bolt" class="size-5 text-amber-500" />
              <span>快捷操作</span>
            </div>
          </div>
          <div class="quick-actions-grid">
            <div
              v-for="action in filteredQuickActions"
              :key="action.title"
              class="quick-action-item" :class="[action.color]"
              @click="handleNavigate(action.route)"
            >
              <div class="quick-action-icon">
                <FaIcon :name="action.icon" class="size-6" />
              </div>
              <span class="quick-action-label">{{ action.title }}</span>
            </div>
            <div
              v-if="filteredQuickActions.length === 0"
              class="quick-actions-empty"
            >
              <FaIcon name="i-heroicons-solid:lock-closed" class="size-8" />
              <p>暂无可用操作</p>
            </div>
          </div>
        </div>

        <!-- 系统信息 -->
        <div class="card system-info-card">
          <div class="card-header">
            <div class="card-title">
              <FaIcon name="i-heroicons-solid:information-circle" class="size-5 text-gray-500" />
              <span>系统信息</span>
            </div>
          </div>
          <div class="system-info-content">
            <div class="info-item">
              <span class="info-label">系统名称</span>
              <span class="info-value">XovoeJ 管理系统</span>
            </div>
            <div class="info-item">
              <span class="info-label">当前版本</span>
              <span class="info-value version">v1.0.0</span>
            </div>
            <div class="info-item">
              <span class="info-label">登录角色</span>
              <div class="role-tags">
                <span v-for="role in userStore.roles" :key="role" class="role-tag">{{ role }}</span>
              </div>
            </div>
            <div class="info-item">
              <span class="info-label">服务器时间</span>
              <span class="info-value time">{{ new Date().toLocaleString('zh-CN') }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.dashboard {
  padding: 0;
}

/* 欢迎区域 */
.welcome-section {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 24px;
  margin-bottom: 24px;
  background: linear-gradient(135deg, hsl(var(--primary)) 0%, hsl(var(--primary) / 0.8) 100%);
  border-radius: 16px;
  overflow: hidden;
}

.welcome-section::before {
  content: '';
  position: absolute;
  top: -50%;
  right: -10%;
  width: 400px;
  height: 400px;
  background: radial-gradient(circle, rgba(255, 255, 255, 0.1) 0%, transparent 70%);
  border-radius: 50%;
}

.welcome-content {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  position: relative;
  z-index: 1;
}

.welcome-greeting {
  flex: 1;
}

.welcome-title {
  font-size: 24px;
  font-weight: 700;
  color: white;
  margin: 0;
  display: flex;
  align-items: center;
  gap: 8px;
}

.welcome-emoji {
  font-size: 28px;
  animation: wave 2s ease-in-out infinite;
}

@keyframes wave {
  0%, 100% { transform: rotate(0deg); }
  25% { transform: rotate(20deg); }
  75% { transform: rotate(-20deg); }
}

.welcome-subtitle {
  margin-top: 8px;
  font-size: 14px;
  color: rgba(255, 255, 255, 0.85);
}

.welcome-date {
  display: flex;
  gap: 20px;
}

.date-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 16px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 10px;
  color: white;
  font-size: 14px;
  backdrop-filter: blur(10px);
}

.date-item .fa-icon {
  opacity: 0.9;
}

.welcome-decoration {
  position: absolute;
  right: 20px;
  top: 50%;
  transform: translateY(-50%);
}

.decoration-dot {
  position: absolute;
  width: 12px;
  height: 12px;
  background: rgba(255, 255, 255, 0.3);
  border-radius: 50%;
  animation: float 3s ease-in-out infinite;
}

.decoration-dot-1 { right: 0; animation-delay: 0s; }
.decoration-dot-2 { right: 20px; animation-delay: 0.5s; }
.decoration-dot-3 { right: 40px; animation-delay: 1s; }

@keyframes float {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-10px); }
}

/* 统计卡片 */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 20px;
  margin-bottom: 24px;
}

@media (max-width: 1024px) {
  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 640px) {
  .stats-grid {
    grid-template-columns: 1fr;
  }
}

.stat-card {
  position: relative;
  border-radius: 16px;
  padding: 20px;
  overflow: hidden;
  cursor: pointer;
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.stat-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 24px -10px rgba(0, 0, 0, 0.15);
}

.stat-card-bg {
  position: absolute;
  inset: 0;
  z-index: 0;
}

.stat-card-content {
  position: relative;
  display: flex;
  align-items: center;
  gap: 16px;
  z-index: 1;
}

.stat-icon {
  width: 56px;
  height: 56px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 14px;
  color: white;
}

.stat-info {
  flex: 1;
}

.stat-label {
  font-size: 13px;
  color: rgba(255, 255, 255, 0.85);
  margin-bottom: 4px;
}

.stat-value {
  margin: 0;
}

.stat-skeleton {
  display: inline-block;
  width: 60px;
  height: 28px;
  background: rgba(255, 255, 255, 0.3);
  border-radius: 4px;
  animation: pulse 1.5s ease-in-out infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

.stat-number {
  font-size: 26px;
  font-weight: 700;
  color: white;
  line-height: 1;
}

.stat-card-decoration {
  position: absolute;
  right: 12px;
  bottom: 12px;
  color: white;
  z-index: 0;
}

/* 渐变样式 */
.gradient-0 .stat-card-bg {
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
}

.gradient-1 .stat-card-bg {
  background: linear-gradient(135deg, #10b981 0%, #059669 100%);
}

.gradient-2 .stat-card-bg {
  background: linear-gradient(135deg, #f97316 0%, #ea580c 100%);
}

.gradient-3 .stat-card-bg {
  background: linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%);
}

/* 主网格 */
.main-grid {
  display: grid;
  grid-template-columns: 1fr 380px;
  gap: 24px;
}

@media (max-width: 1280px) {
  .main-grid {
    grid-template-columns: 1fr;
  }

  .right-section {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 24px;
  }

  .right-section > .card {
    grid-column: span 1;
  }
}

@media (max-width: 768px) {
  .right-section {
    grid-template-columns: 1fr;
  }

  .welcome-content {
    flex-direction: column;
    align-items: flex-start;
    gap: 16px;
  }
}

/* 左侧区域 */
.left-section {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

/* 卡片通用样式 */
.card {
  background: white;
  border-radius: 16px;
  border: 1px solid hsl(var(--border));
  overflow: hidden;
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  border-bottom: 1px solid hsl(var(--border) / 0.6);
  background: hsl(var(--muted) / 0.2);
}

.card-title {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 15px;
  font-weight: 600;
  color: hsl(var(--foreground));
}

.view-all-btn {
  font-size: 13px;
  color: hsl(var(--foreground) / 0.6);
  padding: 6px 12px;
}

.view-all-btn:hover {
  color: hsl(var(--primary));
  background: hsl(var(--primary) / 0.1);
}

/* 订单状态统计 */
.order-stats-grid {
  display: grid;
  grid-template-columns: repeat(6, 1fr);
  gap: 1px;
  background: hsl(var(--border) / 0.6);
}

@media (max-width: 768px) {
  .order-stats-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}

.order-stat-item {
  background: white;
  padding: 20px 16px;
  text-align: center;
  transition: background 0.2s;
}

.order-stat-item:hover {
  background: hsl(var(--muted) / 0.3);
}

.order-stat-value {
  font-size: 24px;
  font-weight: 700;
  color: hsl(var(--foreground));
  margin-bottom: 4px;
}

.order-stat-label {
  font-size: 12px;
  color: hsl(var(--foreground) / 0.5);
}

.stat-total .order-stat-value { color: #3b82f6; }
.stat-pendingPayment .order-stat-value { color: #f59e0b; }
.stat-pendingShipment .order-stat-value { color: #6366f1; }
.stat-shipped .order-stat-value { color: #8b5cf6; }
.stat-completed .order-stat-value { color: #10b981; }
.stat-cancelled .order-stat-value { color: #9ca3af; }

/* 用户统计 */
.user-stats-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
  padding: 20px;
}

@media (max-width: 768px) {
  .user-stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

.user-stat-card {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px;
  border-radius: 12px;
  border: 1px solid hsl(var(--border));
  transition: all 0.2s;
}

.user-stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px -4px rgba(0, 0, 0, 0.1);
}

.user-stat-card.primary {
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
}

.user-stat-card.primary .user-stat-icon,
.user-stat-card.primary .user-stat-value,
.user-stat-card.primary .user-stat-label {
  color: white;
}

.user-stat-card.success {
  background: linear-gradient(135deg, #10b981 0%, #059669 100%);
}

.user-stat-card.success .user-stat-icon,
.user-stat-card.success .user-stat-value,
.user-stat-card.success .user-stat-label {
  color: white;
}

.user-stat-card.info {
  background: linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%);
}

.user-stat-card.info .user-stat-icon,
.user-stat-card.info .user-stat-value,
.user-stat-card.info .user-stat-label {
  color: white;
}

.user-stat-card.warning {
  background: linear-gradient(135deg, #f97316 0%, #ea580c 100%);
}

.user-stat-card.warning .user-stat-icon,
.user-stat-card.warning .user-stat-value,
.user-stat-card.warning .user-stat-label {
  color: white;
}

.user-stat-icon {
  width: 44px;
  height: 44px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 10px;
  flex-shrink: 0;
}

.user-stat-value {
  font-size: 22px;
  font-weight: 700;
  line-height: 1;
}

.user-stat-label {
  font-size: 12px;
  opacity: 0.9;
  margin-top: 2px;
}

/* 最近订单 */
.recent-orders-content {
  padding: 16px 20px;
  min-height: 200px;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px 20px;
  color: hsl(var(--foreground) / 0.3);
}

.empty-state .fa-icon {
  margin-bottom: 12px;
}

.orders-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.order-item {
  display: grid;
  grid-template-columns: 1fr auto auto;
  gap: 16px;
  align-items: center;
  padding: 14px 16px;
  border-radius: 12px;
  border: 1px solid hsl(var(--border));
  transition: all 0.2s;
}

.order-item:hover {
  border-color: hsl(var(--primary) / 0.3);
  background: hsl(var(--muted) / 0.2);
}

.order-item-left {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.order-no {
  font-weight: 600;
  color: hsl(var(--foreground));
  font-size: 14px;
}

.order-user {
  font-size: 12px;
  color: hsl(var(--foreground) / 0.5);
}

.order-item-center {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
}

.order-status {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  font-weight: 500;
  padding: 4px 10px;
  border-radius: 20px;
}

.order-status.text-amber-600 { background: #fef3c7; }
.order-status.text-blue-600 { background: #dbeafe; }
.order-status.text-violet-600 { background: #ede9fe; }
.order-status.text-emerald-600 { background: #d1fae5; }
.order-status.text-gray-500 { background: #f3f4f6; }

.order-time {
  font-size: 11px;
  color: hsl(var(--foreground) / 0.4);
}

.order-item-right {
  text-align: right;
}

.order-amount {
  font-weight: 600;
  font-size: 16px;
  color: hsl(var(--primary));
}

/* 右侧区域 */
.right-section {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

/* 商品统计 */
.product-stats-content {
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.product-stat-item {
  padding: 12px;
  border-radius: 12px;
  border: 1px solid hsl(var(--border));
  transition: all 0.2s;
}

.product-stat-item:hover {
  border-color: hsl(var(--primary) / 0.3);
}

.product-stat-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 10px;
}

.product-stat-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  border-radius: 10px;
  color: white;
}

.product-stat-icon.blue { background: linear-gradient(135deg, #3b82f6, #2563eb); }
.product-stat-icon.green { background: linear-gradient(135deg, #10b981, #059669); }
.product-stat-icon.orange { background: linear-gradient(135deg, #f97316, #ea580c); }
.product-stat-icon.red { background: linear-gradient(135deg, #ef4444, #dc2626); }

.product-stat-info {
  flex: 1;
  text-align: right;
}

.product-stat-value {
  font-size: 20px;
  font-weight: 700;
  color: hsl(var(--foreground));
  line-height: 1;
}

.product-stat-label {
  font-size: 12px;
  color: hsl(var(--foreground) / 0.5);
  margin-top: 2px;
}

.product-stat-progress {
  margin-top: 8px;
}

.progress-bar {
  height: 6px;
  background: hsl(var(--muted) / 0.3);
  border-radius: 3px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  border-radius: 3px;
  transition: width 0.5s ease;
}

.progress-fill.blue { background: linear-gradient(90deg, #3b82f6, #2563eb); }
.progress-fill.green { background: linear-gradient(90deg, #10b981, #059669); }
.progress-fill.orange { background: linear-gradient(90deg, #f97316, #ea580c); }
.progress-fill.red { background: linear-gradient(90deg, #ef4444, #dc2626); }

/* 快捷操作 */
.quick-actions-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
  padding: 20px;
}

.quick-action-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 20px 16px;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.3s ease;
  color: white;
}

.quick-action-item:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 20px -6px rgba(0, 0, 0, 0.2);
}

.quick-action-item.from-blue-500 {
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
}

.quick-action-item.from-emerald-500 {
  background: linear-gradient(135deg, #10b981 0%, #059669 100%);
}

.quick-action-item.from-violet-500 {
  background: linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%);
}

.quick-action-item.from-orange-500 {
  background: linear-gradient(135deg, #f97316 0%, #ea580c 100%);
}

.quick-action-icon {
  width: 44px;
  height: 44px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 12px;
  margin-bottom: 10px;
}

.quick-action-label {
  font-size: 13px;
  font-weight: 500;
}

.quick-actions-empty {
  grid-column: span 2;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 30px;
  color: hsl(var(--foreground) / 0.3);
}

.quick-actions-empty .fa-icon {
  margin-bottom: 10px;
}

.quick-actions-empty p {
  font-size: 13px;
  margin: 0;
}

/* 系统信息 */
.system-info-content {
  padding: 16px 20px;
}

.info-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 0;
  border-bottom: 1px solid hsl(var(--border) / 0.5);
}

.info-item:last-child {
  border-bottom: none;
}

.info-label {
  font-size: 13px;
  color: hsl(var(--foreground) / 0.5);
}

.info-value {
  font-size: 13px;
  font-weight: 500;
  color: hsl(var(--foreground));
}

.info-value.version {
  background: linear-gradient(135deg, hsl(var(--primary)), hsl(var(--primary) / 0.7));
  color: white;
  padding: 4px 10px;
  border-radius: 20px;
  font-size: 12px;
}

.info-value.time {
  font-family: 'SF Mono', 'Monaco', 'Consolas', monospace;
}

.role-tags {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
  justify-content: flex-end;
}

.role-tag {
  display: inline-block;
  padding: 4px 10px;
  background: hsl(var(--primary) / 0.1);
  color: hsl(var(--primary));
  border-radius: 20px;
  font-size: 12px;
  font-weight: 500;
}
</style>
