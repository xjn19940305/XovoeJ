<script setup lang="ts">
import type { FormInstance, FormRules } from 'element-plus'
import { ElMessage, ElMessageBox } from 'element-plus'
import productApi from '@/api/modules/product'
import categoryApi from '@/api/modules/category'
import dayjs from 'dayjs'

defineOptions({
  name: 'ShopProductList',
})

// 搜索表单
const searchForm = ref({
  keyword: '',
  categoryId: '',
  status: undefined as number | undefined,
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

// 对话框
const dialogVisible = ref(false)
const dialogTitle = ref('创建商品')
const dialogLoading = ref(false)
const editingProductId = ref<string>()
const formData = ref<Api.Product.CreateProductRequest>({
  name: '',
  description: '',
  price: 0,
  stock: 0,
  categoryId: '',
  coverImage: '',
  images: [],
  status: 1,
})

// 分类选项
const categoryOptions = ref<Api.Category.Category[]>([])

// 表单引用
const formRef = ref<FormInstance>()

// 表单验证规则
const rules: FormRules = {
  name: [
    { required: true, message: '请输入商品名称', trigger: 'blur' },
  ],
  price: [
    { required: true, message: '请输入商品价格', trigger: 'blur' },
    { type: 'number', min: 0, message: '价格不能小于0', trigger: 'blur' },
  ],
  stock: [
    { required: true, message: '请输入库存数量', trigger: 'blur' },
    { type: 'number', min: 0, message: '库存不能小于0', trigger: 'blur' },
  ],
}

// 订单状态映射
const statusMap: Record<number, { label: string; type: any }> = {
  0: { label: '下架', type: 'info' },
  1: { label: '上架', type: 'success' },
}

// 获取商品列表
async function getProductList() {
  loading.value = true
  try {
    const params: Api.Product.ProductListParams = {
      page: currentPage.value,
      pageSize: pageSize.value,
      ...searchForm.value,
    }
    const res = await productApi.getList(params)
    tableData.value = res.data.items
    total.value = res.data.total
  }
  finally {
    loading.value = false
  }
}

// 获取分类列表
async function getCategoryList() {
  const res = await categoryApi.getList()
  categoryOptions.value = res.data
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
    status: undefined,
  }
  currentPage.value = 1
  getProductList()
}

// 打开创建对话框
function handleCreate() {
  dialogTitle.value = '创建商品'
  editingProductId.value = undefined
  formData.value = {
    name: '',
    description: '',
    price: 0,
    stock: 0,
    categoryId: '',
    coverImage: '',
    images: [],
    status: 1,
  }
  dialogVisible.value = true
}

// 打开编辑对话框
async function handleEdit(row: Api.Product.Product) {
  dialogTitle.value = '修改商品'
  editingProductId.value = row.id
  dialogLoading.value = true
  dialogVisible.value = true
  try {
    const res = await productApi.getDetail(row.id)
    const data = res.data
    formData.value = {
      name: data.name,
      description: data.description,
      price: data.price,
      stock: data.stock,
      categoryId: data.categoryId,
      coverImage: data.coverImage,
      images: data.images,
      status: data.status,
    }
  }
  finally {
    dialogLoading.value = false
  }
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
async function handleToggleStatus(row: Api.Product.Product) {
  const newStatus = row.status === 1 ? 0 : 1
  await productApi.updateStatus(row.id, newStatus)
  ElMessage.success(newStatus === 1 ? '上架成功' : '下架成功')
  getProductList()
}

// 提交表单
async function handleSubmit() {
  await formRef.value?.validate()
  dialogLoading.value = true
  try {
    if (editingProductId.value) {
      await productApi.update(editingProductId.value, formData.value)
      ElMessage.success('修改成功')
    }
    else {
      await productApi.create(formData.value)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    getProductList()
  }
  finally {
    dialogLoading.value = false
  }
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

onMounted(() => {
  getProductList()
  getCategoryList()
})
</script>

<template>
  <div class="shop-product-list">
    <!-- 搜索表单 -->
    <FaCard class="mb-4">
      <div class="p-4">
        <el-form :model="searchForm" inline>
          <el-form-item label="关键字">
            <el-input v-model="searchForm.keyword" placeholder="商品名称" clearable />
          </el-form-item>
          <el-form-item label="分类">
            <el-select v-model="searchForm.categoryId" placeholder="请选择" clearable class="w-40">
              <el-option
                v-for="item in categoryOptions"
                :key="item.id"
                :label="item.name"
                :value="item.id"
              />
            </el-select>
          </el-form-item>
          <el-form-item label="状态">
            <el-select v-model="searchForm.status" placeholder="请选择" clearable class="w-32">
              <el-option label="上架" :value="1" />
              <el-option label="下架" :value="0" />
            </el-select>
          </el-form-item>
          <el-form-item>
            <FaButton @click="handleSearch">
              <template #icon>
                <FaIcon name="i-mage-icons:search" />
              </template>
              搜索
            </FaButton>
            <FaButton variant="ghost" @click="handleReset">
              重置
            </FaButton>
          </el-form-item>
        </el-form>
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
        <el-table-column prop="name" label="商品名称" min-width="150" />
        <el-table-column prop="categoryName" label="分类" width="120" />
        <el-table-column prop="price" label="价格" width="100" align="right">
          <template #default="{ row }">
            ¥{{ row.price.toFixed(2) }}
          </template>
        </el-table-column>
        <el-table-column prop="stock" label="库存" width="100" align="right" />
        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="statusMap[row.status]?.type">
              {{ statusMap[row.status]?.label }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="创建时间" min-width="180">
          <template #default="{ row }">
            {{ row.createdAt ? dayjs(row.createdAt).format('YYYY-MM-DD HH:mm:ss') : '-' }}
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
            <FaButton variant="ghost" size="sm" @click="handleToggleStatus(row)">
              {{ row.status === 1 ? '下架' : '上架' }}
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

    <!-- 创建/编辑对话框 -->
    <el-dialog
      v-model="dialogVisible"
      :title="dialogTitle"
      width="600px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="formRef"
        v-loading="dialogLoading"
        :model="formData"
        :rules="rules"
        label-width="100px"
      >
        <el-form-item label="商品名称" prop="name">
          <el-input v-model="formData.name" placeholder="请输入商品名称" />
        </el-form-item>
        <el-form-item label="商品描述">
          <el-input v-model="formData.description" type="textarea" :rows="3" placeholder="请输入商品描述" />
        </el-form-item>
        <el-form-item label="商品分类" prop="categoryId">
          <el-select v-model="formData.categoryId" placeholder="请选择分类" class="w-full">
            <el-option
              v-for="item in categoryOptions"
              :key="item.id"
              :label="item.name"
              :value="item.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="价格" prop="price">
          <el-input-number v-model="formData.price" :min="0" :precision="2" class="w-full" />
        </el-form-item>
        <el-form-item label="库存" prop="stock">
          <el-input-number v-model="formData.stock" :min="0" :precision="0" class="w-full" />
        </el-form-item>
        <el-form-item label="状态">
          <el-radio-group v-model="formData.status">
            <el-radio :value="1">上架</el-radio>
            <el-radio :value="0">下架</el-radio>
          </el-radio-group>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="dialogLoading" @click="handleSubmit">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>
