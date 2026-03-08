import type { Route } from '#/global'
import { permissionCodes } from '@/utils/permission'

function Layout() {
  return import('@/layouts/index.vue')
}

const marketingMenuPermissions = [
  permissionCodes.admin.marketing,
  permissionCodes.admin.coupon.page,
  permissionCodes.admin.coupon.read,
  permissionCodes.admin.promotion.page,
  permissionCodes.admin.promotion.read,
  permissionCodes.admin.seckill.page,
  permissionCodes.admin.seckill.manage,
  permissionCodes.admin.groupBuy.page,
  permissionCodes.admin.groupBuy.manage,
  permissionCodes.admin.bargain.page,
  permissionCodes.admin.bargain.manage,
]

const marketingRoute: Route.recordMainRaw = {
  meta: {
    title: '营销中心',
    icon: 'i-heroicons-solid:megaphone',
    auth: marketingMenuPermissions,
  },
  children: [
    {
      path: '/marketing',
      component: Layout(),
      name: 'Marketing',
      redirect: '/marketing/coupon',
      meta: {
        title: '营销中心',
        icon: 'i-heroicons-solid:megaphone',
        auth: marketingMenuPermissions,
      },
      children: [
        {
          path: 'coupon',
          name: 'MarketingCoupon',
          component: () => import('@/views/marketing/coupon/index.vue'),
          meta: {
            title: '优惠券中心',
            icon: 'i-heroicons-solid:ticket',
            auth: [permissionCodes.admin.coupon.page, permissionCodes.admin.coupon.read],
          },
        },
        {
          path: 'promotion',
          name: 'MarketingPromotion',
          component: () => import('@/views/marketing/promotion/index.vue'),
          meta: {
            title: '营销活动',
            icon: 'i-heroicons-solid:gift-top',
            auth: [permissionCodes.admin.promotion.page, permissionCodes.admin.promotion.read],
          },
        },
        {
          path: 'advanced',
          name: 'MarketingAdvanced',
          component: () => import('@/views/marketing/advanced/index.vue'),
          meta: {
            title: '高级营销',
            icon: 'i-heroicons-solid:bolt',
            auth: [
              permissionCodes.admin.seckill.page,
              permissionCodes.admin.seckill.manage,
              permissionCodes.admin.groupBuy.page,
              permissionCodes.admin.groupBuy.manage,
              permissionCodes.admin.bargain.page,
              permissionCodes.admin.bargain.manage,
            ],
          },
        },
      ],
    },
  ],
}

export default marketingRoute
