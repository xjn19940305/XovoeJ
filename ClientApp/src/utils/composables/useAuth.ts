export default function useAuth() {
  function hasPermission(permission: string) {
    const settingsStore = useSettingsStore()
    const userStore = useUserStore()
    if (settingsStore.settings.app.enablePermission) {
      // 如果用户有超级管理员角色，拥有所有权限
      const userRoles = userStore.roles || []
      if (userRoles.includes('超级管理员')) {
        return true
      }
      // 检查通配符权限
      if (userStore.permissions.includes('*')) {
        return true
      }
      return userStore.permissions.includes(permission)
    }
    else {
      return true
    }
  }

  function auth(value: string | string[]) {
    let auth
    if (typeof value === 'string') {
      auth = value !== '' ? hasPermission(value) : true
    }
    else {
      auth = value.length > 0 ? value.some(item => hasPermission(item)) : true
    }
    return auth
  }

  function authAll(value: string[]) {
    return value.length > 0 ? value.every(item => hasPermission(item)) : true
  }

  return {
    auth,
    authAll,
  }
}
