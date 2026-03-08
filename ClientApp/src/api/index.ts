import axios from 'axios'
import { toast } from 'vue-sonner'

// Token 刷新相关
let isRefreshing = false
let refreshSubscribers: Array<(token: string) => void> = []

// 扩展 AxiosRequestConfig 类型
declare module 'axios' {
  export interface AxiosRequestConfig {
    retry?: boolean
    retryCount?: number
    skipAuthRefresh?: boolean
  }
}

const useProxy = import.meta.env.DEV && import.meta.env.VITE_OPEN_PROXY

const api = axios.create({
  baseURL: useProxy ? '/proxy/' : import.meta.env.VITE_APP_API_BASEURL,
  timeout: 1000 * 60,
  responseType: 'json',
})

export function buildAdminApiPath(path: string) {
  return `api/admin/${path.replace(/^\/+/, '')}`
}

export function buildMallApiPath(path: string) {
  return `api/mall/${path.replace(/^\/+/, '')}`
}

export function buildIntegrationApiPath(path: string) {
  return `api/integration/${path.replace(/^\/+/, '')}`
}

// 添加 Token 到请求
function addToken(config: any) {
  const userStore = useUserStore()
  if (userStore.isLogin && config.headers) {
    config.headers.Authorization = `Bearer ${userStore.token}`
  }
  return config
}

// 刷新 Token
async function refreshTokenAndRetry(originalRequest: any): Promise<any> {
  const userStore = useUserStore()

  // 如果已经在刷新 Token，将请求加入队列
  if (isRefreshing) {
    return new Promise((resolve) => {
      refreshSubscribers.push((token: string) => {
        originalRequest.headers.Authorization = `Bearer ${token}`
        resolve(api(originalRequest))
      })
    })
  }

  isRefreshing = true

  try {
    const loginData = await userStore.refreshTokenApi()
    // 刷新成功，处理队列中的请求
    refreshSubscribers.forEach(callback => callback(loginData.accessToken))
    refreshSubscribers = []
    // 重试当前请求
    originalRequest.headers.Authorization = `Bearer ${loginData.accessToken}`
    return api(originalRequest)
  }
  catch {
    // 刷新失败，跳转到登录页
    refreshSubscribers = []
    userStore.requestLogout()
    return Promise.reject(new Error('登录已过期'))
  }
  finally {
    isRefreshing = false
  }
}

api.interceptors.request.use(
  (request) => {
    return addToken(request)
  },
)

// 处理错误信息的函数
function handleError(error: any) {
  let message = error.message
  if (message === 'Network Error') {
    message = '后端网络故障'
  }
  else if (message.includes('timeout')) {
    message = '接口请求超时'
  }
  else if (error.response?.data?.message) {
    message = error.response.data.message
  }
  toast.error('Error', {
    description: message,
  })
  return Promise.reject(error)
}

api.interceptors.response.use(
  (response) => {
    // ASP.NET Core API 通常直接返回数据
    return response
  },
  async (error) => {
    const originalRequest = error.config

    // 401 错误，尝试刷新 Token
    if (error.response?.status === 401 && originalRequest && !originalRequest.skipAuthRefresh) {
      const userStore = useUserStore()
      if (userStore.isLogin && userStore.refreshToken) {
        // 标记跳过刷新，避免无限循环
        originalRequest.skipAuthRefresh = true
        return refreshTokenAndRetry(originalRequest)
      }
      else {
        userStore.requestLogout()
        return Promise.reject(error)
      }
    }

    // 其他错误，显示提示
    return handleError(error)
  },
)

export default api
