import type { Menu, Route } from '#/global'
import type { RouteRecordRaw } from 'vue-router'
import { cloneDeep } from 'es-toolkit'
import { resolveRoutePath } from '@/utils'

export const useMenuStore = defineStore(
  'menu',
  () => {
    const settingsStore = useSettingsStore()
    const routeStore = useRouteStore()

    const actived = ref(0)

    function convertRouteToMenu(routes: Route.recordMainRaw[]): Menu.recordMainRaw[] {
      const returnMenus: Menu.recordMainRaw[] = []
      routes.forEach((item) => {
        if (item.children.length > 0) {
          if (settingsStore.settings.menu.mode === 'single') {
            if (returnMenus.length === 0) {
              returnMenus.push({
                meta: {},
                children: [],
              })
            }
            returnMenus[0].children.push(...convertRouteToMenuRecursive(item.children))
          }
          else {
            const menuItem: Menu.recordMainRaw = {
              meta: {
                title: item.meta?.title,
                icon: item.meta?.icon,
                auth: item.meta?.auth,
              },
              children: convertRouteToMenuRecursive(item.children),
            }
            returnMenus.push(menuItem)
          }
        }
      })
      return returnMenus
    }

    function convertRouteToMenuRecursive(routes: RouteRecordRaw[], basePath = ''): Menu.recordRaw[] {
      const returnMenus: Menu.recordRaw[] = []
      routes.forEach((item) => {
        const menuItem: Menu.recordRaw = {
          path: resolveRoutePath(basePath, item.path),
          meta: {
            title: item.meta?.title,
            icon: item.meta?.icon,
            defaultOpened: item.meta?.defaultOpened,
            auth: item.meta?.auth,
            menu: item.meta?.menu,
            link: item.meta?.link,
          },
        }
        if (item.children) {
          menuItem.children = convertRouteToMenuRecursive(item.children, menuItem.path)
        }
        returnMenus.push(menuItem)
      })
      return returnMenus
    }

    const allMenus = computed(() => {
      let returnMenus: Menu.recordMainRaw[] = []
      const routesRaw = routeStore.routesRaw

      if (settingsStore.settings.menu.mode === 'single') {
        routesRaw.forEach((mainRoute) => {
          if (!mainRoute.children) {
            return
          }
          mainRoute.children.forEach((route) => {
            if (!routeStore.hasPermission(route.meta?.auth ?? '')) {
              return
            }
            const children = route.children
              ? convertRouteToMenuRecursive(route.children, route.path)
              : []
            if (children.length === 0) {
              return
            }
            returnMenus.push({
              meta: {
                title: route.meta?.title,
                icon: route.meta?.icon,
                auth: route.meta?.auth,
              },
              children,
            })
          })
        })
      }
      else {
        returnMenus = convertRouteToMenu(routesRaw)
        returnMenus = filterAsyncMenus(returnMenus)
      }

      return returnMenus
    })

    const sidebarMenus = computed<Menu.recordMainRaw['children']>(() => {
      if (allMenus.value.length === 0) {
        return []
      }

      if (settingsStore.settings.menu.mode === 'single') {
        return allMenus.value
          .filter(mainMenu => mainMenu.children?.length)
          .map((mainMenu, index) => ({
            path: `#group-${index}`,
            meta: {
              title: mainMenu.meta?.title,
              icon: mainMenu.meta?.icon,
              isGroup: true,
            },
            children: mainMenu.children,
          }))
      }

      return allMenus.value.length > 1
        ? allMenus.value[actived.value].children
        : allMenus.value[0].children
    })

    const sidebarMenusFirstDeepestPath = computed(() => {
      return sidebarMenus.value.length > 0
        ? getDeepestPath(sidebarMenus.value[0])
        : settingsStore.settings.home.fullPath
    })

    function getDeepestPath(menu: Menu.recordRaw, rootPath = '') {
      let retnPath = ''
      if (menu.children?.some(item => item.meta?.menu !== false)) {
        const item = menu.children.find(item => item.meta?.menu !== false)
        if (item) {
          retnPath = getDeepestPath(item, resolveRoutePath(rootPath, menu.path))
        }
        else {
          retnPath = getDeepestPath(menu.children[0], resolveRoutePath(rootPath, menu.path))
        }
      }
      else {
        retnPath = resolveRoutePath(rootPath, menu.path)
      }
      return retnPath
    }

    const sidebarMenusHasOnlyMenu = computed(() => {
      return isSidebarMenusHasOnlyMenu(sidebarMenus.value)
    })

    function isSidebarMenusHasOnlyMenu(menus: Menu.recordRaw[]) {
      let count = 0
      let isOnly = true
      menus.forEach((menu) => {
        if (menu.meta?.menu !== false) {
          count++
        }
        if (menu.children) {
          isOnly = isSidebarMenusHasOnlyMenu(menu.children)
        }
      })
      return count <= 1 && isOnly
    }

    const defaultOpenedPaths = computed(() => {
      const openedPaths: string[] = []
      allMenus.value.forEach((item) => {
        openedPaths.push(...getDefaultOpenedPaths(item.children))
      })
      return openedPaths
    })

    function getDefaultOpenedPaths(menus: Menu.recordRaw[], rootPath = '') {
      const openedPaths: string[] = []
      menus.forEach((item) => {
        if (item.meta?.defaultOpened && item.children) {
          openedPaths.push(resolveRoutePath(rootPath, item.path))
          const childrenDefaultOpenedPaths = getDefaultOpenedPaths(item.children, resolveRoutePath(rootPath, item.path))
          if (childrenDefaultOpenedPaths.length > 0) {
            openedPaths.push(...childrenDefaultOpenedPaths)
          }
        }
      })
      return openedPaths
    }

    const auth = useAuth()

    function filterAsyncMenus<T extends Menu.recordMainRaw[] | Menu.recordRaw[]>(menus: T): T {
      const res: any = []
      menus.forEach((menu) => {
        if (auth.auth(menu.meta?.auth ?? '')) {
          const tmpMenu = cloneDeep(menu)
          if (tmpMenu.children && tmpMenu.children.length > 0) {
            tmpMenu.children = filterAsyncMenus(tmpMenu.children) as Menu.recordRaw[]
            if (tmpMenu.children.length > 0) {
              res.push(tmpMenu)
            }
          }
          else {
            delete tmpMenu.children
            res.push(tmpMenu)
          }
        }
      })
      return res
    }

    function isPathInMenus(menus: Menu.recordRaw[], path: string): boolean {
      return menus.some((item) => {
        if (item.children) {
          return isPathInMenus(item.children, path)
        }
        return path.indexOf(`${item.path}/`) === 0 || path === item.path
      })
    }

    function setActived(indexOrPath: number | string) {
      if (typeof indexOrPath === 'number') {
        actived.value = indexOrPath
        return
      }

      const findIndex = allMenus.value.findIndex(item => isPathInMenus(item.children, indexOrPath))
      if (findIndex >= 0) {
        actived.value = findIndex
      }
    }

    return {
      actived,
      allMenus,
      sidebarMenus,
      sidebarMenusFirstDeepestPath,
      sidebarMenusHasOnlyMenu,
      defaultOpenedPaths,
      setActived,
    }
  },
)
