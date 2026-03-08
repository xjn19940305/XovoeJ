import apiAuth from '@/api/modules/auth'
import router from '@/router'

export const useUserStore = defineStore(
  // 唯一ID
  'user',
  () => {
    const settingsStore = useSettingsStore()
    const routeStore = useRouteStore()
    const menuStore = useMenuStore()
    const tabbarStore = useTabbarStore()

    const account = ref(localStorage.account ?? '')
    const token = ref(localStorage.token ?? '')
    const refreshToken = ref(localStorage.refreshToken ?? '')
    const avatar = ref(localStorage.avatar ?? '')
    const userId = ref(localStorage.userId ?? '')
    const userName = ref(localStorage.userName ?? '')
    const nickName = ref(localStorage.nickName ?? '')
    const permissions = ref<string[]>([])
    const roles = ref<string[]>(JSON.parse(localStorage.roles ?? '[]'))

    function updatePermissions(nextPermissions: string[]) {
      const next = nextPermissions || []
      localStorage.setItem('permissions', JSON.stringify(next))
      permissions.value = next
    }

    const isLogin = computed(() => {
      return !!token.value
    })

    // 登录
    async function login(data: {
      account: string
      password: string
    }) {
      const res = await apiAuth.login(data)
      const loginData = res.data

      // 保存登录信息
      localStorage.setItem('account', loginData.user.userName ?? data.account)
      localStorage.setItem('token', loginData.accessToken)
      localStorage.setItem('refreshToken', loginData.refreshToken)
      localStorage.setItem('avatar', loginData.user.picture ?? '')
      localStorage.setItem('userId', loginData.user.id)
      localStorage.setItem('userName', loginData.user.userName ?? '')
      localStorage.setItem('nickName', loginData.user.nickName ?? '')
      localStorage.setItem('roles', JSON.stringify(loginData.user.roles || []))
      updatePermissions(loginData.user.permissions || [])

      account.value = loginData.user.userName ?? data.account
      token.value = loginData.accessToken
      refreshToken.value = loginData.refreshToken
      avatar.value = loginData.user.picture ?? ''
      userId.value = loginData.user.id
      userName.value = loginData.user.userName ?? ''
      nickName.value = loginData.user.nickName ?? ''
      roles.value = loginData.user.roles || []
    }

    // 获取用户信息
    async function getUserInfo() {
      const res = await apiAuth.getUserInfo()
      const userInfo = res.data

      // 更新本地存储
      localStorage.setItem('avatar', userInfo.picture ?? '')
      localStorage.setItem('userId', userInfo.id)
      localStorage.setItem('userName', userInfo.userName ?? '')
      localStorage.setItem('nickName', userInfo.nickName ?? '')
      localStorage.setItem('roles', JSON.stringify(userInfo.roles || []))
      updatePermissions(userInfo.permissions || [])

      avatar.value = userInfo.picture ?? ''
      userId.value = userInfo.id
      userName.value = userInfo.userName ?? ''
      nickName.value = userInfo.nickName ?? ''
      roles.value = userInfo.roles || []

      return userInfo
    }

    // 手动登出
    async function logout(redirect = router.currentRoute.value.fullPath) {
      try {
        // 调用后端登出接口
        await apiAuth.logout(refreshToken.value)
      }
      catch {
        // 忽略登出接口错误
      }
      finally {
        // 清除本地存储
        localStorage.removeItem('token')
        localStorage.removeItem('refreshToken')
        token.value = ''
        refreshToken.value = ''

        router.push({
          name: 'login',
          query: {
            ...(redirect !== settingsStore.settings.home.fullPath && router.currentRoute.value.name !== 'login' && { redirect }),
          },
        }).then(logoutCleanStatus)
      }
    }

    // 请求登出
    async function requestLogout() {
      try {
        await apiAuth.logout(refreshToken.value)
      }
      catch {
        // 忽略登出接口错误
      }
      finally {
        localStorage.removeItem('token')
        localStorage.removeItem('refreshToken')
        token.value = ''
        refreshToken.value = ''

        router.push({
          name: 'login',
          query: {
            ...(
              router.currentRoute.value.fullPath !== settingsStore.settings.home.fullPath
              && router.currentRoute.value.name !== 'login'
              && {
                redirect: router.currentRoute.value.fullPath,
              }
            ),
          },
        }).then(logoutCleanStatus)
      }
    }

    // 登出后清除状态
    function logoutCleanStatus() {
      localStorage.removeItem('account')
      localStorage.removeItem('avatar')
      localStorage.removeItem('userId')
      localStorage.removeItem('userName')
      localStorage.removeItem('nickName')
      localStorage.removeItem('roles')
      localStorage.removeItem('permissions')
      account.value = ''
      avatar.value = ''
      userId.value = ''
      userName.value = ''
      nickName.value = ''
      permissions.value = []
      roles.value = []
      settingsStore.updateSettings({}, true)
      tabbarStore.clean()
      routeStore.removeRoutes()
      menuStore.setActived(0)
    }

    // 获取权限（兼容旧接口）
    async function getPermissions() {
      await getUserInfo()
      return permissions.value
    }

    // 修改密码
    async function editPassword(_data: {
      password: string
      newPassword: string
    }) {
      // TODO: 调用后端修改密码接口
    }

    // 刷新Token
    async function refreshTokenApi() {
      if (!refreshToken.value || !userId.value) {
        throw new Error('缺少刷新令牌')
      }
      const res = await apiAuth.refreshToken({
        userId: userId.value,
        refreshToken: refreshToken.value,
      })
      const loginData = res.data

      localStorage.setItem('token', loginData.accessToken)
      localStorage.setItem('refreshToken', loginData.refreshToken)
      token.value = loginData.accessToken
      refreshToken.value = loginData.refreshToken

      return loginData
    }

    return {
      account,
      token,
      refreshToken,
      avatar,
      userId,
      userName,
      nickName,
      permissions,
      roles,
      isLogin,
      login,
      logout,
      requestLogout,
      getUserInfo,
      getPermissions,
      editPassword,
      refreshTokenApi,
    }
  },
)
