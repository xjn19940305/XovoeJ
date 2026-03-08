<script setup lang="ts">
import type { UploadFile, UploadRequestOptions } from 'element-plus'
import { ElMessage } from 'element-plus'
import { ref, watch } from 'vue'

interface Props {
  modelValue: string[]
  maxSize?: number // MB
  maxCount?: number
  limit?: number
  disabled?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  maxSize: 5,
  maxCount: 10,
  limit: 10,
  disabled: false,
})

const emit = defineEmits<{
  'update:modelValue': [value: string[]]
  'change': [value: string[]]
}>()

const fileList = ref<UploadFile[]>([])
const previewVisible = ref(false)
const previewUrl = ref('')

// 获取完整 URL（用于显示）
function getFullUrl(path: string) {
  // 如果已经是完整 URL，直接返回
  if (path.startsWith('http://') || path.startsWith('https://') || path.startsWith('data:')) {
    return path
  }
  // 否则拼接为完整 URL
  const baseURL = import.meta.env.VITE_APP_API_BASEURL || 'https://localhost:7216'
  return `${baseURL}/api/files/proxy?key=${encodeURIComponent(path)}`
}

// 记录上次的值，避免重复更新
const prevValue = ref<string[]>([])
const isInternalUpdate = ref(false)

// 初始化文件列表
watch(() => props.modelValue, (newVal) => {
  // 如果是内部更新导致的，跳过
  if (isInternalUpdate.value) {
    isInternalUpdate.value = false
    return
  }
  // 检查是否真的发生了变化
  const newValStr = JSON.stringify(newVal)
  const prevStr = JSON.stringify(prevValue.value)

  if (newValStr !== prevStr) {
    prevValue.value = [...newVal]
    fileList.value = newVal.map((path, index) => ({
      name: `image-${index}`,
      url: getFullUrl(path), // 使用完整 URL 显示
      path, // 保存原始路径
      uid: Date.now() + index,
      status: 'success',
    } as UploadFile))
  }
}, { immediate: true })

type UploadError = Parameters<NonNullable<UploadRequestOptions['onError']>>[0]

function toUploadError(error: unknown): UploadError {
  return error as UploadError
}

function beforeUpload(file: File) {
  const isImage = file.type.startsWith('image/')
  if (!isImage) {
    ElMessage.error('只能上传图片文件!')
    return false
  }
  const isLtM = file.size / 1024 / 1024 < props.maxSize
  if (!isLtM) {
    ElMessage.error(`图片大小不能超过 ${props.maxSize}MB!`)
    return false
  }
  return true
}

/* eslint-disable no-console */
async function handleUpload(options: UploadRequestOptions) {
  const { file, onSuccess, onError } = options

  const baseURL = import.meta.env.VITE_APP_API_BASEURL || 'https://localhost:7216'
  const formData = new FormData()
  formData.append('file', file)

  try {
    // 上传到服务器获取 OSS key
    const response = await fetch(`${baseURL}/api/files/upload`, {
      method: 'POST',
      body: formData,
    })
    const result = await response.json()

    if (result.key) {
      // 存储相对路径（key）
      const relativePath = result.key
      // 用于显示的完整 URL
      const fullUrl = `${baseURL}/api/files/proxy?key=${encodeURIComponent(result.key)}`

      console.log('上传成功 - relativePath:', relativePath)
      console.log('上传成功 - fullUrl:', fullUrl)

      const newUrls = [...props.modelValue]
      if (newUrls.length < props.maxCount) {
        newUrls.push(relativePath) // 存储相对路径
        console.log('存储到 modelValue 的值:', newUrls)
        emit('update:modelValue', newUrls)
        emit('change', newUrls)
        onSuccess(fullUrl) // el-upload 使用完整 URL 显示
      }
      else {
        ElMessage.warning(`最多只能上传${props.maxCount}张图片`)
        onError(toUploadError(new Error('超过最大数量')))
      }
    }
    else {
      ElMessage.error(result.message || '上传失败')
      onError(toUploadError(new Error(result.message || '上传失败')))
    }
  }
  catch (error) {
    ElMessage.error('上传失败')
    onError(toUploadError(error))
  }
}
/* eslint-enable no-console */

function handleRemove(file: UploadFile) {
  const index = fileList.value.findIndex(f => f.uid === file.uid)
  if (index > -1) {
    const newUrls = [...props.modelValue]
    newUrls.splice(index, 1)
    emit('update:modelValue', newUrls)
    emit('change', newUrls)
  }
}

function handlePreview(file: UploadFile) {
  previewUrl.value = file.url || ''
  previewVisible.value = true
}

// 将完整 URL 转换为相对路径
/* eslint-disable style/max-statements-per-line */
function convertToRelativePath(url: string | undefined): string | undefined {
  if (!url) { return undefined }
  // 如果已经是相对路径（不以 http 开头），直接返回
  if (!url.startsWith('http')) { return url }

  const baseURL = import.meta.env.VITE_APP_API_BASEURL || 'https://localhost:7216'
  const proxyPrefix = `${baseURL}/api/files/proxy?key=`

  if (url.startsWith(proxyPrefix)) {
    // 从 URL 中提取 key（URL 编码的）
    const urlObj = new URL(url)
    const keyParam = urlObj.searchParams.get('key')
    if (keyParam) {
      return keyParam // 已经是解码后的
    }
  }
  return url
}
/* eslint-enable style/max-statements-per-line */

function handleChange(_file: UploadFile, fileList: UploadFile[]) {
  // 当 el-upload 内部变化时，同步更新我们的 fileList
  // 同时更新 modelValue（需要转换为相对路径）
  isInternalUpdate.value = true
  const urls = fileList
    .filter(f => f.status === 'success' && f.url)
    .map(f => convertToRelativePath(f.url) || f.url as string)
  emit('update:modelValue', urls)
}
</script>

<template>
  <div class="product-image-upload">
    <el-upload
      :file-list="fileList"
      :http-request="handleUpload"
      :on-remove="handleRemove"
      :on-preview="handlePreview"
      :before-upload="beforeUpload"
      :on-change="handleChange"
      :limit="maxCount"
      :disabled="disabled"
      list-type="picture-card"
      accept="image/*"
      multiple
    >
      <div class="upload-trigger">
        <FaIcon name="i-iconoir:plus" class="text-2xl" />
      </div>
    </el-upload>
    <div class="upload-tips">
      <div class="tip-item">
        <FaIcon name="i-heroicons-solid:information-circle" class="size-4" />
        <span>第一张为主图，最多{{ maxCount }}张</span>
      </div>
      <div class="tip-item">
        <FaIcon name="i-heroicons-solid:photo" class="size-4" />
        <span>建议尺寸：800x800像素，单张不超过{{ maxSize }}MB</span>
      </div>
    </div>

    <!-- 图片预览 -->
    <el-dialog v-model="previewVisible" title="图片预览" width="600px">
      <img :src="previewUrl" class="w-full">
    </el-dialog>
  </div>
</template>

<style scoped>
.product-image-upload :deep(.el-upload--picture-card) {
  width: 100px;
  height: 100px;
  border-radius: 8px;
  border: 1px dashed hsl(var(--border));
  transition: all 0.2s;
}

.product-image-upload :deep(.el-upload--picture-card:hover) {
  border-color: hsl(var(--primary));
}

.product-image-upload :deep(.el-upload-list--picture-card .el-upload-list__item) {
  width: 100px;
  height: 100px;
  border-radius: 8px;
}

.upload-trigger {
  width: 100px;
  height: 100px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: hsl(var(--foreground) / 0.4);
}

.upload-tips {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  margin-top: 12px;
}

.tip-item {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  color: hsl(var(--foreground) / 0.5);
}
</style>
