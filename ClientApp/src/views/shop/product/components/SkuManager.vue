<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { ElMessage } from 'element-plus'
import { Minus, Plus } from '@element-plus/icons-vue'
import type { UploadRequestOptions } from 'element-plus'

interface SpecAttribute {
  id: string
  name: string
  values: {
    id: string
    name: string
  }[]
}

interface Sku {
  id: string
  specs: Record<string, string>
  price: number
  originalPrice?: number
  costPrice?: number
  stock: number
  lowStock: number
  image?: string
}

interface Props {
  modelValue: Sku[]
  isEdit?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  isEdit: false
})

const emit = defineEmits<{
  'update:modelValue': [value: Sku[]]
}>()

const specAttributes = ref<SpecAttribute[]>([])
// 确保 skuList 至少有一个默认规格
const skuList = ref<Sku[]>(
  props.modelValue.length > 0
    ? [...props.modelValue]
    : [{
        id: generateId(),
        specs: {},
        price: 0,
        originalPrice: 0,
        costPrice: 0,
        stock: 0,
        lowStock: 10
      }]
)

// 是否正在更新（防止循环）
let isUpdating = false

// 生成唯一ID
function generateId() {
  return Date.now().toString(36) + Math.random().toString(36).substr(2)
}

// 从 SKU 数据反向推导 specAttributes
function deriveSpecAttributesFromSkus(skus: Sku[]) {
  const attrMap = new Map<string, Set<string>>()

  skus.forEach(sku => {
    Object.entries(sku.specs).forEach(([attrName, valueName]) => {
      if (!attrMap.has(attrName)) {
        attrMap.set(attrName, new Set())
      }
      attrMap.get(attrName)!.add(valueName)
    })
  })

  return Array.from(attrMap.entries()).map(([name, valueSet]) => ({
    id: generateId(),
    name,
    values: Array.from(valueSet).map(v => ({ id: generateId(), name: v }))
  }))
}

// 获取完整 URL（用于显示）
function getFullUrl(path: string | undefined) {
  if (!path) return ''
  // 如果已经是完整 URL 或 base64，直接返回
  if (path.startsWith('http://') || path.startsWith('https://') || path.startsWith('data:')) {
    return path
  }
  // 否则拼接为完整 URL
  const baseURL = import.meta.env.VITE_APP_API_BASEURL || 'https://localhost:7216'
  return `${baseURL}/api/files/proxy?key=${encodeURIComponent(path)}`
}

// 监听外部变化
watch(() => props.modelValue, (newVal) => {
  if (!isUpdating) {
    isUpdating = true
    skuList.value = [...newVal]
    // 反向推导 specAttributes（当有规格数据时）
    if (newVal.length > 0 && newVal.some(s => Object.keys(s.specs || {}).length > 0)) {
      specAttributes.value = deriveSpecAttributesFromSkus(newVal)
    }
    nextTick(() => {
      isUpdating = false
    })
  }
}, { deep: true })

// 监听内部变化
watch(skuList, (newVal) => {
  if (!isUpdating) {
    isUpdating = true
    emit('update:modelValue', newVal)
    nextTick(() => {
      isUpdating = false
    })
  }
}, { deep: true })

// 添加规格
function handleAddAttribute() {
  specAttributes.value.push({
    id: generateId(),
    name: '',
    values: []
  })
}

// 删除规格
function handleRemoveAttribute(id: string) {
  const index = specAttributes.value.findIndex(attr => attr.id === id)
  if (index > -1) {
    specAttributes.value.splice(index, 1)
    generateSkus()
  }
}

// 添加规格值
function handleAddValue(attrId: string) {
  const attr = specAttributes.value.find(a => a.id === attrId)
  if (attr) {
    attr.values.push({
      id: generateId(),
      name: ''
    })
  }
}

// 删除规格值
function handleRemoveValue(attrId: string, valueId: string) {
  const attr = specAttributes.value.find(a => a.id === attrId)
  if (attr) {
    const index = attr.values.findIndex(v => v.id === valueId)
    if (index > -1) {
      attr.values.splice(index, 1)
      generateSkus()
    }
  }
}

// 生成SKU组合
function generateSkus() {
  const validAttrs = specAttributes.value.filter(a => a.name && a.values.length > 0)

  if (validAttrs.length === 0) {
    skuList.value = [{
      id: generateId(),
      specs: {},
      price: 0,
      originalPrice: 0,
      costPrice: 0,
      stock: 0,
      lowStock: 10
    }]
    return
  }

  // 笛卡尔积生成所有组合
  const combinations = (arrays: string[][]): string[][] => {
    if (arrays.length === 0) return [[]]
    const [first, ...rest] = arrays
    const restCombinations = combinations(rest)
    const result: string[][] = []
    for (const value of first) {
      for (const combo of restCombinations) {
        result.push([value, ...combo])
      }
    }
    return result
  }

  const attrValues = validAttrs.map(attr => attr.values.map(v => v.name))
  const combos = combinations(attrValues)

  const newSkus: Sku[] = combos.map((combo, index) => {
    const specs: Record<string, string> = {}
    validAttrs.forEach((attr, i) => {
      specs[attr.name] = combo[i]
    })

    // 尝试匹配现有SKU
    const existingSku = skuList.value.find(s => {
      return Object.keys(specs).every(key => s.specs[key] === specs[key])
    })

    return {
      id: existingSku?.id || generateId(),
      specs,
      price: existingSku?.price || 0,
      originalPrice: existingSku?.originalPrice || 0,
      costPrice: existingSku?.costPrice || 0,
      stock: existingSku?.stock || 0,
      lowStock: existingSku?.lowStock || 10,
      image: existingSku?.image
    }
  })

  skuList.value = newSkus
}

// SKU图片上传
async function handleSkuImageUpload(options: UploadRequestOptions, skuId: string) {
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

      const sku = skuList.value.find(s => s.id === skuId)
      if (sku) {
        sku.image = relativePath // 存储相对路径
      }
      onSuccess(fullUrl) // el-upload 使用完整 URL 显示
    } else {
      ElMessage.error(result.message || '上传失败')
      onError(new Error(result.message || '上传失败'))
    }
  } catch (error) {
    ElMessage.error('上传失败')
    onError(error as Error)
  }
}

function beforeSkuImageUpload(file: File) {
  const isImage = file.type.startsWith('image/')
  if (!isImage) {
    ElMessage.error('只能上传图片文件!')
    return false
  }
  const isLtM = file.size / 1024 / 1024 < 5
  if (!isLtM) {
    ElMessage.error('图片大小不能超过 5MB!')
    return false
  }
  return true
}
</script>

<template>
  <div class="sku-manager">
    <!-- 规格选择区域 -->
    <div class="sku-attributes-section">
      <div class="sku-section-header">
        <span class="sku-section-title">
          <FaIcon name="i-heroicons-solid:adjustments-horizontal" class="size-4" />
          商品规格
        </span>
        <el-button type="primary" size="small" @click="handleAddAttribute">
          <template #icon>
            <FaIcon name="i-iconoir:plus" />
          </template>
          添加规格
        </el-button>
      </div>

      <!-- 空状态 -->
      <div v-if="specAttributes.length === 0" class="empty-state">
        <FaIcon name="i-heroicons-solid:inbox" class="size-12 text-gray-300 mb-2" />
        <p>暂无规格，请点击"添加规格"添加商品规格</p>
      </div>

      <!-- 规格列表 -->
      <div v-else class="attribute-list">
        <div
          v-for="attr in specAttributes"
          :key="attr.id"
          class="attribute-item"
        >
          <div class="attribute-header">
            <el-input
              v-model="attr.name"
              placeholder="请输入规格名称，如：颜色"
              class="attribute-name-input"
              @input="generateSkus"
            />
            <el-button
              v-if="specAttributes.length > 1"
              type="danger"
              size="small"
              link
              @click="handleRemoveAttribute(attr.id)"
            >
              <template #icon>
                <FaIcon name="i-iconoir:trash" />
              </template>
              删除
            </el-button>
          </div>

          <div class="attribute-values">
            <div
              v-for="value in attr.values"
              :key="value.id"
              class="value-item"
            >
              <el-input
                v-model="value.name"
                placeholder="规格值"
                size="small"
                class="value-input"
                @input="generateSkus"
              />
              <el-button
                type="danger"
                size="small"
                class="value-remove-btn"
                @click="handleRemoveValue(attr.id, value.id)"
              >
                <el-icon><Minus /></el-icon>
              </el-button>
            </div>
            <el-button
              size="small"
              class="add-value-btn"
              @click="handleAddValue(attr.id)"
            >
              <el-icon><Plus /></el-icon>
              添加规格值
            </el-button>
          </div>
        </div>
      </div>
    </div>

    <!-- SKU 表格区域 -->
    <div v-if="skuList.length > 0" class="sku-table-section">
      <div class="sku-section-header">
        <span class="sku-section-title">
          <FaIcon name="i-heroicons-solid:chart-bar" class="size-4" />
          规格库存设置
        </span>
        <el-text type="info" size="small">
          {{ specAttributes.length > 0 && specAttributes.some(a => a.name && a.values.length) ? `共 ${skuList.length} 个 SKU` : '默认规格' }}
        </el-text>
      </div>

      <div class="sku-table-wrapper">
        <table class="sku-table">
          <thead>
            <tr>
              <th v-if="specAttributes.length === 0 || !specAttributes.some(a => a.name && a.values.length)" class="spec-col">
                规格
              </th>
              <th
                v-else
                v-for="attr in specAttributes.filter(a => a.name && a.values.length)"
                :key="attr.id"
                class="spec-col"
              >
                {{ attr.name }}
              </th>
              <th class="price-col">售价(元)</th>
              <th class="price-col">原价(元)</th>
              <th class="price-col">成本价(元)</th>
              <th class="stock-col">库存</th>
              <th class="stock-col">预警库存</th>
              <th class="image-col">SKU图片</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="sku in skuList" :key="sku.id">
              <td v-if="specAttributes.length === 0 || !specAttributes.some(a => a.name && a.values.length)" class="spec-col">
                默认规格
              </td>
              <td
                v-else
                v-for="attr in specAttributes.filter(a => a.name && a.values.length)"
                :key="attr.id"
                class="spec-col"
              >
                {{ sku.specs[attr.name] || '-' }}
              </td>
              <td class="price-col">
                <el-input-number
                  v-model="sku.price"
                  :min="0"
                  :precision="2"
                  :step="0.01"
                  controls-position="right"
                  class="w-full"
                />
              </td>
              <td class="price-col">
                <el-input-number
                  v-model="sku.originalPrice"
                  :min="0"
                  :precision="2"
                  :step="0.01"
                  controls-position="right"
                  class="w-full"
                  placeholder="原价"
                />
              </td>
              <td class="price-col">
                <el-input-number
                  v-model="sku.costPrice"
                  :min="0"
                  :precision="2"
                  :step="0.01"
                  controls-position="right"
                  class="w-full"
                  placeholder="成本价"
                />
              </td>
              <td class="stock-col">
                <el-input-number
                  v-model="sku.stock"
                  :min="0"
                  :step="1"
                  controls-position="right"
                  class="w-full"
                />
              </td>
              <td class="stock-col">
                <el-input-number
                  v-model="sku.lowStock"
                  :min="0"
                  :step="1"
                  controls-position="right"
                  class="w-full"
                />
              </td>
              <td class="image-col">
                <el-upload
                  :http-request="(options) => handleSkuImageUpload(options, sku.id)"
                  :show-file-list="false"
                  :before-upload="beforeSkuImageUpload"
                  accept="image/*"
                  class="sku-image-uploader"
                >
                  <img v-if="sku.image" :src="getFullUrl(sku.image)" class="sku-image-thumb">
                  <div v-else class="sku-image-placeholder">
                    <FaIcon name="i-iconoir:plus" />
                  </div>
                </el-upload>
                <el-button
                  v-if="sku.image"
                  type="danger"
                  size="small"
                  link
                  @click="sku.image = undefined"
                  class="sku-image-remove"
                >
                  <template #icon>
                    <FaIcon name="i-iconoir:trash" />
                  </template>
                </el-button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<style scoped>
.sku-manager {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

/* 规格选择区域 */
.sku-attributes-section {
  padding: 20px;
  background: hsl(var(--muted) / 0.12);
  border-radius: 8px;
}

.sku-section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}

.sku-section-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  font-weight: 600;
  color: hsl(var(--foreground) / 0.9);
}

.empty-state {
  padding: 32px 0;
  text-align: center;
  color: hsl(var(--foreground) / 0.4);
  font-size: 14px;
  background: hsl(var(--muted) / 0.15);
  border-radius: 8px;
  border: 1px dashed hsl(var(--border));
}

.attribute-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.attribute-item {
  padding: 16px;
  background: hsl(var(--background));
  border-radius: 8px;
  border: 1px solid hsl(var(--border));
}

.attribute-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 12px;
}

.attribute-name-input {
  width: 200px;
}

.attribute-values {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
}

.value-item {
  display: inline-flex;
  align-items: center;
  gap: 4px;
}

.value-input {
  width: 100px;
}

.value-remove-btn {
  width: 24px;
  height: 24px;
  padding: 0;
  min-width: 24px;
  border-radius: 4px;
}

.value-remove-btn :deep(.el-icon) {
  font-size: 14px;
}

.add-value-btn {
  height: 32px;
  padding: 0 12px;
  border-style: dashed;
  border-radius: 6px;
}

/* SKU 表格区域 */
.sku-table-section {
  padding: 20px;
  background: hsl(var(--muted) / 0.12);
  border-radius: 8px;
}

.sku-table-wrapper {
  overflow-x: auto;
  border-radius: 8px;
  border: 1px solid hsl(var(--border));
}

.sku-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 14px;
}

.sku-table th,
.sku-table td {
  border: 1px solid hsl(var(--border));
  padding: 12px;
  text-align: left;
}

.sku-table th {
  background: linear-gradient(180deg, hsl(var(--muted) / 0.2) 0%, hsl(var(--muted) / 0.1) 100%);
  color: hsl(var(--foreground) / 0.8);
  font-weight: 600;
}

.sku-table tbody tr:hover {
  background: hsl(var(--muted) / 0.15);
}

.spec-col {
  min-width: 100px;
}

.price-col {
  min-width: 130px;
}

.stock-col {
  min-width: 110px;
}

.image-col {
  min-width: 100px;
  width: 100px;
}

.sku-image-uploader :deep(.el-upload) {
  border: 1px dashed hsl(var(--border));
  border-radius: 6px;
  cursor: pointer;
  width: 50px;
  height: 50px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: hsl(var(--background));
}

.sku-image-thumb {
  width: 50px;
  height: 50px;
  object-fit: cover;
  display: block;
  border-radius: 4px;
}

.sku-image-placeholder {
  width: 50px;
  height: 50px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: hsl(var(--foreground) / 0.3);
  font-size: 18px;
}
</style>
