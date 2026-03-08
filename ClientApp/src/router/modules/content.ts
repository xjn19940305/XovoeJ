import type { Route } from '#/global'
import { permissionCodes } from '@/utils/permission'

function Layout() {
  return import('@/layouts/index.vue')
}

const contentMenuPermissions = [
  permissionCodes.admin.banner.read,
  permissionCodes.admin.dictionary.read,
  permissionCodes.admin.workflow.read,
]

const contentRoute: Route.recordMainRaw = {
  meta: {
    title: '内容管理',
    icon: 'i-heroicons-solid:document-text',
    auth: contentMenuPermissions,
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
        auth: contentMenuPermissions,
      },
      children: [
        {
          path: 'banner',
          name: 'ContentBanner',
          component: () => import('@/views/content/banner/index.vue'),
          meta: {
            title: '轮播图管理',
            icon: 'i-heroicons-solid:photo',
            auth: [permissionCodes.admin.banner.read],
          },
        },
        {
          path: 'dict',
          name: 'ContentDict',
          component: () => import('@/views/content/dict/index.vue'),
          meta: {
            title: '字典管理',
            icon: 'i-heroicons-solid:book-open',
            auth: [permissionCodes.admin.dictionary.read],
          },
        },
        {
          path: 'workflow',
          name: 'ContentWorkflow',
          component: () => import('@/views/content/workflow/index.vue'),
          meta: {
            title: '工作流配置',
            icon: 'i-heroicons-solid:academic-cap',
            auth: [permissionCodes.admin.workflow.read],
          },
        },
      ],
    },
  ],
}

export default contentRoute
