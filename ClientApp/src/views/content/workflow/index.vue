<script setup lang="ts">
import type { FormInstance, FormRules } from 'element-plus'
import { ElMessage, ElMessageBox } from 'element-plus'
import workflowApi from '@/api/modules/workflow'
import dayjs from 'dayjs'

defineOptions({
  name: 'ContentWorkflow',
})

// 搜索表单
const searchForm = ref({
  type: '',
})

// 表格数据
const tableData = ref<Api.Workflow.WorkflowDefinition[]>([])
const loading = ref(false)

// 对话框
const dialogVisible = ref(false)
const dialogTitle = ref('创建工作流')
const dialogLoading = ref(false)
const formRef = ref<FormInstance>()
const formData = ref<Api.Workflow.CreateWorkflowDefinitionRequest>({
  name: '',
  type: '',
  steps: [],
})

// 当前编辑的工作流编码
const editingCode = ref<string>()

// 步骤编辑器
const stepEditorVisible = ref(false)
const editingStepIndex = ref<number>()
const stepFormRef = ref<FormInstance>()
const stepFormData = ref<Api.Workflow.WorkflowStepDefinition>({
  id: '',
  name: '',
  type: 1,
  approverType: 1,
  approverIds: [],
  approvalRule: 1,
  order: 1,
  allowWithdraw: true,
})

// 表单验证规则
const rules: FormRules = {
  name: [
    { required: true, message: '请输入工作流名称', trigger: 'blur' },
    { max: 128, message: '工作流名称长度不能超过128个字符', trigger: 'blur' },
  ],
  type: [
    { required: true, message: '请输入工作流类型', trigger: 'blur' },
    { max: 64, message: '工作流类型长度不能超过64个字符', trigger: 'blur' },
  ],
}

// 步骤表单验证规则
const stepRules: FormRules = {
  name: [
    { required: true, message: '请输入步骤名称', trigger: 'blur' },
  ],
}

// 枚举选项
const stepTypeOptions = [
  { label: '审批节点', value: 1 },
  { label: '抄送节点', value: 2 },
  { label: '条件分支', value: 3 },
  { label: '并行网关', value: 4 },
  { label: '汇聚网关', value: 5 },
]

const approverTypeOptions = [
  { label: '指定用户', value: 1 },
  { label: '指定角色', value: 2 },
  { label: '部门主管', value: 3 },
  { label: '发起人的直属领导', value: 4 },
  { label: '动态选择', value: 5 },
  { label: '自选审批人', value: 6 },
]

const approvalRuleOptions = [
  { label: '需所有人同意', value: 1 },
  { label: '任一人同意即可', value: 2 },
  { label: '按顺序依次审批', value: 3 },
  { label: '按比例同意', value: 4 },
]

const timeoutActionOptions = [
  { label: '自动通过', value: 1 },
  { label: '自动拒绝', value: 2 },
  { label: '提醒审批人', value: 3 },
  { label: '转交上级', value: 4 },
]

// 获取工作流定义列表
async function getDefinitionList() {
  loading.value = true
  try {
    const res = await workflowApi.getDefinitions(searchForm.value.type || undefined)
    tableData.value = res.data.data || []
  }
  catch (error) {
    tableData.value = []
  }
  finally {
    loading.value = false
  }
}

// 搜索
function handleSearch() {
  getDefinitionList()
}

// 重置搜索
function handleReset() {
  searchForm.value.type = ''
  getDefinitionList()
}

// 打开创建对话框
function handleCreate() {
  dialogTitle.value = '创建工作流'
  editingCode.value = undefined
  formData.value = {
    name: '',
    type: '',
    steps: [],
  }
  dialogVisible.value = true
}

// 打开编辑对话框
async function handleEdit(row: Api.Workflow.WorkflowDefinition) {
  dialogTitle.value = '编辑工作流'
  dialogLoading.value = true
  dialogVisible.value = true
  editingCode.value = row.code
  try {
    const res = await workflowApi.getDefinition(row.code)
    const data = res.data.data
    formData.value = {
      name: data.name,
      description: data.description,
      type: data.type,
      steps: data.steps || [],
    }
  }
  finally {
    dialogLoading.value = false
  }
}

// 删除工作流
async function handleDelete(code: string) {
  await ElMessageBox.confirm('确定要删除该工作流吗？删除后不可恢复。', '提示', {
    type: 'warning',
  })
  await workflowApi.deleteDefinition(code)
  ElMessage.success('删除成功')
  getDefinitionList()
}

// 切换启用状态
async function handleToggleEnabled(row: Api.Workflow.WorkflowDefinition) {
  // 需要后端提供单独的启用/禁用接口，这里先通过更新实现
  const action = row.isEnabled ? '禁用' : '启用'
  try {
    await ElMessageBox.confirm(`确定要${action}该工作流吗？`, '提示', { type: 'warning' })
    // 这里需要调用更新接口，暂时只在前端更新显示
    row.isEnabled = !row.isEnabled
    ElMessage.success(`${action}成功`)
  }
  catch {
    // 用户取消
  }
}

// 提交表单
async function handleSubmit() {
  await formRef.value?.validate()
  if (formData.value.steps.length === 0) {
    ElMessage.warning('请至少添加一个步骤')
    return
  }
  dialogLoading.value = true
  try {
    if (editingCode.value) {
      await workflowApi.updateDefinition(editingCode.value, formData.value)
      ElMessage.success('修改成功')
    }
    else {
      await workflowApi.createDefinition(formData.value)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    getDefinitionList()
  }
  finally {
    dialogLoading.value = false
  }
}

// ========== 步骤管理 ==========

// 打开添加步骤对话框
function handleAddStep() {
  editingStepIndex.value = undefined
  stepFormData.value = {
    id: `step_${Date.now()}`,
    name: '',
    type: 1,
    approverType: 1,
    approverIds: [],
    approvalRule: 1,
    order: formData.value.steps.length + 1,
    allowWithdraw: true,
  }
  stepEditorVisible.value = true
}

// 打开编辑步骤对话框
function handleEditStep(index: number) {
  editingStepIndex.value = index
  stepFormData.value = { ...formData.value.steps[index] }
  stepEditorVisible.value = true
}

// 删除步骤
function handleDeleteStep(index: number) {
  formData.value.steps.splice(index, 1)
  // 更新步骤顺序
  formData.value.steps.forEach((step, i) => {
    step.order = i + 1
  })
}

// 上移步骤
function handleMoveUpStep(index: number) {
  if (index === 0)
    return
  const temp = formData.value.steps[index]
  formData.value.steps[index] = formData.value.steps[index - 1]
  formData.value.steps[index - 1] = temp
  // 更新顺序
  formData.value.steps[index].order = index + 1
  formData.value.steps[index - 1].order = index
}

// 下移步骤
function handleMoveDownStep(index: number) {
  if (index === formData.value.steps.length - 1)
    return
  const temp = formData.value.steps[index]
  formData.value.steps[index] = formData.value.steps[index + 1]
  formData.value.steps[index + 1] = temp
  // 更新顺序
  formData.value.steps[index].order = index + 1
  formData.value.steps[index + 1].order = index + 2
}

// 保存步骤
async function handleSaveStep() {
  await stepFormRef.value?.validate()
  if (editingStepIndex.value === undefined) {
    formData.value.steps.push({ ...stepFormData.value })
  }
  else {
    formData.value.steps[editingStepIndex.value] = { ...stepFormData.value }
  }
  stepEditorVisible.value = false
}

// 获取步骤类型标签
function getStepTypeLabel(type: Api.Workflow.WorkflowStepType): string {
  const map = {
    1: '审批',
    2: '抄送',
    3: '条件',
    4: '并行',
    5: '汇聚',
  }
  return map[type] || '未知'
}

// 获取审批人类型标签
function getApproverTypeLabel(type: Api.Workflow.ApproverType): string {
  const map = {
    1: '指定用户',
    2: '指定角色',
    3: '部门主管',
    4: '直属领导',
    5: '动态选择',
    6: '自选审批人',
  }
  return map[type] || '未知'
}

// 获取审批规则标签
function getApprovalRuleLabel(rule: Api.Workflow.ApprovalRule): string {
  const map = {
    1: '全部同意',
    2: '任一同意',
    3: '依次审批',
    4: '比例同意',
  }
  return map[rule] || '未知'
}

onMounted(() => {
  getDefinitionList()
})
</script>

<template>
  <div class="content-workflow p-4">
    <!-- 搜索表单 -->
    <FaCard class="search-card mb-4">
      <div class="search-header">
        <div class="search-title">
          <FaIcon name="i-heroicons-solid:magnifying-glass" class="size-5" />
          <span>工作流筛选</span>
        </div>
      </div>
      <div class="search-body">
        <div class="search-grid">
          <div class="search-field">
            <label class="search-label">工作流类型</label>
            <el-input
              v-model="searchForm.type"
              placeholder="搜索工作流类型"
              clearable
            >
              <template #prefix>
                <FaIcon name="i-heroicons-solid:arrow-path" class="size-4 text-gray-400" />
              </template>
            </el-input>
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
          创建工作流
        </FaButton>
      </div>
    </div>

    <!-- 表格 -->
    <FaCard>
      <el-table
        v-loading="loading"
        :data="tableData"
      >
        <el-table-column prop="code" label="编码" min-width="150" show-overflow-tooltip />
        <el-table-column prop="name" label="名称" min-width="150" show-overflow-tooltip />
        <el-table-column prop="type" label="类型" min-width="120" show-overflow-tooltip />
        <el-table-column prop="description" label="描述" min-width="200" show-overflow-tooltip />
        <el-table-column prop="version" label="版本" width="80" />
        <el-table-column prop="steps" label="步骤数" width="80">
          <template #default="{ row }">
            {{ row.steps?.length || 0 }}
          </template>
        </el-table-column>
        <el-table-column prop="usageCount" label="使用次数" width="100" />
        <el-table-column label="状态" width="80">
          <template #default="{ row }">
            <el-tag :type="row.isEnabled ? 'success' : 'info'" size="small">
              {{ row.isEnabled ? '启用' : '禁用' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="创建时间" min-width="170">
          <template #default="{ row }">
            {{ dayjs(row.createdAt).format('YYYY-MM-DD HH:mm:ss') }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="220" fixed="right">
          <template #default="{ row }">
            <FaButton variant="ghost" size="sm" @click="handleEdit(row)">
              <template #icon>
                <FaIcon name="i-iconoir:edit-pencil" />
              </template>
              编辑
            </FaButton>
            <FaButton variant="ghost" size="sm" @click="handleToggleEnabled(row)">
              <template #icon>
                <FaIcon :name="row.isEnabled ? 'i-iconoir:lock' : 'i-iconoir:unlock'" />
              </template>
              {{ row.isEnabled ? '禁用' : '启用' }}
            </FaButton>
            <FaButton variant="ghost" size="sm" class="text-red-500" @click="handleDelete(row.code)">
              <template #icon>
                <FaIcon name="i-iconoir:trash" />
              </template>
              删除
            </FaButton>
          </template>
        </el-table-column>
      </el-table>
    </FaCard>

    <!-- 创建/编辑对话框 -->
    <el-dialog
      v-model="dialogVisible"
      :title="dialogTitle"
      width="900px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="formRef"
        v-loading="dialogLoading"
        :model="formData"
        :rules="rules"
        label-width="100px"
      >
        <el-form-item label="工作流名称" prop="name">
          <el-input v-model="formData.name" placeholder="请输入工作流名称，如：采购申请审批" />
        </el-form-item>
        <el-form-item label="工作流类型" prop="type">
          <el-input v-model="formData.type" placeholder="请输入工作流类型，如：procurement" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="formData.description" type="textarea" :rows="2" placeholder="请输入工作流描述" />
        </el-form-item>

        <!-- 步骤配置 -->
        <el-form-item label="审批步骤">
          <div class="w-full">
            <div class="flex items-center justify-between mb-2">
              <span class="text-sm text-gray-500">配置工作流的审批步骤</span>
              <FaButton size="sm" @click="handleAddStep">
                <template #icon>
                  <FaIcon name="i-iconoir:plus" />
                </template>
                添加步骤
              </FaButton>
            </div>

            <div class="step-list">
              <div
                v-for="(step, index) in formData.steps"
                :key="step.id"
                class="step-item"
              >
                <div class="step-order">{{ index + 1 }}</div>
                <div class="step-content">
                  <div class="step-name">{{ step.name }}</div>
                  <div class="step-meta">
                    <el-tag size="small" type="info">{{ getStepTypeLabel(step.type) }}</el-tag>
                    <span class="text-xs text-gray-500 ml-2">
                      {{ getApproverTypeLabel(step.approverType) }}
                    </span>
                    <span v-if="step.approverIds.length" class="text-xs text-gray-500 ml-2">
                      ({{ step.approverIds.length }}人)
                    </span>
                    <span class="text-xs text-gray-500 ml-2">
                      {{ getApprovalRuleLabel(step.approvalRule) }}
                    </span>
                  </div>
                </div>
                <div class="step-actions">
                  <FaButton size="sm" variant="ghost" @click="handleEditStep(index)">
                    <FaIcon name="i-iconoir:edit-pencil" />
                  </FaButton>
                  <FaButton size="sm" variant="ghost" @click="handleMoveUpStep(index)" :disabled="index === 0">
                    <FaIcon name="i-iconoir:nav-arrow-up" />
                  </FaButton>
                  <FaButton size="sm" variant="ghost" @click="handleMoveDownStep(index)" :disabled="index === formData.steps.length - 1">
                    <FaIcon name="i-iconoir:nav-arrow-down" />
                  </FaButton>
                  <FaButton size="sm" variant="ghost" class="text-red-500" @click="handleDeleteStep(index)">
                    <FaIcon name="i-iconoir:trash" />
                  </FaButton>
                </div>
              </div>
              <div v-if="formData.steps.length === 0" class="step-empty">
                <p class="text-gray-400">暂无审批步骤，请点击"添加步骤"按钮添加</p>
              </div>
            </div>
          </div>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="dialogLoading" @click="handleSubmit">确定</el-button>
      </template>
    </el-dialog>

    <!-- 步骤编辑对话框 -->
    <el-dialog
      v-model="stepEditorVisible"
      :title="editingStepIndex === undefined ? '添加步骤' : '编辑步骤'"
      width="600px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="stepFormRef"
        :model="stepFormData"
        :rules="stepRules"
        label-width="110px"
      >
        <el-form-item label="步骤名称" prop="name">
          <el-input v-model="stepFormData.name" placeholder="请输入步骤名称，如：部门主管审批" />
        </el-form-item>
        <el-form-item label="步骤描述">
          <el-input v-model="stepFormData.description" placeholder="请输入步骤描述" />
        </el-form-item>
        <el-form-item label="步骤类型">
          <el-select v-model="stepFormData.type" placeholder="请选择步骤类型" class="w-full">
            <el-option
              v-for="item in stepTypeOptions"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="审批人类型">
          <el-select v-model="stepFormData.approverType" placeholder="请选择审批人类型" class="w-full">
            <el-option
              v-for="item in approverTypeOptions"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item v-if="[1, 2].includes(stepFormData.approverType)" label="审批人ID">
          <el-select
            v-model="stepFormData.approverIds"
            multiple
            filterable
            allow-create
            placeholder="请输入审批人ID（支持多选）"
            class="w-full"
          >
            <!-- 这里可以改成从后端获取用户/角色列表 -->
          </el-select>
          <span class="text-xs text-gray-500">多个审批人ID用逗号分隔或从下拉选择</span>
        </el-form-item>
        <el-form-item label="审批规则">
          <el-select v-model="stepFormData.approvalRule" placeholder="请选择审批规则" class="w-full">
            <el-option
              v-for="item in approvalRuleOptions"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="超时时间(小时)">
          <el-input-number v-model="stepFormData.timeoutHours" :min="1" :max="720" placeholder="不限制" />
        </el-form-item>
        <el-form-item v-if="stepFormData.timeoutHours" label="超时处理">
          <el-select v-model="stepFormData.timeoutAction" placeholder="请选择超时处理方式" class="w-full">
            <el-option
              v-for="item in timeoutActionOptions"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="允许撤回">
          <el-switch v-model="stepFormData.allowWithdraw" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="stepEditorVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSaveStep">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.step-list {
  border: 1px solid var(--el-border-color);
  border-radius: 4px;
  padding: 8px;
  background-color: var(--el-bg-color-page);
  min-height: 100px;
}

.step-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  background-color: var(--el-bg-color);
  border: 1px solid var(--el-border-color);
  border-radius: 6px;
  margin-bottom: 8px;
}

.step-item:last-child {
  margin-bottom: 0;
}

.step-order {
  flex-shrink: 0;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border-radius: 50%;
  font-weight: bold;
  font-size: 14px;
}

.step-content {
  flex: 1;
  min-width: 0;
}

.step-name {
  font-weight: 500;
  margin-bottom: 4px;
}

.step-meta {
  display: flex;
  align-items: center;
  gap: 4px;
  flex-wrap: wrap;
}

.step-actions {
  display: flex;
  gap: 4px;
}

.step-empty {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 80px;
  border: 1px dashed var(--el-border-color);
  border-radius: 4px;
}
</style>
