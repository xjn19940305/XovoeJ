<script setup lang="ts">
import { ElMessage, ElMessageBox } from 'element-plus'
import { useRouter } from 'vue-router'
import productApi from '@/api/modules/product'
import categoryApi from '@/api/modules/category'
import dayjs from 'dayjs'

defineOptions({
  name: 'ShopProductList',
})

const router = useRouter()

// 搜索表单
const searchForm = ref({
  keyword: '',
  categoryId: '',
  isHot: undefined as boolean | undefined,
  isNew: undefined as boolean | undefined,
  isRecommend: undefined as boolean | undefined,
})

// 表格数据
const tableData = ref<Api.Product.Product[]>([])
const loading = ref(false)
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(20)

// 选中的行
const selectedRows = ref<Api.Product.Product[]>([])
const selectedIds = computed(() => selectedRows.value.map(item => item.id))

// 分类树数据
const categoryTreeData = ref<Api.Category.CategoryTreeNode[]>([])

// 分类树选项
const categoryTreeOptions = computed(() => {
  function buildTree(items: Api.Category.CategoryTreeNode[]) {
    return items.map(item => ({
      value: item.id,
      label: item.name,
      children: item.children?.length ? buildTree(item.children) : undefined,
    }))
  }
  return [
    { value: '', label: '请选择' },
    ...buildTree(categoryTreeData.value),
  ]
})

// 获取商品列表
async function getProductList() {
  loading.value = true
  try {
    const params: Api.Product.ProductListParams = {
      page: currentPage.value,
      pageSize: pageSize.value,
    }

    // 只添加有值的搜索条件
    if (searchForm.value.keyword) {
      params.keyword = searchForm.value.keyword
    }
    if (searchForm.value.categoryId) {
      params.categoryId = searchForm.value.categoryId
    }
    if (searchForm.value.isHot !== undefined) {
      params.isHot = searchForm.value.isHot
    }
    if (searchForm.value.isNew !== undefined) {
      params.isNew = searchForm.value.isNew
    }
    if (searchForm.value.isRecommend !== undefined) {
      params.isRecommend = searchForm.value.isRecommend
    }

    const res = await productApi.getList(params)
    tableData.value = res.data.items
    total.value = res.data.total
  }
  finally {
    loading.value = false
  }
}

// 获取分类树
async function getCategoryTree() {
  const res = await categoryApi.getTree()
  categoryTreeData.value = res.data
}

// 搜索
function handleSearch() {
  currentPage.value = 1
  getProductList()
}

// 重置搜索
function handleReset() {
  searchForm.value = {
    keyword: '',
    categoryId: '',
    isHot: undefined,
    isNew: undefined,
    isRecommend: undefined,
  }
  currentPage.value = 1
  getProductList()
}

// 打开创建页面
function handleCreate() {
  router.push('/shop/product/create')
}

// 打开编辑页面
function handleEdit(row: Api.Product.Product) {
  router.push(`/shop/product/${row.id}/edit`)
}

// 删除商品
async function handleDelete(id: string) {
  await ElMessageBox.confirm('确定要删除该商品吗？', '提示', {
    type: 'warning',
  })
  await productApi.delete(id)
  ElMessage.success('删除成功')
  getProductList()
}

// 批量删除
async function handleBatchDelete() {
  if (selectedIds.value.length === 0) {
    ElMessage.warning('请选择要删除的商品')
    return
  }
  await ElMessageBox.confirm(`确定要删除选中的 ${selectedIds.value.length} 个商品吗？`, '提示', {
    type: 'warning',
  })
  for (const id of selectedIds.value) {
    await productApi.delete(id)
  }
  ElMessage.success('删除成功')
  getProductList()
}

// 上架/下架
async function handleToggleEnabled(row: Api.Product.Product) {
  const newData = { ...row, isEnabled: !row.isEnabled }
  await productApi.update(row.id, {
    categoryId: newData.categoryId,
    name: newData.name,
    subtitle: newData.subtitle,
    description: newData.description,
    isEnabled: newData.isEnabled,
    isHot: newData.isHot,
    isNew: newData.isNew,
    isRecommend: newData.isRecommend,
    skus: newData.skus.map(sku => ({
      skuCode: sku.skuCode,
      specs: sku.specs,
      price: sku.price,
      originalPrice: sku.originalPrice,
      costPrice: sku.costPrice,
      stock: sku.stock,
      lowStock: sku.lowStock,
      image: sku.image,
    })),
  })
  ElMessage.success(newData.isEnabled ? '上架成功' : '下架成功')
  getProductList()
}

// 分页改变
function handlePageChange(page: number) {
  currentPage.value = page
  getProductList()
}

function handleSizeChange(size: number) {
  pageSize.value = size
  currentPage.value = 1
  getProductList()
}

// 格式化价格区间
function formatPriceRange(product: Api.Product.Product) {
  if (product.minPrice === undefined || product.maxPrice === undefined)
    return '-'
  if (product.minPrice === product.maxPrice)
    return `¥${product.minPrice.toFixed(2)}`
  return `¥${product.minPrice.toFixed(2)} - ¥${product.maxPrice.toFixed(2)}`
}

onMounted(() => {
  getCategoryTree()
  getProductList()
})
</script>

<template>
  <div class="shop-product-list">
    <!-- 搜索表单 -->
    <FaCard class="search-card mb-4">
      <div class="search-header">
        <div class="search-title">
          <FaIcon name="i-heroicons-solid:magnifying-glass" class="size-5" />
          <span>商品筛选</span>
        </div>
      </div>
      <div class="search-body">
        <div class="search-grid">
          <div class="search-field">
            <label class="search-label">关键字</label>
            <el-input
              v-model="searchForm.keyword"
              placeholder="搜索商品名称"
              clearable
            >
              <template #prefix>
                <FaIcon name="i-heroicons-solid:tag" class="size-4 text-gray-400" />
              </template>
            </el-input>
          </div>
          <div class="search-field">
            <label class="search-label">商品分类</label>
            <el-tree-select
              v-model="searchForm.categoryId"
              :data="categoryTreeOptions"
              placeholder="选择商品分类"
              clearable
              check-strictly
              class="w-full"
            />
          </div>
          <div class="search-field">
            <label class="search-label">热门商品</label>
            <el-select v-model="searchForm.isHot" placeholder="全部" clearable class="w-full">
              <el-option label="是" :value="true">
                <span class="flex items-center gap-2">
                  <FaIcon name="i-heroicons-solid:fire" class="size-4 text-orange-500" />
                  是
                </span>
              </el-option>
              <el-option label="否" :value="false">否</el-option>
            </el-select>
          </div>
          <div class="search-field">
            <label class="search-label">新品</label>
            <el-select v-model="searchForm.isNew" placeholder="全部" clearable class="w-full">
              <el-option label="是" :value="true">
                <span class="flex items-center gap-2">
                  <FaIcon name="i-heroicons-solid:sparkles" class="size-4 text-green-500" />
                  是
                </span>
              </el-option>
              <el-option label="否" :value="false">否</el-option>
            </el-select>
          </div>
          <div class="search-field">
            <label class="search-label">推荐商品</label>
            <el-select v-model="searchForm.isRecommend" placeholder="全部" clearable class="w-full">
              <el-option label="是" :value="true">
                <span class="flex items-center gap-2">
                  <FaIcon name="i-heroicons-solid:thumb-up" class="size-4 text-blue-500" />
                  是
                </span>
              </el-option>
              <el-option label="否" :value="false">否</el-option>
            </el-select>
          </div>
        </div>
      </div>
      <div class="search-footer">
        <FaButton @click="handleSearch">
          <template #icon>
            <FaIcon name="i-heroicons-solid:magnifying-glass" />
          </template>
          搜索
        </FaButton>
        <FaButton class="search-reset-btn" @click="handleReset">
          <template #icon>
            <FaIcon name="i-heroicons-solid:arrow-path" />
          </template>
          重置
        </FaButton>
      </div>
    </FaCard>

    <!-- 操作按钮 -->
    <div class="mb-4 flex items-center justify-between">
      <div class="flex gap-2">
        <FaButton @click="handleCreate">
          <template #icon>
            <FaIcon name="i-iconoir:plus" />
          </template>
          创建商品
        </FaButton>
        <FaButton variant="destructive" :disabled="selectedIds.length === 0" @click="handleBatchDelete">
          <template #icon>
            <FaIcon name="i-iconoir:trash" />
          </template>
          批量删除
        </FaButton>
      </div>
    </div>

    <!-- 表格 -->
    <FaCard>
      <el-table
        v-loading="loading"
        :data="tableData"
        @selection-change="(rows) => selectedRows = rows"
      >
        <el-table-column type="selection" width="55" />
        <el-table-column prop="name" label="商品名称" min-width="180" />
        <el-table-column prop="categoryName" label="分类" width="120" />
        <el-table-column label="价格区间" width="150" align="right">
          <template #default="{ row }">
            {{ formatPriceRange(row) }}
          </template>
        </el-table-column>
        <el-table-column prop="totalStock" label="库存" width="100" align="right" />
        <el-table-column prop="salesCount" label="销量" width="100" align="right" />
        <el-table-column label="状态" width="180" align="center">
          <template #default="{ row }">
            <el-tag v-if="row.isHot" size="small" type="danger" class="mr-1">热门</el-tag>
            <el-tag v-if="row.isNew" size="small" type="success" class="mr-1">新品</el-tag>
            <el-tag v-if="row.isRecommend" size="small" type="warning">推荐</el-tag>
            <el-tag v-if="!row.isEnabled" size="small" type="info" class="ml-1">已下架</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="创建时间" width="180">
          <template #default="{ row }">
            {{ dayjs(row.createdAt).format('YYYY-MM-DD HH:mm:ss') }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <FaButton variant="ghost" size="sm" @click="handleEdit(row)">
              <template #icon>
                <FaIcon name="i-iconoir:edit-pencil" />
              </template>
              编辑
            </FaButton>
            <FaButton variant="ghost" size="sm" @click="handleToggleEnabled(row)">
              {{ row.isEnabled ? '下架' : '上架' }}
            </FaButton>
            <FaButton variant="ghost" size="sm" class="text-red-500" @click="handleDelete(row.id)">
              <template #icon>
                <FaIcon name="i-iconoir:trash" />
              </template>
              删除
            </FaButton>
          </template>
        </el-table-column>
      </el-table>

      <!-- 分页 -->
      <div class="mt-4 flex justify-end">
        <el-pagination
          :current-page="currentPage"
          :page-size="pageSize"
          :total="total"
          :page-sizes="[20, 50, 100]"
          layout="total, sizes, prev, pager, next, jumper"
          @current-change="handlePageChange"
          @size-change="handleSizeChange"
        />
      </div>
    </FaCard>
  </div>
</template>
