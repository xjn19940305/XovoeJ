import type { Route } from '#/global'
import { permissionCodes } from '@/utils/permission'

function Layout() {
  return import('@/layouts/index.vue')
}

const systemMenuPermissions = [
  permissionCodes.admin.user.read,
  permissionCodes.admin.role.read,
  permissionCodes.admin.role.assignPermissions,
]

const systemRoute: Route.recordMainRaw = {
  meta: {
    title: '权限管理',
    icon: 'i-heroicons-solid:shield-check',
    auth: systemMenuPermissions,
  },
  children: [
    {
      path: '/system',
      component: Layout(),
      name: 'System',
      redirect: '/system/user',
      meta: {
        title: '权限管理',
        icon: 'i-heroicons-solid:shield-check',
        auth: systemMenuPermissions,
      },
      children: [
        {
          path: 'user',
          name: 'SystemUser',
          component: () => import('@/views/system/user/index.vue'),
          meta: {
            title: '用户管理',
            icon: 'i-heroicons-solid:users',
            auth: [permissionCodes.admin.user.read],
          },
        },
        {
          path: 'role',
          name: 'SystemRole',
          component: () => import('@/views/system/role/index.vue'),
          meta: {
            title: '角色管理',
            icon: 'i-heroicons-solid:identification',
            auth: [permissionCodes.admin.role.read],
          },
        },
        {
          path: 'role-auth',
          name: 'SystemRoleAuth',
          component: () => import('@/views/system/role-auth/index.vue'),
          meta: {
            title: '角色授权',
            icon: 'i-heroicons-solid:key',
            auth: [permissionCodes.admin.role.assignPermissions],
          },
        },
      ],
    },
  ],
}

export default systemRoute
