<script setup lang="ts">
import type { FormInstance, FormRules } from 'element-plus'
import dayjs from 'dayjs'
import { ElMessage, ElMessageBox } from 'element-plus'
import dictionaryApi from '@/api/modules/dictionary'

defineOptions({
  name: 'ContentDict',
})

interface GroupFormData {
  code?: string
  name: string
  description?: string
  type?: Api.Dictionary.DictionaryType
  parentId?: string
  icon?: string
  sortOrder: number
  isEnabled: boolean
  remark?: string
}

interface ItemFormData {
  groupId?: string
  key: string
  name: string
  value?: string
  dataType: Api.Dictionary.DictionaryDataType
  defaultValue?: string
  placeholder?: string
  description?: string
  options?: string
  validation?: string
  isRequired: boolean
  isEnabled: boolean
  sortOrder: number
  remark?: string
}

// 字典类型选项
const dictionaryTypeOptions = [
  { label: '系统配置', value: 0 },
  { label: '业务字典', value: 1 },
]

// 数据类型选项
const dataTypeOptions = [
  { label: '字符串', value: 0 },
  { label: '数字', value: 1 },
  { label: '布尔值', value: 2 },
  { label: 'JSON', value: 3 },
]

// 搜索表单
const searchForm = ref({
  type: undefined as number | undefined,
})

// 分组树数据
const groupTreeData = ref<Api.Dictionary.DictionaryGroupTree[]>([])
const groupLoading = ref(false)

// 当前选中的分组
const selectedGroupId = ref<string>()
const selectedGroup = ref<Api.Dictionary.DictionaryGroupTree>()

// 字典项列表
const itemList = ref<Api.Dictionary.DictionaryItem[]>([])
const itemLoading = ref(false)

// 分组对话框
const groupDialogVisible = ref(false)
const groupDialogTitle = ref('创建分组')
const groupDialogLoading = ref(false)
const groupFormRef = ref<FormInstance>()
const groupFormData = ref<GroupFormData>({
  code: '',
  name: '',
  sortOrder: 0,
  isEnabled: true,
  type: 0,
})
const editingGroupId = ref<string>()

// 字典项对话框
const itemDialogVisible = ref(false)
const itemDialogTitle = ref('创建字典项')
const itemDialogLoading = ref(false)
const itemFormRef = ref<FormInstance>()
const itemFormData = ref<ItemFormData>({
  groupId: '',
  key: '',
  name: '',
  dataType: 0,
  sortOrder: 0,
  isEnabled: true,
  isRequired: false,
})
const editingItemId = ref<string>()

// 分组表单验证规则
const groupRules: FormRules = {
  code: [
    { required: true, message: '请输入分组编码', trigger: 'blur' },
    { pattern: /^[\w-]+$/, message: '分组编码只能包含字母、数字、下划线和连字符', trigger: 'blur' },
  ],
  name: [
    { required: true, message: '请输入分组名称', trigger: 'blur' },
    { max: 64, message: '分组名称长度不能超过64个字符', trigger: 'blur' },
  ],
}

// 字典项表单验证规则
const itemRules: FormRules = {
  key: [
    { required: true, message: '请输入配置键', trigger: 'blur' },
    { pattern: /^[\w.-]+$/, message: '配置键只能包含字母、数字、下划线、点和连字符', trigger: 'blur' },
  ],
  name: [
    { required: true, message: '请输入配置名称', trigger: 'blur' },
    { max: 128, message: '配置名称长度不能超过128个字符', trigger: 'blur' },
  ],
}

// 获取分组树
async function getGroupTree() {
  groupLoading.value = true
  try {
    const res = await dictionaryApi.getGroupTree(searchForm.value.type)
    groupTreeData.value = res.data
    // 如果有选中的分组，清空选择
    if (selectedGroupId.value && !findGroupById(res.data, selectedGroupId.value)) {
      selectedGroupId.value = undefined
      selectedGroup.value = undefined
      itemList.value = []
    }
  }
  finally {
    groupLoading.value = false
  }
}

// 递归查找分组
function findGroupById(groups: Api.Dictionary.DictionaryGroupTree[], id: string): Api.Dictionary.DictionaryGroupTree | undefined {
  for (const group of groups) {
    if (group.id === id) {
      return group
    }
    if (group.children?.length) {
      const found = findGroupById(group.children, id)
      if (found) {
        return found
      }
    }
  }
  return undefined
}

// 获取字典项列表
async function getItemList() {
  if (!selectedGroupId.value) {
    itemList.value = []
    return
  }
  itemLoading.value = true
  try {
    const res = await dictionaryApi.getItems(selectedGroupId.value, false)
    itemList.value = res.data.sort((a, b) => a.sortOrder - b.sortOrder)
  }
  catch (error) {
    console.error('获取字典项列表失败:', error)
    itemList.value = []
  }
  finally {
    itemLoading.value = false
  }
}

// 搜索
function handleSearch() {
  getGroupTree()
}

// 重置搜索
function handleReset() {
  searchForm.value.type = undefined
  getGroupTree()
}

// 选择分组
function handleGroupSelect(data: any, node: any) {
  // eslint-disable-next-line no-console
  console.log('节点点击:', data, node)
  selectedGroupId.value = data.id
  selectedGroup.value = data
  getItemList()
}

// 格式化数据类型
function formatDataType(dataType: Api.Dictionary.DictionaryDataType): string {
  const map = {
    0: '字符串',
    1: '数字',
    2: '布尔值',
    3: 'JSON',
  }
  return map[dataType] || '未知'
}

// ========== 分组操作 ==========

// 打开创建分组对话框
function handleCreateGroup() {
  groupDialogTitle.value = '创建分组'
  editingGroupId.value = undefined
  groupFormData.value = {
    code: '',
    name: '',
    sortOrder: 0,
    isEnabled: true,
    type: searchForm.value.type ?? 0,
  }
  groupDialogVisible.value = true
}

// 提交分组表单
async function handleGroupSubmit() {
  await groupFormRef.value?.validate()
  groupDialogLoading.value = true
  try {
    if (editingGroupId.value) {
      await dictionaryApi.updateGroup(editingGroupId.value, groupFormData.value)
      ElMessage.success('修改成功')
    }
    else {
      await dictionaryApi.createGroup(groupFormData.value as Api.Dictionary.CreateDictionaryGroupRequest)
      ElMessage.success('创建成功')
    }
    groupDialogVisible.value = false
    getGroupTree()
  }
  finally {
    groupDialogLoading.value = false
  }
}

// ========== 字典项操作 ==========

// 打开创建字典项对话框
function handleCreateItem() {
  if (!selectedGroupId.value) {
    ElMessage.warning('请先选择一个分组')
    return
  }
  itemDialogTitle.value = '创建字典项'
  editingItemId.value = undefined
  itemFormData.value = {
    groupId: selectedGroupId.value,
    key: '',
    name: '',
    dataType: 0,
    sortOrder: 0,
    isEnabled: true,
    isRequired: false,
  }
  itemDialogVisible.value = true
}

// 打开编辑字典项对话框
function handleEditItem(item: Api.Dictionary.DictionaryItem) {
  itemDialogTitle.value = '编辑字典项'
  editingItemId.value = item.id
  itemFormData.value = {
    key: item.key,
    name: item.name,
    value: item.value,
    dataType: item.dataType,
    defaultValue: item.defaultValue,
    placeholder: item.placeholder,
    description: item.description,
    options: item.options,
    validation: item.validation,
    isRequired: item.isRequired,
    isEnabled: item.isEnabled,
    sortOrder: item.sortOrder,
    remark: item.remark,
  }
  itemDialogVisible.value = true
}

// 删除字典项
async function handleDeleteItem(item: Api.Dictionary.DictionaryItem) {
  await ElMessageBox.confirm('确定要删除该字典项吗？', '提示', { type: 'warning' })
  await dictionaryApi.deleteItem(item.id)
  ElMessage.success('删除成功')
  getItemList()
}

// 提交字典项表单
async function handleItemSubmit() {
  await itemFormRef.value?.validate()
  itemDialogLoading.value = true
  try {
    if (editingItemId.value) {
      await dictionaryApi.updateItem(editingItemId.value, itemFormData.value)
      ElMessage.success('修改成功')
    }
    else {
      await dictionaryApi.createItem(itemFormData.value as Api.Dictionary.CreateDictionaryItemRequest)
      ElMessage.success('创建成功')
    }
    itemDialogVisible.value = false
    getItemList()
  }
  finally {
    itemDialogLoading.value = false
  }
}

onMounted(() => {
  getGroupTree()
})
</script>

<template>
  <div class="content-dict p-4">
    <!-- 搜索表单 -->
    <FaCard class="search-card mb-4">
      <div class="search-header">
        <div class="search-title">
          <FaIcon name="i-heroicons-solid:magnifying-glass" class="size-5" />
          <span>字典筛选</span>
        </div>
      </div>
      <div class="search-body">
        <div class="search-grid">
          <div class="search-field">
            <label class="search-label">字典类型</label>
            <el-select v-model="searchForm.type" placeholder="请选择字典类型" clearable class="w-full">
              <el-option
                v-for="item in dictionaryTypeOptions"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              >
                <span class="flex items-center gap-2">
                  <FaIcon
                    :name="item.value === 0 ? 'i-heroicons-solid:cog' : 'i-heroicons-solid:book-open'"
                    class="size-4"
                    :class="item.value === 0 ? 'text-blue-500' : 'text-green-500'"
                  />
                  {{ item.label }}
                </span>
              </el-option>
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

    <div class="flex gap-4">
      <!-- 左侧分组树 -->
      <div class="w-80 flex-shrink-0">
        <FaCard>
          <template #header>
            <div class="flex items-center justify-between">
              <span class="font-medium">字典分组</span>
              <FaButton size="sm" @click="handleCreateGroup">
                <template #icon>
                  <FaIcon name="i-iconoir:plus" />
                </template>
                新建
              </FaButton>
            </div>
          </template>
          <el-tree
            v-loading="groupLoading"
            :data="groupTreeData"
            node-key="id"
            :current-node-key="selectedGroupId"
            :props="{ label: 'name', children: 'children' }"
            highlight-current
            @node-click="handleGroupSelect"
          />
        </FaCard>
      </div>

      <!-- 右侧字典项列表 -->
      <div class="min-w-0 flex-1">
        <FaCard>
          <template #header>
            <div class="flex items-center justify-between">
              <div>
                <span class="font-medium">{{ selectedGroup?.name || '字典项' }}</span>
                <span v-if="selectedGroup?.code" class="ml-2 text-sm text-gray-500">({{ selectedGroup.code }})</span>
              </div>
              <FaButton size="sm" :disabled="!selectedGroupId" @click="handleCreateItem">
                <template #icon>
                  <FaIcon name="i-iconoir:plus" />
                </template>
                新建
              </FaButton>
            </div>
          </template>

          <el-table
            v-loading="itemLoading"
            :data="itemList"
          >
            <el-table-column prop="key" label="配置键" min-width="180" show-overflow-tooltip />
            <el-table-column prop="name" label="配置名称" min-width="150" show-overflow-tooltip />
            <el-table-column label="数据类型" width="100">
              <template #default="{ row }">
                <el-tag size="small">
                  {{ formatDataType(row.dataType) }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="value" label="配置值" min-width="150" show-overflow-tooltip />
            <el-table-column prop="sortOrder" label="排序" width="80" />
            <el-table-column label="状态" width="80">
              <template #default="{ row }">
                <el-tag :type="row.isEnabled ? 'success' : 'info'" size="small">
                  {{ row.isEnabled ? '启用' : '禁用' }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column label="创建时间" width="170">
              <template #default="{ row }">
                {{ dayjs(row.createdAt).format('YYYY-MM-DD HH:mm:ss') }}
              </template>
            </el-table-column>
            <el-table-column label="操作" width="120" fixed="right">
              <template #default="{ row }">
                <FaButton variant="ghost" size="sm" @click="handleEditItem(row)">
                  <template #icon>
                    <FaIcon name="i-iconoir:edit-pencil" />
                  </template>
                  编辑
                </FaButton>
                <FaButton variant="ghost" size="sm" class="text-red-500" @click="handleDeleteItem(row)">
                  <template #icon>
                    <FaIcon name="i-iconoir:trash" />
                  </template>
                  删除
                </FaButton>
              </template>
            </el-table-column>
          </el-table>

          <div v-if="!selectedGroupId" class="flex flex-col items-center justify-center py-20 text-gray-400">
            <FaIcon name="i-heroicons-solid:arrow-left" class="mb-2 size-12" />
            <p>请从左侧选择一个分组查看字典项</p>
          </div>
        </FaCard>
      </div>
    </div>

    <!-- 分组对话框 -->
    <el-dialog
      v-model="groupDialogVisible"
      :title="groupDialogTitle"
      width="600px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="groupFormRef"
        v-loading="groupDialogLoading"
        :model="groupFormData"
        :rules="groupRules"
        label-width="100px"
      >
        <el-form-item v-if="!editingGroupId" label="分组编码" prop="code">
          <el-input v-model="groupFormData.code" placeholder="请输入分组编码，如：sys_config" />
        </el-form-item>
        <el-form-item label="分组名称" prop="name">
          <el-input v-model="groupFormData.name" placeholder="请输入分组名称" />
        </el-form-item>
        <el-form-item v-if="!editingGroupId" label="字典类型" prop="type">
          <el-radio-group v-model="groupFormData.type">
            <el-radio :value="0">
              系统配置
            </el-radio>
            <el-radio :value="1">
              业务字典
            </el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="图标">
          <FaIconPicker v-model="groupFormData.icon" />
        </el-form-item>
        <el-form-item label="排序">
          <el-input-number v-model="groupFormData.sortOrder" :min="0" :max="9999" />
        </el-form-item>
        <el-form-item label="状态">
          <el-switch v-model="groupFormData.isEnabled" active-text="启用" inactive-text="禁用" />
        </el-form-item>
        <el-form-item label="分组描述">
          <el-input v-model="groupFormData.description" type="textarea" :rows="3" placeholder="请输入分组描述" />
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="groupFormData.remark" type="textarea" :rows="2" placeholder="请输入备注" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="groupDialogVisible = false">
          取消
        </el-button>
        <el-button type="primary" :loading="groupDialogLoading" @click="handleGroupSubmit">
          确定
        </el-button>
      </template>
    </el-dialog>

    <!-- 字典项对话框 -->
    <el-dialog
      v-model="itemDialogVisible"
      :title="itemDialogTitle"
      width="700px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="itemFormRef"
        v-loading="itemDialogLoading"
        :model="itemFormData"
        :rules="itemRules"
        label-width="110px"
      >
        <el-form-item label="配置键" prop="key">
          <el-input v-model="itemFormData.key" placeholder="请输入配置键，如：site.title" :disabled="!!editingItemId" />
        </el-form-item>
        <el-form-item label="配置名称" prop="name">
          <el-input v-model="itemFormData.name" placeholder="请输入配置名称" />
        </el-form-item>
        <el-form-item label="数据类型" prop="dataType">
          <el-select v-model="itemFormData.dataType" placeholder="请选择数据类型" class="w-full">
            <el-option
              v-for="item in dataTypeOptions"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="配置值">
          <el-input v-model="itemFormData.value" type="textarea" :rows="3" placeholder="请输入配置值" />
        </el-form-item>
        <el-form-item label="默认值">
          <el-input v-model="itemFormData.defaultValue" placeholder="请输入默认值" />
        </el-form-item>
        <el-form-item label="占位提示">
          <el-input v-model="itemFormData.placeholder" placeholder="请输入占位提示" />
        </el-form-item>
        <el-form-item label="配置说明">
          <el-input v-model="itemFormData.description" type="textarea" :rows="2" placeholder="请输入配置说明" />
        </el-form-item>
        <el-form-item label="可选值">
          <el-input v-model="itemFormData.options" placeholder="请输入可选值，逗号分隔" />
        </el-form-item>
        <el-form-item label="排序">
          <el-input-number v-model="itemFormData.sortOrder" :min="0" :max="9999" />
        </el-form-item>
        <el-form-item label="是否必填">
          <el-switch v-model="itemFormData.isRequired" />
        </el-form-item>
        <el-form-item label="状态">
          <el-switch v-model="itemFormData.isEnabled" active-text="启用" inactive-text="禁用" />
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="itemFormData.remark" type="textarea" :rows="2" placeholder="请输入备注" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="itemDialogVisible = false">
          取消
        </el-button>
        <el-button type="primary" :loading="itemDialogLoading" @click="handleItemSubmit">
          确定
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>
