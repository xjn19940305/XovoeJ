import type { Route } from '#/global'

function Layout() {
  return import('@/layouts/index.vue')
}

// 内容管理路由
const contentRoute: Route.recordMainRaw = {
  meta: {
    title: '内容管理',
    icon: 'i-heroicons-solid:document-text',
    // 只要有任何一个子权限就能看到菜单
    auth: ['content:view', 'content:banner:view', 'content:dict:view'],
  },
  children: [
    {
      path: '/content',
      component: Layout(),
      name: 'Content',
      redirect: '/content/banner',
      meta: {
        title: '内容管理',
        icon: 'i-heroicons-solid:document-text',
        // 只要有任何一个子权限就能看到菜单
        auth: ['content:view', 'content:banner:view'],
      },
      children: [
        {
          path: 'banner',
          name: 'ContentBanner',
          component: () => import('@/views/content/banner/index.vue'),
          meta: {
            title: '轮播图管理',
            icon: 'i-heroicons-solid:photo',
            auth: ['content:banner:view'],
          },
        },
        {
          path: 'dict',
          name: 'ContentDict',
          component: () => import('@/views/content/dict/index.vue'),
          meta: {
            title: '字典管理',
            icon: 'i-heroicons-solid:book-open',
            auth: ['content:dict:view'],
          },
        },
      ],
    },
  ],
}

export default contentRoute
