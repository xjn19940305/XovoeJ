<script setup lang="ts">
import type { FormInstance, FormRules } from 'element-plus'
import dayjs from 'dayjs'
import { ElMessage, ElMessageBox } from 'element-plus'
import bannerApi from '@/api/modules/banner'

defineOptions({
  name: 'ContentBannerList',
})

const searchForm = ref({
  keyword: '',
  isEnabled: undefined as boolean | undefined,
  page: 1,
  pageSize: 10,
})

const tableData = ref<Api.Banner.BannerItem[]>([])
const loading = ref(false)
const total = ref(0)

const dialogVisible = ref(false)
const dialogLoading = ref(false)
const dialogTitle = ref('新建轮播图')
const formRef = ref<FormInstance>()
const editingId = ref<string>()
const formData = ref<Api.Banner.SaveBannerRequest>(createDefaultForm())

const rules: FormRules = {
  title: [
    { required: true, message: '请输入轮播图标题', trigger: 'blur' },
    { max: 128, message: '标题长度不能超过 128 个字符', trigger: 'blur' },
  ],
  imageUrl: [
    { required: true, message: '请输入轮播图图片地址', trigger: 'blur' },
  ],
}

const statusOptions = [
  { label: '全部', value: undefined },
  { label: '启用', value: true },
  { label: '禁用', value: false },
]

function createDefaultForm(): Api.Banner.SaveBannerRequest {
  return {
    title: '',
    subtitle: '',
    imageUrl: '',
    linkUrl: '',
    sortOrder: 0,
    isEnabled: true,
    startTime: '',
    endTime: '',
    remark: '',
  }
}

async function getList() {
  loading.value = true
  try {
    const res = await bannerApi.getList({
      page: searchForm.value.page,
      pageSize: searchForm.value.pageSize,
      keyword: searchForm.value.keyword || undefined,
      isEnabled: searchForm.value.isEnabled,
    })
    tableData.value = res.data.items
    total.value = res.data.total
  }
  catch {
    tableData.value = []
    total.value = 0
  }
  finally {
    loading.value = false
  }
}

function handleSearch() {
  searchForm.value.page = 1
  getList()
}

function handleReset() {
  searchForm.value.keyword = ''
  searchForm.value.isEnabled = undefined
  searchForm.value.page = 1
  getList()
}

function handleCreate() {
  editingId.value = undefined
  dialogTitle.value = '新建轮播图'
  formData.value = createDefaultForm()
  dialogVisible.value = true
}

async function handleEdit(row: Api.Banner.BannerItem) {
  dialogLoading.value = true
  dialogTitle.value = '编辑轮播图'
  editingId.value = row.id
  dialogVisible.value = true
  try {
    const res = await bannerApi.getById(row.id)
    formData.value = {
      title: res.data.title,
      subtitle: res.data.subtitle || '',
      imageUrl: res.data.imageUrl,
      linkUrl: res.data.linkUrl || '',
      sortOrder: res.data.sortOrder,
      isEnabled: res.data.isEnabled,
      startTime: res.data.startTime || '',
      endTime: res.data.endTime || '',
      remark: res.data.remark || '',
    }
  }
  finally {
    dialogLoading.value = false
  }
}

async function handleSubmit() {
  await formRef.value?.validate()
  dialogLoading.value = true
  try {
    if (editingId.value) {
      await bannerApi.update(editingId.value, formData.value)
      ElMessage.success('轮播图更新成功')
    }
    else {
      await bannerApi.create(formData.value)
      ElMessage.success('轮播图创建成功')
    }
    dialogVisible.value = false
    getList()
  }
  finally {
    dialogLoading.value = false
  }
}

async function handleToggleStatus(row: Api.Banner.BannerItem) {
  const nextEnabled = !row.isEnabled
  const action = nextEnabled ? '启用' : '禁用'
  await ElMessageBox.confirm(`确定要${action}该轮播图吗？`, '提示', {
    type: 'warning',
  })
  await bannerApi.updateStatus(row.id, { isEnabled: nextEnabled })
  ElMessage.success(`${action}成功`)
  getList()
}

async function handleDelete(row: Api.Banner.BannerItem) {
  await ElMessageBox.confirm(`确定删除轮播图“${row.title}”吗？删除后不可恢复。`, '提示', {
    type: 'warning',
  })
  await bannerApi.remove(row.id)
  ElMessage.success('删除成功')
  if (tableData.value.length === 1 && searchForm.value.page > 1) {
    searchForm.value.page -= 1
  }
  getList()
}

function handleCurrentChange(page: number) {
  searchForm.value.page = page
  getList()
}

function handleSizeChange(pageSize: number) {
  searchForm.value.pageSize = pageSize
  searchForm.value.page = 1
  getList()
}

onMounted(() => {
  getList()
})
</script>

<template>
  <div class="content-banner-list p-4">
    <FaCard class="search-card mb-4">
      <div class="search-header">
        <div class="search-title">
          <FaIcon name="i-heroicons-solid:magnifying-glass" class="size-5" />
          <span>轮播图筛选</span>
        </div>
      </div>
      <div class="search-body">
        <div class="search-grid">
          <div class="search-field">
            <label class="search-label">关键词</label>
            <el-input v-model="searchForm.keyword" placeholder="搜索标题、副标题或跳转链接" clearable />
          </div>
          <div class="search-field">
            <label class="search-label">状态</label>
            <el-select v-model="searchForm.isEnabled" class="w-full" placeholder="请选择状态" clearable>
              <el-option
                v-for="item in statusOptions"
                :key="item.label"
                :label="item.label"
                :value="item.value"
              />
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

    <div class="mb-4 flex items-center justify-between">
      <div class="section-title">
        <h3>轮播图管理</h3>
        <p>维护商城首页或专题页的轮播图内容。</p>
      </div>
      <FaButton @click="handleCreate">
        <template #icon>
          <FaIcon name="i-iconoir:plus" />
        </template>
        新建轮播图
      </FaButton>
    </div>

    <FaCard>
      <el-table
        v-loading="loading"
        :data="tableData"
      >
        <el-table-column label="图片" width="120">
          <template #default="{ row }">
            <el-image
              :src="row.imageUrl"
              fit="cover"
              preview-teleported
              :preview-src-list="[row.imageUrl]"
              class="banner-cover"
            />
          </template>
        </el-table-column>
        <el-table-column prop="title" label="标题" min-width="180" show-overflow-tooltip />
        <el-table-column prop="subtitle" label="副标题" min-width="180" show-overflow-tooltip />
        <el-table-column prop="linkUrl" label="跳转链接" min-width="220" show-overflow-tooltip />
        <el-table-column prop="sortOrder" label="排序" width="90" />
        <el-table-column label="状态" width="90">
          <template #default="{ row }">
            <el-tag :type="row.isEnabled ? 'success' : 'info'" size="small">
              {{ row.isEnabled ? '启用' : '禁用' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="有效期" min-width="220">
          <template #default="{ row }">
            <span v-if="row.startTime || row.endTime">
              {{ row.startTime ? dayjs(row.startTime).format('YYYY-MM-DD HH:mm') : '不限' }}
              -
              {{ row.endTime ? dayjs(row.endTime).format('YYYY-MM-DD HH:mm') : '不限' }}
            </span>
            <span v-else class="text-gray-400">长期有效</span>
          </template>
        </el-table-column>
        <el-table-column label="更新时间" width="170">
          <template #default="{ row }">
            {{ dayjs(row.updatedAt || row.createdAt).format('YYYY-MM-DD HH:mm:ss') }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="240" fixed="right">
          <template #default="{ row }">
            <FaButton variant="ghost" size="sm" @click="handleEdit(row)">
              <template #icon>
                <FaIcon name="i-iconoir:edit-pencil" />
              </template>
              编辑
            </FaButton>
            <FaButton variant="ghost" size="sm" @click="handleToggleStatus(row)">
              <template #icon>
                <FaIcon :name="row.isEnabled ? 'i-iconoir:lock' : 'i-iconoir:unlock'" />
              </template>
              {{ row.isEnabled ? '禁用' : '启用' }}
            </FaButton>
            <FaButton variant="ghost" size="sm" class="text-red-500" @click="handleDelete(row)">
              <template #icon>
                <FaIcon name="i-iconoir:trash" />
              </template>
              删除
            </FaButton>
          </template>
        </el-table-column>
      </el-table>

      <div class="mt-4 flex justify-end">
        <el-pagination
          background
          layout="total, sizes, prev, pager, next"
          :current-page="searchForm.page"
          :page-size="searchForm.pageSize"
          :page-sizes="[10, 20, 50]"
          :total="total"
          @current-change="handleCurrentChange"
          @size-change="handleSizeChange"
        />
      </div>
    </FaCard>

    <el-dialog
      v-model="dialogVisible"
      :title="dialogTitle"
      width="720px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="formRef"
        v-loading="dialogLoading"
        :model="formData"
        :rules="rules"
        label-width="110px"
      >
        <el-form-item label="轮播图标题" prop="title">
          <el-input v-model="formData.title" placeholder="请输入轮播图标题" />
        </el-form-item>
        <el-form-item label="副标题">
          <el-input v-model="formData.subtitle" placeholder="请输入副标题" />
        </el-form-item>
        <el-form-item label="图片地址" prop="imageUrl">
          <el-input v-model="formData.imageUrl" placeholder="请输入图片 URL" />
        </el-form-item>
        <el-form-item v-if="formData.imageUrl" label="图片预览">
          <el-image :src="formData.imageUrl" fit="cover" class="banner-preview" />
        </el-form-item>
        <el-form-item label="跳转链接">
          <el-input v-model="formData.linkUrl" placeholder="请输入点击跳转链接" />
        </el-form-item>
        <el-form-item label="排序值">
          <el-input-number v-model="formData.sortOrder" :min="0" :max="9999" />
        </el-form-item>
        <el-form-item label="状态">
          <el-switch v-model="formData.isEnabled" active-text="启用" inactive-text="禁用" />
        </el-form-item>
        <el-form-item label="开始时间">
          <el-date-picker
            v-model="formData.startTime"
            type="datetime"
            value-format="YYYY-MM-DD HH:mm:ss"
            placeholder="请选择开始时间"
            class="w-full"
          />
        </el-form-item>
        <el-form-item label="结束时间">
          <el-date-picker
            v-model="formData.endTime"
            type="datetime"
            value-format="YYYY-MM-DD HH:mm:ss"
            placeholder="请选择结束时间"
            class="w-full"
          />
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="formData.remark" type="textarea" :rows="3" placeholder="请输入备注" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">
          取消
        </el-button>
        <el-button type="primary" :loading="dialogLoading" @click="handleSubmit">
          确定
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.section-title h3 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
}

.section-title p {
  margin: 4px 0 0;
  color: rgb(107 114 128);
  font-size: 13px;
}

.banner-cover {
  width: 84px;
  height: 48px;
  border-radius: 8px;
  overflow: hidden;
  background: rgb(243 244 246);
}

.banner-preview {
  width: 240px;
  height: 120px;
  border-radius: 10px;
  overflow: hidden;
  background: rgb(243 244 246);
}
</style>
