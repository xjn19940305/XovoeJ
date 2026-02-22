<route lang="yaml">
meta:
  title: 主页
  icon: ant-design:home-twotone
</route>

<script setup lang="ts">
import { ElMessage } from 'element-plus'
import dashboardApi from '@/api/modules/dashboard'

defineOptions({
  name: 'Dashboard',
})

// 统计卡片数据
const statsCards = ref([
  {
    title: '今日订单',
    value: 0,
    icon: 'i-heroicons-solid:shopping-cart',
    color: 'bg-blue-500',
    textColor: 'text-blue-500',
    bgColor: 'bg-blue-50',
    loading: true,
  },
  {
    title: '今日用户',
    value: 0,
    icon: 'i-heroicons-solid:users',
    color: 'bg-green-500',
    textColor: 'text-green-500',
    bgColor: 'bg-green-50',
    loading: true,
  },
  {
    title: '今日销售额',
    value: 0,
    icon: 'i-heroicons-solid:currency-dollar',
    color: 'bg-orange-500',
    textColor: 'text-orange-500',
    bgColor: 'bg-orange-50',
    loading: true,
    prefix: '¥',
  },
  {
    title: '本月销售额',
    value: 0,
    icon: 'i-heroicons-solid:chart-bar',
    color: 'bg-purple-500',
    textColor: 'text-purple-500',
    bgColor: 'bg-purple-50',
    loading: true,
    prefix: '¥',
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
  { title: '创建订单', icon: 'i-heroicons-solid:plus', route: '/shop/order', auth: ['shop:order:create'] },
  { title: '添加商品', icon: 'i-heroicons-solid:plus-circle', route: '/shop/product', auth: ['shop:product:create'] },
  { title: '添加用户', icon: 'i-heroicons-solid:user-plus', route: '/system/user', auth: ['system:user:create'] },
  { title: '创建角色', icon: 'i-heroicons-solid:identification', route: '/system/role', auth: ['system:role:create'] },
]

const userStore = useUserStore()

// 过滤有权限的快捷操作
const filteredQuickActions = computed(() => {
  return quickActions.filter(action => {
    if (!action.auth || action.auth.length === 0)
      return true
    return action.auth.some(permission => userStore.permissions.includes(permission))
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
  catch (error) {
    // 使用模拟数据
    statsCards.value.forEach(card => {
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
    recentOrders.value = res.data.slice(0, 5)
  }
  catch {
    recentOrders.value = []
  }
  finally {
    recentOrdersLoading.value = false
  }
}

// 订单状态映射
const orderStatusMap: Record<number, { label: string; type: any }> = {
  0: { label: '待付款', type: 'warning' },
  1: { label: '待发货', type: 'primary' },
  2: { label: '待收货', type: 'info' },
  3: { label: '已完成', type: 'success' },
  4: { label: '已取消', type: 'danger' },
}

// 格式化金额
function formatAmount(amount: number) {
  return `¥${amount.toFixed(2)}`
}

// 获取问候语
const greeting = computed(() => {
  const hour = new Date().getHours()
  if (hour < 6)
    return '凌晨好'
  if (hour < 9)
    return '早上好'
  if (hour < 12)
    return '上午好'
  if (hour < 14)
    return '中午好'
  if (hour < 18)
    return '下午好'
  if (hour < 22)
    return '晚上好'
  return '夜深了'
})

onMounted(() => {
  getStatsData()
  getRecentOrders()
})
</script>

<template>
  <div class="dashboard">
    <!-- 欢迎区域 -->
    <div class="mb-6">
      <h1 class="text-2xl font-bold text-gray-800">
        {{ greeting }}，{{ userStore.nickName || userStore.userName }}！
      </h1>
      <p class="text-gray-500 mt-1">欢迎回来，这是您的数据概览</p>
    </div>

    <!-- 统计卡片 -->
    <div class="grid grid-cols-1 gap-4 mb-6 sm:grid-cols-2 lg:grid-cols-4">
      <div
        v-for="(card, index) in statsCards"
        :key="index"
        class="rounded-xl border border-gray-200 bg-white p-6 shadow-sm transition-all hover:shadow-md"
      >
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-gray-500">{{ card.title }}</p>
            <p class="mt-2 text-2xl font-bold" :class="card.textColor">
              <span v-if="card.loading">--</span>
              <span v-else>{{ card.prefix }}{{ card.value.toLocaleString() }}</span>
            </p>
          </div>
          <div :class="[card.color, 'rounded-lg p-3']">
            <FaIcon :name="card.icon" class="size-6 text-white" />
          </div>
        </div>
      </div>
    </div>

    <div class="grid grid-cols-1 gap-6 lg:grid-cols-3">
      <!-- 左侧：订单统计 + 用户统计 -->
      <div class="space-y-6 lg:col-span-2">
        <!-- 订单状态统计 -->
        <FaCard>
          <template #header>
            <div class="flex items-center justify-between">
              <span class="font-medium">订单状态统计</span>
              <FaButton variant="ghost" size="sm" @click="handleNavigate('/shop/order')">
                查看全部
                <FaIcon name="i-heroicons-solid:arrow-right" class="ml-1" />
              </FaButton>
            </div>
          </template>
          <div v-loading="orderStats.loading" class="grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-6">
            <div class="text-center">
              <div class="text-2xl font-bold text-gray-800">{{ orderStats.total }}</div>
              <div class="mt-1 text-xs text-gray-500">总订单</div>
            </div>
            <div class="text-center">
              <div class="text-2xl font-bold text-orange-500">{{ orderStats.pendingPayment }}</div>
              <div class="mt-1 text-xs text-gray-500">待付款</div>
            </div>
            <div class="text-center">
              <div class="text-2xl font-bold text-blue-500">{{ orderStats.pendingShipment }}</div>
              <div class="mt-1 text-xs text-gray-500">待发货</div>
            </div>
            <div class="text-center">
              <div class="text-2xl font-bold text-purple-500">{{ orderStats.shipped }}</div>
              <div class="mt-1 text-xs text-gray-500">已发货</div>
            </div>
            <div class="text-center">
              <div class="text-2xl font-bold text-green-500">{{ orderStats.completed }}</div>
              <div class="mt-1 text-xs text-gray-500">已完成</div>
            </div>
            <div class="text-center">
              <div class="text-2xl font-bold text-gray-400">{{ orderStats.cancelled }}</div>
              <div class="mt-1 text-xs text-gray-500">已取消</div>
            </div>
          </div>
        </FaCard>

        <!-- 用户统计 -->
        <FaCard>
          <template #header>
            <div class="flex items-center justify-between">
              <span class="font-medium">用户统计</span>
              <FaButton variant="ghost" size="sm" @click="handleNavigate('/system/user')">
                查看全部
                <FaIcon name="i-heroicons-solid:arrow-right" class="ml-1" />
              </FaButton>
            </div>
          </template>
          <div v-loading="userStats.loading" class="grid grid-cols-2 gap-4 sm:grid-cols-4">
            <div class="rounded-lg border border-gray-100 p-4 text-center">
              <div class="text-2xl font-bold text-gray-800">{{ userStats.total }}</div>
              <div class="mt-1 text-sm text-gray-500">总用户数</div>
            </div>
            <div class="rounded-lg border border-gray-100 p-4 text-center">
              <div class="text-2xl font-bold text-blue-500">{{ userStats.today }}</div>
              <div class="mt-1 text-sm text-gray-500">今日新增</div>
            </div>
            <div class="rounded-lg border border-gray-100 p-4 text-center">
              <div class="text-2xl font-bold text-green-500">{{ userStats.thisWeek }}</div>
              <div class="mt-1 text-sm text-gray-500">本周新增</div>
            </div>
            <div class="rounded-lg border border-gray-100 p-4 text-center">
              <div class="text-2xl font-bold text-purple-500">{{ userStats.thisMonth }}</div>
              <div class="mt-1 text-sm text-gray-500">本月新增</div>
            </div>
          </div>
        </FaCard>

        <!-- 最近订单 -->
        <FaCard>
          <template #header>
            <div class="flex items-center justify-between">
              <span class="font-medium">最近订单</span>
              <FaButton variant="ghost" size="sm" @click="handleNavigate('/shop/order')">
                查看全部
                <FaIcon name="i-heroicons-solid:arrow-right" class="ml-1" />
              </FaButton>
            </div>
          </template>
          <div v-loading="recentOrdersLoading">
            <div v-if="recentOrders.length === 0" class="py-8 text-center text-gray-400">
              暂无订单数据
            </div>
            <div v-else class="space-y-3">
              <div
                v-for="order in recentOrders"
                :key="order.id"
                class="flex items-center justify-between rounded-lg border border-gray-100 p-3 hover:bg-gray-50"
              >
                <div class="flex-1">
                  <div class="flex items-center gap-2">
                    <span class="font-medium text-gray-800">{{ order.orderNo }}</span>
                    <el-tag :type="orderStatusMap[order.status]?.type" size="small">
                      {{ orderStatusMap[order.status]?.label }}
                    </el-tag>
                  </div>
                  <div class="mt-1 text-sm text-gray-500">
                    {{ order.userName || '未知用户' }} · {{ order.createdAt }}
                  </div>
                </div>
                <div class="text-right">
                  <div class="font-medium text-gray-800">{{ formatAmount(order.totalAmount) }}</div>
                </div>
              </div>
            </div>
          </div>
        </FaCard>
      </div>

      <!-- 右侧：商品统计 + 快捷操作 -->
      <div class="space-y-6">
        <!-- 商品统计 -->
        <FaCard>
          <template #header>
            <div class="flex items-center justify-between">
              <span class="font-medium">商品统计</span>
              <FaButton variant="ghost" size="sm" @click="handleNavigate('/shop/product')">
                查看全部
                <FaIcon name="i-heroicons-solid:arrow-right" class="ml-1" />
              </FaButton>
            </div>
          </template>
          <div v-loading="productStats.loading">
            <div class="space-y-4">
              <div class="flex items-center justify-between rounded-lg border border-gray-100 p-4">
                <div class="flex items-center gap-3">
                  <div class="rounded-lg bg-blue-100 p-2">
                    <FaIcon name="i-heroicons-solid:cube" class="size-5 text-blue-500" />
                  </div>
                  <span class="text-gray-700">商品总数</span>
                </div>
                <span class="text-xl font-bold text-gray-800">{{ productStats.total }}</span>
              </div>
              <div class="flex items-center justify-between rounded-lg border border-gray-100 p-4">
                <div class="flex items-center gap-3">
                  <div class="rounded-lg bg-green-100 p-2">
                    <FaIcon name="i-heroicons-solid:check-circle" class="size-5 text-green-500" />
                  </div>
                  <span class="text-gray-700">在售商品</span>
                </div>
                <span class="text-xl font-bold text-green-500">{{ productStats.onSale }}</span>
              </div>
              <div class="flex items-center justify-between rounded-lg border border-gray-100 p-4">
                <div class="flex items-center gap-3">
                  <div class="rounded-lg bg-red-100 p-2">
                    <FaIcon name="i-heroicons-solid:x-circle" class="size-5 text-red-500" />
                  </div>
                  <span class="text-gray-700">缺货商品</span>
                </div>
                <span class="text-xl font-bold text-red-500">{{ productStats.outOfStock }}</span>
              </div>
            </div>
          </div>
        </FaCard>

        <!-- 快捷操作 -->
        <FaCard>
          <template #header>
            <span class="font-medium">快捷操作</span>
          </template>
          <div class="grid grid-cols-2 gap-3">
            <div
              v-for="action in filteredQuickActions"
              :key="action.title"
              class="flex cursor-pointer flex-col items-center justify-center rounded-lg border border-gray-200 p-4 transition-all hover:border-blue-300 hover:bg-blue-50"
              @click="handleNavigate(action.route)"
            >
              <FaIcon :name="action.icon" class="size-8 text-blue-500" />
              <span class="mt-2 text-sm text-gray-700">{{ action.title }}</span>
            </div>
            <div
              v-if="filteredQuickActions.length === 0"
              class="col-span-2 py-8 text-center text-sm text-gray-400"
            >
              暂无可用操作
            </div>
          </div>
        </FaCard>

        <!-- 系统信息 -->
        <FaCard>
          <template #header>
            <span class="font-medium">系统信息</span>
          </template>
          <div class="space-y-3 text-sm">
            <div class="flex justify-between">
              <span class="text-gray-500">系统名称</span>
              <span class="text-gray-800">XovoeJ 管理系统</span>
            </div>
            <div class="flex justify-between">
              <span class="text-gray-500">当前版本</span>
              <span class="text-gray-800">v1.0.0</span>
            </div>
            <div class="flex justify-between">
              <span class="text-gray-500">登录角色</span>
              <span class="text-gray-800">{{ userStore.roles.join('、') }}</span>
            </div>
            <div class="flex justify-between">
              <span class="text-gray-500">服务器时间</span>
              <span class="text-gray-800">{{ new Date().toLocaleString('zh-CN') }}</span>
            </div>
          </div>
        </FaCard>
      </div>
    </div>
  </div>
</template>

<style scoped>
.dashboard {
  padding: 0;
}
</style>
