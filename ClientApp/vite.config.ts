import fs from 'node:fs'
import path from 'node:path'
import process from 'node:process'
import dayjs from 'dayjs'
import { defineConfig, loadEnv } from 'vite'
import pkg from './package.json'
import createVitePlugins from './vite/plugins'

function createManualChunks(id: string) {
  if (!id.includes('node_modules')) {
    return
  }

  if (
    id.includes('/vue/')
    || id.includes('/vue-router/')
    || id.includes('/pinia/')
    || id.includes('/@vue/')
    || id.includes('/@vueuse/')
  ) {
    return 'vendor-vue'
  }

  if (id.includes('/element-plus/')) {
    return 'vendor-element-plus'
  }

  if (id.includes('/@wangeditor/')) {
    return 'vendor-editor'
  }

  if (
    id.includes('/@iconify/')
    || id.includes('/lucide-vue-next/')
    || id.includes('/@element-plus/icons-vue/')
  ) {
    return 'vendor-icons'
  }

  if (
    id.includes('/axios/')
    || id.includes('/dayjs/')
    || id.includes('/qs/')
    || id.includes('/zod/')
    || id.includes('/vee-validate/')
    || id.includes('/@vee-validate/')
    || id.includes('/reka-ui/')
    || id.includes('/@floating-ui/')
  ) {
    return 'vendor-utils'
  }

  return 'vendor-misc'
}

// https://vitejs.dev/config/
export default defineConfig(({ mode, command }) => {
  const env = loadEnv(mode, process.cwd())
  // 全局 scss 资源
  const scssResources: string[] = []
  fs.readdirSync('src/assets/styles/resources').forEach((dirname) => {
    if (fs.statSync(`src/assets/styles/resources/${dirname}`).isFile()) {
      scssResources.push(`@use "/src/assets/styles/resources/${dirname}" as *;`)
    }
  })
  return {
    // 开发服务器选项 https://cn.vitejs.dev/config/server-options
    server: {
      open: true,
      host: true,
      port: 9000,
      proxy: {
        '/proxy': {
          target: env.VITE_APP_API_BASEURL,
          changeOrigin: command === 'serve' && env.VITE_OPEN_PROXY === 'true',
          rewrite: path => path.replace(/\/proxy/, ''),
        },
      },
    },
    // 构建选项 https://cn.vitejs.dev/config/build-options
    build: {
      outDir: mode === 'production' ? 'dist' : `dist-${mode}`,
      sourcemap: env.VITE_BUILD_SOURCEMAP === 'true',
      // Large vendor-only chunks are intentionally isolated for caching and lazy loading.
      chunkSizeWarningLimit: 900,
      rollupOptions: {
        output: {
          manualChunks: createManualChunks,
        },
      },
    },
    define: {
      __SYSTEM_INFO__: JSON.stringify({
        pkg: {
          version: pkg.version,
          dependencies: pkg.dependencies,
          devDependencies: pkg.devDependencies,
        },
        lastBuildTime: dayjs().format('YYYY-MM-DD HH:mm:ss'),
      }),
    },
    plugins: createVitePlugins(mode, command === 'build'),
    resolve: {
      alias: {
        '@': path.resolve(__dirname, 'src'),
        '#': path.resolve(__dirname, 'src/types'),
      },
    },
    css: {
      preprocessorOptions: {
        scss: {
          additionalData: scssResources.join(''),
        },
      },
    },
  }
})
