<script setup lang="ts">
import mallAccountApi from '@/api/modules/mall-account'
import AdminMetricCard from '@/components/admin/AdminMetricCard.vue'

defineOptions({
  name: 'MemberAccountPage',
})

const loading = ref(false)
const summary = ref<Api.Mall.AccountSummary | null>(null)

const statCards = computed(() => {
  const data = summary.value
  if (!data) {
    return []
  }

  return [
    {
      title: '累计消费',
      value: `¥ ${data.totalSpent.toFixed(2)}`,
      icon: 'i-heroicons-solid:banknotes',
      tone: 'emerald' as const,
      description: '会员累计消费金额，反映当前客户价值。',
    },
    {
      title: '全部订单',
      value: data.totalOrders,
      icon: 'i-heroicons-solid:shopping-bag',
      tone: 'sky' as const,
      description: '历史全部订单量，用于观察整体活跃度。',
    },
    {
      title: '待处理订单',
      value: data.pendingOrders + data.shippedOrders,
      icon: 'i-heroicons-solid:clock',
      tone: 'amber' as const,
      description: '待付款、待收货等处理中订单汇总。',
    },
    {
      title: '可用优惠券',
      value: data.availableCouponCount,
      icon: 'i-heroicons-solid:ticket',
      tone: 'blue' as const,
      description: '当前可直接在结算时使用的优惠券数量。',
    },
  ]
})

async function getSummary() {
  loading.value = true
  try {
    const res = await mallAccountApi.getSummary()
    summary.value = res.data
  }
  finally {
    loading.value = false
  }
}

onMounted(() => {
  getSummary()
})
</script>

<template>
  <div class="member-account-page">
    <FaCard class="mb-4 member-hero">
      <div v-loading="loading" class="member-hero__content">
        <template v-if="summary">
          <div class="member-hero__identity">
            <div class="member-hero__avatar">
              {{ (summary.nickName || summary.realName || summary.phoneNumber || '会').slice(0, 1) }}
            </div>
            <div class="space-y-2">
              <p class="member-hero__eyebrow">
                商城会员
              </p>
              <h2>{{ summary.nickName || summary.realName || summary.phoneNumber || '未命名用户' }}</h2>
              <p class="member-hero__meta">
                {{ summary.memberLevel }} · {{ summary.phoneNumber || '未绑定手机号' }}
              </p>
            </div>
          </div>

          <div class="member-hero__actions">
            <RouterLink to="/member/coupon">
              <FaButton>
                <template #icon>
                  <FaIcon name="i-heroicons-solid:ticket" />
                </template>
                查看我的优惠券
              </FaButton>
            </RouterLink>
          </div>
        </template>
      </div>
    </FaCard>

    <div class="grid mb-4 gap-4 md:grid-cols-2 xl:grid-cols-4">
      <AdminMetricCard
        v-for="card in statCards"
        :key="card.title"
        :title="card.title"
        :value="card.value"
        :description="card.description"
        :icon="card.icon"
        :tone="card.tone"
        variant="board"
      />
    </div>

    <div class="grid gap-4 xl:grid-cols-[1.4fr_1fr]">
      <FaCard v-loading="loading">
        <template #header>
          <div class="flex items-center gap-2">
            <FaIcon name="i-heroicons-solid:archive-box" class="size-5 text-primary" />
            <span class="font-medium">订单概览</span>
          </div>
        </template>

        <div v-if="summary" class="order-overview">
          <div class="order-overview__item">
            <span>待付款</span>
            <strong>{{ summary.pendingOrders }}</strong>
          </div>
          <div class="order-overview__item">
            <span>待收货</span>
            <strong>{{ summary.shippedOrders }}</strong>
          </div>
          <div class="order-overview__item">
            <span>已完成</span>
            <strong>{{ summary.completedOrders }}</strong>
          </div>
        </div>
      </FaCard>

      <FaCard v-loading="loading">
        <template #header>
          <div class="flex items-center gap-2">
            <FaIcon name="i-heroicons-solid:map-pin" class="size-5 text-primary" />
            <span class="font-medium">默认收货地址</span>
          </div>
        </template>

        <div v-if="summary?.defaultAddress" class="space-y-3 text-sm text-stone-600">
          <div class="flex items-center justify-between gap-3">
            <strong class="text-base text-stone-900">{{ summary.defaultAddress.consigneeName }}</strong>
            <span class="rounded-full bg-primary/10 px-3 py-1 text-xs text-primary">
              {{ summary.defaultAddress.label || '默认地址' }}
            </span>
          </div>
          <p>{{ summary.defaultAddress.mobile }}</p>
          <p class="leading-6">
            {{ summary.defaultAddress.fullAddress }}
          </p>
        </div>
        <el-empty v-else description="还没有可用的收货地址" />
      </FaCard>
    </div>
  </div>
</template>

<style scoped>
.member-hero {
  position: relative;
  overflow: hidden;
}

.member-hero::before {
  position: absolute;
  inset: 0;
  background:
    radial-gradient(circle at top left, rgb(56 189 248 / 0.18), transparent 32%),
    radial-gradient(circle at bottom right, rgb(16 185 129 / 0.14), transparent 28%);
  content: '';
}

.member-hero__content {
  position: relative;
  display: flex;
  justify-content: space-between;
  gap: 24px;
  align-items: center;
}

.member-hero__identity {
  display: flex;
  gap: 18px;
  align-items: center;
}

.member-hero__avatar {
  display: flex;
  width: 72px;
  height: 72px;
  align-items: center;
  justify-content: center;
  border-radius: 24px;
  background: linear-gradient(135deg, rgb(14 165 233), rgb(45 212 191));
  color: white;
  font-size: 28px;
  font-weight: 700;
}

.member-hero__eyebrow {
  color: rgb(8 145 178);
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.14em;
  text-transform: uppercase;
}

.member-hero__identity h2 {
  margin: 0;
  color: rgb(28 25 23);
  font-size: 28px;
  font-weight: 700;
}

.member-hero__meta {
  color: rgb(87 83 78);
  font-size: 14px;
}

.order-overview {
  display: grid;
  gap: 12px;
}

.order-overview__item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border: 1px solid rgb(231 229 228);
  border-radius: 16px;
  padding: 14px 16px;
}

.order-overview__item span {
  color: rgb(87 83 78);
  font-size: 14px;
}

.order-overview__item strong {
  color: rgb(28 25 23);
  font-size: 20px;
}

@media (max-width: 960px) {
  .member-hero__content,
  .member-hero__identity {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>
