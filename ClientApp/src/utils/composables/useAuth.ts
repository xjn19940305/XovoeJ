import { hasAnyPermission, hasPermission as hasPermissionCode } from '@/utils/permission'

export default function useAuth() {
  const settingsStore = useSettingsStore()
  const userStore = useUserStore()

  function hasPermission(permission: string) {
    if (!settingsStore.settings.app.enablePermission) {
      return true
    }

    const userRoles = userStore.roles || []
    if (userRoles.includes('超级管理员')) {
      return true
    }

    if (userStore.permissions.includes('*')) {
      return true
    }

    return hasPermissionCode(userStore.permissions, permission)
  }

  function auth(value: string | string[]) {
    if (typeof value === 'string') {
      return value !== '' ? hasPermission(value) : true
    }

    return value.length > 0 ? hasAnyPermission(userStore.permissions, value) : true
  }

  function authAll(value: string[]) {
    return value.length > 0 ? value.every(item => hasPermission(item)) : true
  }

  return {
    auth,
    authAll,
  }
}
