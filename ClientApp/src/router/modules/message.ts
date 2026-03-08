import type { Route } from '#/global'
import { permissionCodes } from '@/utils/permission'

function Layout() {
  return import('@/layouts/index.vue')
}

const messageMenuPermissions = [
  permissionCodes.admin.message,
  permissionCodes.admin.messageTemplate.page,
  permissionCodes.admin.messageTemplate.read,
  permissionCodes.admin.messageTask.page,
  permissionCodes.admin.messageTask.read,
  permissionCodes.admin.messageRecord.page,
  permissionCodes.admin.messageRecord.read,
]

const messageRoute: Route.recordMainRaw = {
  meta: {
    title: '消息中心',
    icon: 'i-heroicons-solid:chat-bubble-left-right',
    auth: messageMenuPermissions,
  },
  children: [
    {
      path: '/message',
      component: Layout(),
      name: 'Message',
      redirect: '/message/template',
      meta: {
        title: '消息中心',
        icon: 'i-heroicons-solid:chat-bubble-left-right',
        auth: messageMenuPermissions,
      },
      children: [
        {
          path: 'template',
          name: 'MessageTemplate',
          component: () => import('@/views/message/template/index.vue'),
          meta: {
            title: '消息模板',
            icon: 'i-heroicons-solid:envelope',
            auth: [permissionCodes.admin.messageTemplate.page, permissionCodes.admin.messageTemplate.read],
          },
        },
        {
          path: 'task',
          name: 'MessageTask',
          component: () => import('@/views/message/task/index.vue'),
          meta: {
            title: '消息任务',
            icon: 'i-heroicons-solid:paper-airplane',
            auth: [permissionCodes.admin.messageTask.page, permissionCodes.admin.messageTask.read],
          },
        },
        {
          path: 'record',
          name: 'MessageRecord',
          component: () => import('@/views/message/record/index.vue'),
          meta: {
            title: '发送记录',
            icon: 'i-heroicons-solid:clipboard-document-list',
            auth: [permissionCodes.admin.messageRecord.page, permissionCodes.admin.messageRecord.read],
          },
        },
      ],
    },
  ],
}

export default messageRoute
