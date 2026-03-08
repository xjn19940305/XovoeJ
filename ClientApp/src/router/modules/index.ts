import type { Route } from '#/global'
import content from './content'
import growth from './growth'
import marketing from './marketing'
import message from './message'
import shop from './shop'
import system from './system'

// 合并所有模块路由
const xovoejRoutes: Route.recordMainRaw[] = [
  content,
  growth,
  marketing,
  message,
  shop,
  system,

  // 未来可以在这里添加更多模块
  // import('./xxx').then(m => defaultRoutes.push(m.default)),
]

// 导出为单个对象以保持兼容性
const xovoejRoute: Route.recordMainRaw = {
  meta: {
    title: 'XovoeJ',
    icon: 'i-ant-design:home-twotone',
  },
  children: [],
}

// 将所有模块的 children 合并到主路由中
xovoejRoutes.forEach((moduleRoute) => {
  if (moduleRoute.children) {
    xovoejRoute.children.push(...moduleRoute.children)
  }
})

export default xovoejRoute
export { xovoejRoutes }
