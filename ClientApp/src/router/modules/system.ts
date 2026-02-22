import type { Route } from '#/global'

function Layout() {
  return import('@/layouts/index.vue')
}

// 权限管理路由
const systemRoute: Route.recordMainRaw = {
  meta: {
    title: '权限管理',
    icon: 'i-heroicons-solid:shield-check',
    // 只要有任何一个子权限就能看到菜单
    auth: ['system:view', 'system:user:view', 'system:role:view', 'system:role:assign-permissions'],
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
        // 只要有任何一个子权限就能看到菜单
        auth: ['system:view', 'system:user:view', 'system:role:view', 'system:role:assign-permissions'],
      },
      children: [
        {
          path: 'user',
          name: 'SystemUser',
          component: () => import('@/views/system/user/index.vue'),
          meta: {
            title: '用户管理',
            icon: 'i-heroicons-solid:users',
            auth: ['system:user:view'],
          },
        },
        {
          path: 'role',
          name: 'SystemRole',
          component: () => import('@/views/system/role/index.vue'),
          meta: {
            title: '角色管理',
            icon: 'i-heroicons-solid:identification',
            auth: ['system:role:view'],
          },
        },
        {
          path: 'role-auth',
          name: 'SystemRoleAuth',
          component: () => import('@/views/system/role-auth/index.vue'),
          meta: {
            title: '角色授权',
            icon: 'i-heroicons-solid:key',
            auth: ['system:role:assign-permissions'],
          },
        },
      ],
    },
  ],
}

export default systemRoute
