import type { Route } from '#/global'
import { permissionCodes } from '@/utils/permission'

function Layout() {
  return import('@/layouts/index.vue')
}

const growthMenuPermissions = [
  permissionCodes.admin.growth,
  permissionCodes.admin.distribution.page,
  permissionCodes.admin.distribution.read,
  permissionCodes.admin.referralLink.page,
  permissionCodes.admin.referralLink.read,
  permissionCodes.admin.commission.page,
  permissionCodes.admin.commission.read,
]

const growthRoute: Route.recordMainRaw = {
  meta: {
    title: '增长中心',
    icon: 'i-heroicons-solid:chart-bar',
    auth: growthMenuPermissions,
  },
  children: [
    {
      path: '/growth',
      component: Layout(),
      name: 'Growth',
      redirect: '/growth/distribution',
      meta: {
        title: '增长中心',
        icon: 'i-heroicons-solid:chart-bar',
        auth: growthMenuPermissions,
      },
      children: [
        {
          path: 'distribution',
          name: 'GrowthDistribution',
          component: () => import('@/views/growth/referral/index.vue'),
          meta: {
            title: '分销与邀请',
            icon: 'i-heroicons-solid:user-plus',
            auth: [permissionCodes.admin.distribution.page, permissionCodes.admin.distribution.read],
          },
        },
        {
          path: 'link',
          name: 'GrowthReferralLink',
          component: () => import('@/views/growth/link/index.vue'),
          meta: {
            title: '推广链接',
            icon: 'i-heroicons-solid:share',
            auth: [permissionCodes.admin.referralLink.page, permissionCodes.admin.referralLink.read],
          },
        },
        {
          path: 'commission',
          name: 'GrowthCommission',
          component: () => import('@/views/growth/commission/index.vue'),
          meta: {
            title: '佣金结算',
            icon: 'i-heroicons-solid:banknotes',
            auth: [permissionCodes.admin.commission.page, permissionCodes.admin.commission.read],
          },
        },
      ],
    },
  ],
}

export default growthRoute
