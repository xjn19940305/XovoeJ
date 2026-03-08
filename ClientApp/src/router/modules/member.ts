import type { Route } from '#/global'

function Layout() {
  return import('@/layouts/index.vue')
}

const memberRoute: Route.recordMainRaw = {
  meta: {
    title: '会员中心',
    icon: 'i-heroicons-solid:user-circle',
  },
  children: [
    {
      path: '/member',
      component: Layout(),
      name: 'Member',
      redirect: '/member/account',
      meta: {
        title: '会员中心',
        icon: 'i-heroicons-solid:user-circle',
      },
      children: [
        {
          path: 'account',
          name: 'MemberAccount',
          component: () => import('@/views/member/account/index.vue'),
          meta: {
            title: '会员概览',
            icon: 'i-heroicons-solid:identification',
          },
        },
        {
          path: 'coupon',
          name: 'MemberCoupon',
          component: () => import('@/views/member/coupon/index.vue'),
          meta: {
            title: '我的优惠券',
            icon: 'i-heroicons-solid:ticket',
          },
        },
      ],
    },
  ],
}

export default memberRoute
