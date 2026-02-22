import type { Route } from '#/global'

function Layout() {
  return import('@/layouts/index.vue')
}

// 商城管理路由
const shopRoute: Route.recordMainRaw = {
  meta: {
    title: '商城管理',
    icon: 'i-heroicons-solid:shopping-bag',
    // 只要有任何一个子权限就能看到菜单
    auth: ['shop:view', 'shop:product:view', 'shop:category:view', 'shop:order:view'],
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
        // 只要有任何一个子权限就能看到菜单
        auth: ['shop:view', 'shop:product:view', 'shop:category:view', 'shop:order:view'],
      },
      children: [
        {
          path: 'product',
          name: 'ShopProduct',
          component: () => import('@/views/shop/product/index.vue'),
          meta: {
            title: '商品管理',
            icon: 'i-heroicons-solid:cube',
            auth: ['shop:product:view'],
          },
        },
        {
          path: 'product/create',
          name: 'ShopProductCreate',
          component: () => import('@/views/shop/product/form.vue'),
          meta: {
            title: '创建商品',
            auth: ['shop:product:create'],
            menu: false,
          },
        },
        {
          path: 'product/:id/edit',
          name: 'ShopProductEdit',
          component: () => import('@/views/shop/product/form.vue'),
          meta: {
            title: '编辑商品',
            auth: ['shop:product:update'],
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
            auth: ['shop:category:view'],
          },
        },
        {
          path: 'order',
          name: 'ShopOrder',
          component: () => import('@/views/shop/order/index.vue'),
          meta: {
            title: '订单管理',
            icon: 'i-heroicons-solid:shopping-cart',
            auth: ['shop:order:view'],
          },
        },
      ],
    },
  ],
}

export default shopRoute
