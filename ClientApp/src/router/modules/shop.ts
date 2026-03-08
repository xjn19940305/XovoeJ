import type { Route } from '#/global'
import { permissionCodes } from '@/utils/permission'

function Layout() {
  return import('@/layouts/index.vue')
}

const shopMenuPermissions = [
  permissionCodes.admin.product.read,
  permissionCodes.admin.category.read,
  permissionCodes.admin.order.read,
  permissionCodes.admin.aftersale.read,
]

const shopRoute: Route.recordMainRaw = {
  meta: {
    title: '商城管理',
    icon: 'i-heroicons-solid:shopping-bag',
    auth: shopMenuPermissions,
  },
  children: [
    {
      path: '/shop',
      component: Layout(),
      name: 'Shop',
      redirect: '/shop/product',
      meta: {
        title: '商城管理',
        icon: 'i-heroicons-solid:shopping-bag',
        auth: shopMenuPermissions,
      },
      children: [
        {
          path: 'product',
          name: 'ShopProduct',
          component: () => import('@/views/shop/product/index.vue'),
          meta: {
            title: '商品管理',
            icon: 'i-heroicons-solid:cube',
            auth: [permissionCodes.admin.product.read],
          },
        },
        {
          path: 'product/create',
          name: 'ShopProductCreate',
          component: () => import('@/views/shop/product/form.vue'),
          meta: {
            title: '创建商品',
            auth: [permissionCodes.admin.product.create],
            menu: false,
          },
        },
        {
          path: 'product/:id/edit',
          name: 'ShopProductEdit',
          component: () => import('@/views/shop/product/form.vue'),
          meta: {
            title: '编辑商品',
            auth: [permissionCodes.admin.product.update],
            menu: false,
          },
        },
        {
          path: 'category',
          name: 'ShopProductCategory',
          component: () => import('@/views/shop/category/index.vue'),
          meta: {
            title: '商品分类',
            icon: 'i-heroicons-solid:tag',
            auth: [permissionCodes.admin.category.read],
          },
        },
        {
          path: 'order',
          name: 'ShopOrder',
          component: () => import('@/views/shop/order/index.vue'),
          meta: {
            title: '订单管理',
            icon: 'i-heroicons-solid:shopping-cart',
            auth: [permissionCodes.admin.order.read],
          },
        },
        {
          path: 'after-sale',
          name: 'ShopAfterSale',
          component: () => import('@/views/shop/after-sale/index.vue'),
          meta: {
            title: '售后管理',
            icon: 'i-heroicons-solid:arrow-uturn-left',
            auth: [permissionCodes.admin.aftersale.page, permissionCodes.admin.aftersale.read],
          },
        },
      ],
    },
  ],
}

export default shopRoute
