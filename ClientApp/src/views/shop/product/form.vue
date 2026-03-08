<script setup lang="ts">
import type { IDomEditor, IEditorConfig, IToolbarConfig } from '@wangeditor/editor'
import type { FormInstance, FormRules } from 'element-plus'
import { createEditor, createToolbar } from '@wangeditor/editor'
import { ElMessage } from 'element-plus'
import { useRoute, useRouter } from 'vue-router'
import categoryApi from '@/api/modules/category'
import productApi from '@/api/modules/product'
import FormSection from '@/ui/components/FormSection/index.vue'
import ProductAttributes from './components/ProductAttributes.vue'
import ProductImageUpload from './components/ProductImageUpload.vue'
import SkuManager from './components/SkuManager.vue'
import '@wangeditor/editor/dist/css/style.css'

defineOptions({
  name: 'ShopProductForm',
})

const route = useRoute()
const router = useRouter()
const isEdit = computed(() => !!route.params.id)
const productId = computed(() => route.params.id as string)

// 富文本编辑器
const editorRef = ref()
const toolbarRef = ref()
const toolbarConfig: Partial<IToolbarConfig> = {}
const editorConfig: Partial<IEditorConfig> = {
  placeholder: '请输入商品详情内容...',
  MENU_CONF: {
    uploadImage: {
      customUpload: customUploadImage,
    },
  },
}

// 编辑器初始化
let editor: IDomEditor | null = null
let toolbar: any = null

// 页面标题
const pageTitle = computed(() => isEdit.value ? '编辑商品' : '创建商品')

// 表单引用
const formRef = ref<FormInstance>()
const loading = ref(false)
const saving = ref(false)

// 商品详情（只读，用于编辑时显示）
const productDetail = ref<Api.Product.Product | null>(null)

// SKU 数据（与 SkuManager 组件保持一致）
interface SkuItem {
  id: string
  specs: Record<string, string>
  price: number
  originalPrice?: number
  costPrice?: number
  stock: number
  lowStock: number
  image?: string
}

interface CategoryTreeOption {
  value: string
  label: string
  children?: CategoryTreeOption[]
}

const skuList = ref<SkuItem[]>([])

// 分类树数据
const categoryTreeData = ref<Api.Category.CategoryTreeNode[]>([])

// 自定义上传图片（用于富文本编辑器）
async function customUploadImage(file: File, insertFn: (url: string, alt: string, href: string) => void) {
  const formData = new FormData()
  formData.append('file', file)

  const baseURL = import.meta.env.VITE_APP_API_BASEURL || 'https://localhost:7216'

  try {
    // 上传文件
    const uploadResponse = await fetch(`${baseURL}/api/files/upload`, {
      method: 'POST',
      body: formData,
    })
    const uploadResult = await uploadResponse.json()

    if (uploadResult.key) {
      // 获取永久访问的文件 URL（通过后端代理）
      const urlResponse = await fetch(`${baseURL}/api/files/view?key=${encodeURIComponent(uploadResult.key)}`)
      const urlResult = await urlResponse.json()

      if (urlResult.url) {
        insertFn(urlResult.url, file.name, urlResult.url)
      }
      else {
        ElMessage.error('获取文件URL失败')
      }
    }
    else {
      ElMessage.error(uploadResult.message || '上传失败')
    }
  }
  catch {
    ElMessage.error('上传失败')
  }
}

// 编辑器销毁
onBeforeUnmount(() => {
  if (editor == null) {
    return
  }
  editor.destroy()
  if (toolbar) {
    toolbar.destroy()
  }
})

// 初始化编辑器
onMounted(async () => {
  getCategoryTree()
  if (isEdit.value) {
    await getProductDetail()
  }

  // 等待 DOM 渲染后初始化编辑器
  await nextTick()

  if (toolbarRef.value && editorRef.value) {
    // 先创建编辑器
    editor = createEditor({
      selector: editorRef.value,
      // eslint-disable-next-line ts/no-use-before-define
      html: formData.value.detail || '',
      config: editorConfig,
      mode: 'default',
    })

    // 再创建工具栏，传入编辑器和选择器
    toolbar = createToolbar({
      editor,
      selector: toolbarRef.value,
      config: toolbarConfig,
      mode: 'default',
    })

    // 监听内容变化更新表单数据
    editor.on('change', () => {
      if (editor) {
        // eslint-disable-next-line ts/no-use-before-define
        formData.value.detail = editor.getHtml()
      }
    })
    editor.on('unFullScreen', () => {
      toolbar?.setEditMode('default')
    })
  }
})

// 表单数据
const formData = ref<{
  categoryId: string
  name: string
  subtitle: string
  description: string
  images: string[]
  detail: string
  tags: string[]
  isHot: boolean
  isNew: boolean
  isRecommend: boolean
  isEnabled: boolean
  skus: Api.Product.CreateProductSkuRequest[]
}>({
  categoryId: '',
  name: '',
  subtitle: '',
  description: '',
  images: [],
  detail: '',
  tags: [],
  isHot: false,
  isNew: false,
  isRecommend: false,
  isEnabled: true,
  skus: [],
})

// 分类树选项
const categoryTreeOptions = computed(() => {
  function buildTree(items: Api.Category.CategoryTreeNode[]): CategoryTreeOption[] {
    return items.map(item => ({
      value: item.id,
      label: item.name,
      children: item.children?.length ? buildTree(item.children) : undefined,
    }))
  }
  return buildTree(categoryTreeData.value)
})

// 表单验证规则
const rules: FormRules = {
  categoryId: [
    { required: true, message: '请选择商品分类', trigger: 'change' },
  ],
  name: [
    { required: true, message: '请输入商品名称', trigger: 'blur' },
    { max: 256, message: '商品名称长度不能超过256个字符', trigger: 'blur' },
  ],
  subtitle: [
    { max: 256, message: '副标题长度不能超过256个字符', trigger: 'blur' },
  ],
  description: [
    { max: 1000, message: '商品描述长度不能超过1000个字符', trigger: 'blur' },
  ],
}

// 获取分类树
async function getCategoryTree() {
  const res = await categoryApi.getTree()
  categoryTreeData.value = res.data
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

// 处理富文本内容中的图片 URL，转换为完整 URL（用于编辑器显示）
function convertDetailContentForDisplay(content: string): string {
  if (!content) { return content }

  const baseURL = import.meta.env.VITE_APP_API_BASEURL || 'https://localhost:7216'
  const proxyPrefix = `${baseURL}/api/files/proxy?key=`

  // 匹配相对路径的图片（如 20260223/xxx.jpg），将其转换为完整 URL
  // 排除已经是完整 URL 的情况
  return content.replace(/<img[^>]+src=["']([^"']+)["'][^>]*>/g, (match, src) => {
    // 如果已经是完整 URL 或 base64，不处理
    if (src.startsWith('http://') || src.startsWith('https://') || src.startsWith('data:')) {
      return match
    }
    // 相对路径转换为完整 URL
    const fullUrl = `${proxyPrefix}${encodeURIComponent(src)}`
    return match.replace(`src="${src}"`, `src="${fullUrl}"`).replace(`src='${src}'`, `src='${fullUrl}'`)
  })
}

// 处理富文本内容中的图片 URL，转换为相对路径（用于提交）
function processDetailContent(content: string): string {
  if (!content) { return content }

  const baseURL = import.meta.env.VITE_APP_API_BASEURL || 'https://localhost:7216'
  const proxyPrefix = `${baseURL}/api/files/proxy?key=`

  // 替换所有代理图片 URL 为相对路径
  return content.replace(new RegExp(`${proxyPrefix.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}([^"&'\\s>]+)`, 'g'), (_match, key) => {
    return decodeURIComponent(key)
  })
}

// 获取商品详情
/* eslint-disable no-console */
async function getProductDetail() {
  if (!productId.value) { return }
  loading.value = true
  try {
    const res = await productApi.getDetail(productId.value)
    const data = res.data
    productDetail.value = data

    // 合并主图和图片列表
    const allImages: string[] = []
    if (data.mainImage) {
      const converted = convertToRelativePath(data.mainImage)
      console.log('mainImage 原值:', data.mainImage)
      console.log('mainImage 转换后:', converted)
      allImages.push(converted || data.mainImage)
    }
    if (data.images?.length) {
      allImages.push(...data.images.map((img) => {
        const converted = convertToRelativePath(img)
        console.log('image 原值:', img, '转换后:', converted)
        return converted || img
      }))
    }
    console.log('allImages 最终值:', allImages)

    formData.value = {
      categoryId: data.categoryId,
      name: data.name,
      subtitle: data.subtitle ?? '',
      description: data.description ?? '',
      images: allImages,
      // 将富文本中的相对路径图片转换为完整 URL 用于显示
      detail: convertDetailContentForDisplay(data.detail ?? ''),
      tags: data.tags || [],
      isHot: data.isHot,
      isNew: data.isNew,
      isRecommend: data.isRecommend,
      isEnabled: data.isEnabled,
      skus: data.skus.map(sku => ({
        skuCode: sku.skuCode,
        specs: sku.specs ?? {},
        price: sku.price,
        originalPrice: sku.originalPrice,
        costPrice: sku.costPrice,
        stock: sku.stock,
        lowStock: sku.lowStock,
        image: sku.image ?? '',
      })),
    }

    // 解析 SKU 数据到列表（供 SkuManager 使用）
    skuList.value = data.skus.map((sku) => {
      let specs: Record<string, string> = {}
      if (typeof sku.specs === 'string') {
        try {
          specs = JSON.parse(sku.specs)
        }
        catch {
          specs = {}
        }
      }
      else {
        specs = sku.specs || {}
      }

      return {
        id: sku.skuCode,
        specs,
        price: sku.price,
        originalPrice: sku.originalPrice,
        costPrice: sku.costPrice,
        stock: sku.stock,
        lowStock: sku.lowStock,
        image: convertToRelativePath(sku.image),
      }
    })
  }
  finally {
    loading.value = false
  }
}

// 提交表单
async function handleSubmit() {
  await formRef.value?.validate()

  // 检查图片
  if (formData.value.images.length === 0) {
    ElMessage.error('请至少上传一张商品图片')
    return
  }

  // 同步 SKU 数据
  formData.value.skus = skuList.value.map(sku => ({
    skuCode: sku.id,
    specs: sku.specs,
    price: sku.price,
    originalPrice: sku.originalPrice,
    costPrice: sku.costPrice,
    stock: sku.stock,
    lowStock: sku.lowStock,
    image: sku.image,
  }))

  // 验证 SKU
  if (formData.value.skus.length === 0) {
    ElMessage.error('请至少添加一个SKU')
    return
  }

  // 检查 SKU 价格
  for (const sku of formData.value.skus) {
    if (sku.price < 0) {
      ElMessage.error('SKU 售价不能为负数')
      return
    }
  }

  saving.value = true
  try {
    console.log('提交前 formData.value.images:', formData.value.images)

    // 分离主图和图片列表
    const mainImage = formData.value.images[0]
    const images = formData.value.images.slice(1)

    console.log('提交时 mainImage:', mainImage)
    console.log('提交时 images:', images)

    // 处理富文本内容中的图片 URL，转换为相对路径
    const detail = processDetailContent(formData.value.detail || '')

    if (isEdit.value) {
      // 编辑时提交数据，包含 SKU
      const updateData = {
        categoryId: formData.value.categoryId,
        name: formData.value.name,
        subtitle: formData.value.subtitle || undefined,
        description: formData.value.description || undefined,
        mainImage: mainImage || undefined,
        images,
        detail: detail || undefined,
        tags: formData.value.tags,
        isEnabled: formData.value.isEnabled,
        isHot: formData.value.isHot,
        isNew: formData.value.isNew,
        isRecommend: formData.value.isRecommend,
        skus: formData.value.skus,
      }
      await productApi.update(productId.value, updateData)
      ElMessage.success('修改成功')
    }
    else {
      // 创建时提交完整数据
      const createData = {
        categoryId: formData.value.categoryId,
        name: formData.value.name,
        subtitle: formData.value.subtitle,
        description: formData.value.description,
        mainImage,
        images,
        detail,
        tags: formData.value.tags,
        isHot: formData.value.isHot,
        isNew: formData.value.isNew,
        isRecommend: formData.value.isRecommend,
        skus: formData.value.skus,
      }
      await productApi.create(createData)
      ElMessage.success('创建成功')
    }
    router.back()
  }
  finally {
    saving.value = false
  }
}

// 取消
/* eslint-enable no-console */
/* eslint-enable style/max-statements-per-line */
function handleCancel() {
  router.back()
}
</script>

<template>
  <div class="shop-product-form p-4">
    <FaCard class="product-form-card">
      <template #header>
        <div class="form-card-header">
          <div class="form-header-left">
            <div class="form-icon-wrapper">
              <FaIcon name="i-heroicons-solid:shopping-cart" class="size-5" />
            </div>
            <div>
              <h2 class="form-title">
                {{ pageTitle }}
              </h2>
              <p class="form-subtitle">
                填写商品信息并设置规格库存
              </p>
            </div>
          </div>
        </div>
      </template>

      <el-form
        ref="formRef"
        v-loading="loading"
        :model="formData"
        :rules="rules"
        label-width="120px"
        class="max-w-5xl"
      >
        <!-- 基本信息 -->
        <FormSection title="基本信息" icon="i-heroicons-solid:information-circle">
          <el-row :gutter="20">
            <el-col :span="12">
              <el-form-item label="商品分类" prop="categoryId">
                <el-tree-select
                  v-model="formData.categoryId"
                  :data="categoryTreeOptions"
                  placeholder="请选择分类"
                  check-strictly
                  clearable
                  class="w-full"
                />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="商品名称" prop="name">
                <el-input v-model="formData.name" placeholder="请输入商品名称" />
              </el-form-item>
            </el-col>
          </el-row>

          <el-form-item label="副标题" prop="subtitle">
            <el-input v-model="formData.subtitle" placeholder="请输入副标题" />
          </el-form-item>

          <el-form-item label="商品描述" prop="description">
            <el-input v-model="formData.description" type="textarea" :rows="3" placeholder="请输入商品描述" />
          </el-form-item>

          <el-form-item label="商品标签">
            <el-input-tag
              v-model="formData.tags"
              placeholder="输入标签后按回车添加"
              :max-tags="10"
              class="w-full"
            />
          </el-form-item>

          <el-form-item label="商品属性">
            <ProductAttributes
              v-model:is-hot="formData.isHot"
              v-model:is-new="formData.isNew"
              v-model:is-recommend="formData.isRecommend"
              v-model:is-enabled="formData.isEnabled"
            />
          </el-form-item>
        </FormSection>

        <!-- 图片信息 -->
        <FormSection title="商品图片" icon="i-heroicons-solid:photo">
          <el-form-item label="图片">
            <ProductImageUpload v-model="formData.images" />
          </el-form-item>
        </FormSection>

        <!-- SKU 管理 -->
        <FormSection
          title="SKU 管理"
          icon="i-heroicons-solid:cube"
          :tips="isEdit ? '提示：修改规格时会智能匹配已有的 SKU，保留价格和库存信息。' : '提示：添加商品规格（如颜色、尺寸）和规格值后，系统将自动生成所有 SKU 组合。'"
        >
          <SkuManager v-model="skuList" :is-edit="isEdit" />
        </FormSection>

        <!-- 商品详情 -->
        <FormSection title="商品详情" icon="i-heroicons-solid:document-text">
          <div class="editor-wrapper">
            <div ref="toolbarRef" class="editor-toolbar" />
            <div ref="editorRef" class="editor-content" />
          </div>
        </FormSection>
      </el-form>

      <!-- 底部按钮 -->
      <div class="form-footer">
        <div class="form-footer-actions">
          <FaButton :loading="saving" @click="handleSubmit">
            <template #icon>
              <FaIcon name="i-iconoir:check" />
            </template>
            保存
          </FaButton>
          <FaButton variant="ghost" @click="handleCancel">
            取消
          </FaButton>
        </div>
      </div>
    </FaCard>
  </div>
</template>

<style scoped>
/* 商品表单卡片 */
.product-form-card {
  border-radius: 12px;
  overflow: hidden;
}

/* 表单头部 */
.form-card-header {
  display: flex;
  align-items: center;
}

.form-header-left {
  display: flex;
  align-items: center;
  gap: 16px;
}

.form-icon-wrapper {
  width: 48px;
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, hsl(var(--primary)) 0%, hsl(var(--primary) / 0.8) 100%);
  border-radius: 12px;
  color: white;
}

.form-title {
  font-size: 18px;
  font-weight: 600;
  color: hsl(var(--foreground));
}

.form-subtitle {
  font-size: 13px;
  color: hsl(var(--foreground) / 0.5);
  margin-top: 2px;
}

/* 底部按钮 */
.form-footer {
  padding: 16px 20px;
  border-top: 1px solid hsl(var(--border));
  background: hsl(var(--muted) / 0.1);
}

.form-footer-actions {
  display: flex;
  gap: 12px;
}

/* 编辑器 */
.editor-wrapper {
  border: 1px solid hsl(var(--border));
  border-radius: 8px;
  overflow: hidden;
  background: white;
}

.editor-toolbar {
  border-bottom: 1px solid hsl(var(--border));
  padding: 8px;
}

.editor-content {
  min-height: 300px;
}

.shop-product-form :deep(.el-form-item__label) {
  font-weight: 500;
  color: hsl(var(--foreground) / 0.8);
}

.shop-product-form :deep(.el-input-number) {
  width: 100%;
}
</style>

<style>
/* wangeditor 全局样式 */
.shop-product-form .w-e-text-container {
  background-color: #fff;
}
.shop-product-form .w-e-toolbar {
  background-color: #fff;
}
</style>
