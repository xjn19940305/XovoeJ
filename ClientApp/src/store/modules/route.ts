import type { Route } from '#/global'
import type { RouteRecordRaw, RouterMatcher } from 'vue-router'
import { cloneDeep } from 'es-toolkit'
import { createRouterMatcher } from 'vue-router'
import { systemRoutes as systemRoutesRaw } from '@/router/routes'
import { hasAnyPermission } from '@/utils/permission'

export const useRouteStore = defineStore(
  // 唯一ID
  'route',
  () => {
    const userStore = useUserStore()

    const isGenerate = ref(false)
    // 原始路由
    const routesRaw = ref<Route.recordMainRaw[]>([])
    // 已注册的路由，用于登出时删除路由
    const currentRemoveRoutes = ref<(() => void)[]>([])

    // 检查是否有权限访问该路由
    function hasPermission(auth?: string | string[]): boolean {
      if (!auth) {
        return true
      }

      const userRoles = userStore.roles || []
      const authList = Array.isArray(auth) ? auth : [auth]

      // 如果用户有超级管理员角色，拥有所有权限
      if (userRoles.includes('超级管理员')) {
        return true
      }

      // 其他用户检查权限
      const userPermissions = userStore.permissions || []
      return hasAnyPermission(userPermissions, authList)
    }

    // 递归过滤路由
    function filterRoutesByPermission(routes: RouteRecordRaw[]): RouteRecordRaw[] {
      const filteredRoutes: RouteRecordRaw[] = []

      for (const route of routes) {
        // 克隆路由以避免修改原始数据
        const routeClone = cloneDeep(route)

        // 检查是否有权限访问此路由
        if (hasPermission(routeClone.meta?.auth)) {
          // 如果有子路由，递归过滤
          if (routeClone.children && routeClone.children.length > 0) {
            const filteredChildren = filterRoutesByPermission(routeClone.children)
            if (filteredChildren.length > 0) {
              routeClone.children = filteredChildren
              filteredRoutes.push(routeClone)
            }
          }
          else {
            filteredRoutes.push(routeClone)
          }
        }
      }

      return filteredRoutes
    }

    // 实际路由
    const routes = computed(() => {
      const returnRoutes: RouteRecordRaw[] = []
      if (routesRaw.value) {
        routesRaw.value.forEach((item) => {
          const tmpRoutes = cloneDeep(item.children) as RouteRecordRaw[]
          tmpRoutes.map((v) => {
            if (!v.meta) {
              v.meta = {}
            }
            v.meta.auth = item.meta?.auth ?? v.meta?.auth
            return v
          })
          const filteredRoutes = filterRoutesByPermission(tmpRoutes)
          returnRoutes.push(...filteredRoutes)
        })
        returnRoutes.forEach((item) => {
          if (item.children) {
            item.children = deleteMiddleRouteComponent(item.children)
          }
          return item
        })
      }
      return returnRoutes
    })

    // 系统路由
    const systemRoutes = computed(() => {
      const routes = [...systemRoutesRaw]
      routes.forEach((item) => {
        if (item.children) {
          item.children = deleteMiddleRouteComponent(item.children)
        }
      })
      return routes
    })

    // 删除路由中间层级对应的组件
    function deleteMiddleRouteComponent(routes: RouteRecordRaw[]) {
      const res: RouteRecordRaw[] = []
      routes.forEach((route) => {
        if (route.children?.length) {
          delete route.component
          route.children = deleteMiddleRouteComponent(route.children)
        }
        else {
          delete route.children
        }
        res.push(route)
      })
      return res
    }

    // 路由匹配器
    const routesMatcher = ref<RouterMatcher>()

    // 根据路径获取匹配的路由
    function getRouteMatchedByPath(path: string) {
      return routesMatcher.value?.resolve({ path }, undefined!)?.matched ?? []
    }

    // 生成路由（前端生成）
    function generateRoutesAtFront(asyncRoutes: Route.recordMainRaw[]) {
      // 设置 routes 数据
      routesRaw.value = cloneDeep(asyncRoutes) as any
      // 创建路由匹配器
      const routes: RouteRecordRaw[] = []
      routesRaw.value.forEach((route) => {
        if (route.children) {
          routes.push(...route.children)
        }
      })
      routesMatcher.value = createRouterMatcher(routes, {})
      isGenerate.value = true
    }

    // 记录 accessRoutes 路由，用于登出时删除路由
    function setCurrentRemoveRoutes(routes: (() => void)[]) {
      currentRemoveRoutes.value = routes
    }

    // 清空动态路由
    function removeRoutes() {
      isGenerate.value = false
      routesRaw.value = []
      currentRemoveRoutes.value.forEach((removeRoute) => {
        removeRoute()
      })
      currentRemoveRoutes.value = []
    }

    return {
      isGenerate,
      routesRaw,
      currentRemoveRoutes,
      routes,
      systemRoutes,
      getRouteMatchedByPath,
      generateRoutesAtFront,
      setCurrentRemoveRoutes,
      removeRoutes,
      hasPermission,
    }
  },
)
