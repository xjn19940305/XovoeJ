interface ImportMetaEnv {
  // Auto generate by env-parse
  /**
   * App settings panel
   */
  readonly VITE_APP_SETTING: boolean
  /**
   * Page title
   */
  readonly VITE_APP_TITLE: string
  /**
   * API base URL, used as axios baseURL
   */
  readonly VITE_APP_API_BASEURL: string
  /**
   * Debug tool: eruda or vconsole
   */
  readonly VITE_APP_DEBUG_TOOL: string
  /**
   * Disable devtools
   */
  readonly VITE_APP_DISABLE_DEVTOOL: boolean
  /**
   * Enable proxy in development
   */
  readonly VITE_OPEN_PROXY: boolean
  /**
   * Enable Vue devtools
   */
  readonly VITE_OPEN_DEVTOOLS: boolean
}
