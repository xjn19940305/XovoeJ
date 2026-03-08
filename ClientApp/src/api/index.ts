import axios from 'axios'
import { toast } from 'vue-sonner'

let isRefreshing = false
let refreshSubscribers: Array<(token: string) => void> = []

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

function addToken(config: any) {
  const userStore = useUserStore()
  if (userStore.isLogin && config.headers) {
    config.headers.Authorization = `Bearer ${userStore.token}`
  }
  return config
}

async function refreshTokenAndRetry(originalRequest: any): Promise<any> {
  const userStore = useUserStore()

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
    refreshSubscribers.forEach(callback => callback(loginData.accessToken))
    refreshSubscribers = []
    originalRequest.headers.Authorization = `Bearer ${loginData.accessToken}`
    return api(originalRequest)
  }
  catch {
    refreshSubscribers = []
    userStore.requestLogout()
    return Promise.reject(new Error('登录已过期'))
  }
  finally {
    isRefreshing = false
  }
}

api.interceptors.request.use((request) => {
  return addToken(request)
})

function handleError(error: any) {
  let message = error.message
  if (message === 'Network Error') {
    message = '后端网络异常'
  }
  else if (typeof message === 'string' && message.includes('timeout')) {
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
    const payload = response.data
    if (payload && typeof payload === 'object' && 'code' in payload && 'message' in payload && 'data' in payload) {
      if (payload.code !== 0) {
        const businessError = new Error(payload.message || '请求失败')
        ;(businessError as any).response = response
        throw businessError
      }
      response.data = payload.data
    }

    return response
  },
  async (error) => {
    const originalRequest = error.config

    if (error.response?.status === 401 && originalRequest && !originalRequest.skipAuthRefresh) {
      const userStore = useUserStore()
      if (userStore.isLogin && userStore.refreshToken) {
        originalRequest.skipAuthRefresh = true
        return refreshTokenAndRetry(originalRequest)
      }

      userStore.requestLogout()
      return Promise.reject(error)
    }

    return handleError(error)
  },
)

export default api
