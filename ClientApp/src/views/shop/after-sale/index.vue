<script setup lang="ts">
import type { TagProps } from 'element-plus'
import dayjs from 'dayjs'
import { ElMessage, ElMessageBox } from 'element-plus'
import afterSaleApi from '@/api/modules/after-sale'

defineOptions({
  name: 'ShopAfterSaleList',
})

const loading = ref(false)
const tableData = ref<Api.AfterSale.Item[]>([])
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(20)

const searchForm = reactive({
  keyword: '',
  type: undefined as number | undefined,
  status: undefined as number | undefined,
})

const detailDialogVisible = ref(false)
const detailLoading = ref(false)
const detailData = ref<Api.AfterSale.Detail | null>(null)

const rejectDialogVisible = ref(false)
const refundDialogVisible = ref(false)
const exchangeDialogVisible = ref(false)
const actionSubmitting = ref(false)
const currentRow = ref<Api.AfterSale.Item | null>(null)
const auditRemark = ref('')
const refundAmount = ref<number | undefined>(undefined)
const exchangeForm = reactive({
  shippingCompany: '',
  trackingNo: '',
  exchangeRemark: '',
})

const typeMap: Record<number, string> = {
  1: '仅退款',
  2: '退货退款',
  3: '换货',
}

function getStatusMeta(type: number, status: number): { label: string, type: TagProps['type'] } {
  if (status === 0) {
    return { label: '待审核', type: 'warning' }
  }
  if (status === 1) {
    return type === 3
      ? { label: '待换货', type: 'primary' }
      : { label: '待退款', type: 'primary' }
  }
  if (status === 2) {
    return { label: '已拒绝', type: 'danger' }
  }
  if (status === 3) {
    return type === 3
      ? { label: '已换货', type: 'success' }
      : { label: '已退款', type: 'success' }
  }
  return { label: '未知状态', type: 'info' }
}

async function getList() {
  loading.value = true
  try {
    const res = await afterSaleApi.getList({
      page: currentPage.value,
      pageSize: pageSize.value,
      keyword: searchForm.keyword || undefined,
      type: searchForm.type,
      status: searchForm.status,
    })
    tableData.value = res.data.items
    total.value = res.data.total
  }
  finally {
    loading.value = false
  }
}

function handleSearch() {
  currentPage.value = 1
  getList()
}

function handleReset() {
  searchForm.keyword = ''
  searchForm.type = undefined
  searchForm.status = undefined
  currentPage.value = 1
  getList()
}

async function handleViewDetail(row: Api.AfterSale.Item) {
  detailDialogVisible.value = true
  detailLoading.value = true
  try {
    const res = await afterSaleApi.getDetail(row.id)
    detailData.value = res.data
  }
  finally {
    detailLoading.value = false
  }
}

async function handleApprove(row: Api.AfterSale.Item) {
  await ElMessageBox.confirm(`确认通过售后单 ${row.afterSaleNo} 吗？`, '审核通过', { type: 'warning' })
  await afterSaleApi.approve(row.id)
  ElMessage.success('审核通过成功')
  getList()
}

function openRejectDialog(row: Api.AfterSale.Item) {
  currentRow.value = row
  auditRemark.value = ''
  rejectDialogVisible.value = true
}

async function submitReject() {
  if (!currentRow.value) {
    return
  }
  actionSubmitting.value = true
  try {
    await afterSaleApi.reject(currentRow.value.id, { adminRemark: auditRemark.value || undefined })
    ElMessage.success('驳回成功')
    rejectDialogVisible.value = false
    getList()
  }
  finally {
    actionSubmitting.value = false
  }
}

function openRefundDialog(row: Api.AfterSale.Item) {
  currentRow.value = row
  auditRemark.value = row.reason || ''
  refundAmount.value = row.refundAmount
  refundDialogVisible.value = true
}

function openExchangeDialog(row: Api.AfterSale.Item) {
  currentRow.value = row
  auditRemark.value = ''
  exchangeForm.shippingCompany = ''
  exchangeForm.trackingNo = ''
  exchangeForm.exchangeRemark = ''
  exchangeDialogVisible.value = true
}

async function submitRefund() {
  if (!currentRow.value) {
    return
  }
  if (!refundAmount.value || refundAmount.value <= 0) {
    ElMessage.warning('请输入正确的退款金额')
    return
  }
  actionSubmitting.value = true
  try {
    await afterSaleApi.refund(currentRow.value.id, {
      refundAmount: refundAmount.value,
      adminRemark: auditRemark.value || undefined,
    })
    ElMessage.success('退款成功')
    refundDialogVisible.value = false
    getList()
  }
  finally {
    actionSubmitting.value = false
  }
}

async function submitExchange() {
  if (!currentRow.value) {
    return
  }
  if (!exchangeForm.shippingCompany.trim()) {
    ElMessage.warning('请输入换货物流公司')
    return
  }
  if (!exchangeForm.trackingNo.trim()) {
    ElMessage.warning('请输入换货运单号')
    return
  }
  actionSubmitting.value = true
  try {
    await afterSaleApi.exchange(currentRow.value.id, {
      shippingCompany: exchangeForm.shippingCompany.trim(),
      trackingNo: exchangeForm.trackingNo.trim(),
      exchangeRemark: exchangeForm.exchangeRemark.trim() || undefined,
      adminRemark: auditRemark.value || undefined,
    })
    ElMessage.success('换货处理完成')
    exchangeDialogVisible.value = false
    getList()
  }
  finally {
    actionSubmitting.value = false
  }
}

function formatTime(value?: string) {
  return value ? dayjs(value).format('YYYY-MM-DD HH:mm:ss') : '-'
}

function handlePageChange(page: number) {
  currentPage.value = page
  getList()
}

function handleSizeChange(size: number) {
  pageSize.value = size
  currentPage.value = 1
  getList()
}

onMounted(() => {
  getList()
})
</script>

<template>
  <div class="shop-after-sale-list">
    <FaCard class="mb-4">
      <div class="mb-4 flex items-center gap-2 text-base font-medium">
        <FaIcon name="i-heroicons-solid:funnel" class="size-5" />
        <span>售后筛选</span>
      </div>
      <div class="grid gap-4 lg:grid-cols-3">
        <div>
          <div class="mb-2 text-sm text-gray-500">
            关键词
          </div>
          <el-input
            v-model="searchForm.keyword"
            placeholder="搜索售后单号、订单号、用户名"
            clearable
            @keyup.enter="handleSearch"
          />
        </div>
        <div>
          <div class="mb-2 text-sm text-gray-500">
            售后类型
          </div>
          <el-select v-model="searchForm.type" placeholder="全部类型" clearable class="w-full">
            <el-option label="仅退款" :value="1" />
            <el-option label="退货退款" :value="2" />
            <el-option label="换货" :value="3" />
          </el-select>
        </div>
        <div>
          <div class="mb-2 text-sm text-gray-500">
            售后状态
          </div>
          <el-select v-model="searchForm.status" placeholder="全部状态" clearable class="w-full">
            <el-option label="待审核" :value="0" />
            <el-option label="待处理" :value="1" />
            <el-option label="已拒绝" :value="2" />
            <el-option label="已完成" :value="3" />
          </el-select>
        </div>
      </div>
      <div class="mt-4 flex gap-3">
        <FaButton @click="handleSearch">
          <template #icon>
            <FaIcon name="i-heroicons-solid:magnifying-glass" />
          </template>
          搜索
        </FaButton>
        <FaButton @click="handleReset">
          <template #icon>
            <FaIcon name="i-heroicons-solid:arrow-path" />
          </template>
          重置
        </FaButton>
      </div>
    </FaCard>

    <FaCard>
      <el-table v-loading="loading" :data="tableData">
        <el-table-column prop="afterSaleNo" label="售后单号" min-width="200" />
        <el-table-column prop="orderNo" label="订单号" min-width="180" />
        <el-table-column prop="userName" label="用户" width="120" />
        <el-table-column label="售后类型" width="110" align="center">
          <template #default="{ row }">
            {{ typeMap[row.type] || '未知类型' }}
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="getStatusMeta(row.type, row.status).type">
              {{ getStatusMeta(row.type, row.status).label }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="reason" label="售后原因" min-width="180" show-overflow-tooltip />
        <el-table-column label="金额/补差" width="120" align="right">
          <template #default="{ row }">
            {{ row.type === 3 ? '-' : `¥${row.refundAmount.toFixed(2)}` }}
          </template>
        </el-table-column>
        <el-table-column label="申请时间" min-width="180">
          <template #default="{ row }">
            {{ formatTime(row.appliedAt) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="240" fixed="right">
          <template #default="{ row }">
            <div class="flex flex-wrap gap-2">
              <FaButton variant="ghost" size="sm" @click="handleViewDetail(row)">
                详情
              </FaButton>
              <FaButton v-if="row.status === 0" variant="ghost" size="sm" @click="handleApprove(row)">
                通过
              </FaButton>
              <FaButton v-if="row.status === 0 || row.status === 1" variant="ghost" size="sm" class="text-red-500" @click="openRejectDialog(row)">
                驳回
              </FaButton>
              <FaButton v-if="row.status === 1 && row.type !== 3" variant="ghost" size="sm" @click="openRefundDialog(row)">
                退款
              </FaButton>
              <FaButton v-if="row.status === 1 && row.type === 3" variant="ghost" size="sm" @click="openExchangeDialog(row)">
                完成换货
              </FaButton>
            </div>
          </template>
        </el-table-column>
      </el-table>

      <div class="mt-4 flex justify-end">
        <el-pagination
          :current-page="currentPage"
          :page-size="pageSize"
          :page-sizes="[20, 50, 100]"
          :total="total"
          layout="total, sizes, prev, pager, next, jumper"
          @current-change="handlePageChange"
          @size-change="handleSizeChange"
        />
      </div>
    </FaCard>

    <el-dialog v-model="detailDialogVisible" title="售后详情" width="820px">
      <div v-loading="detailLoading">
        <div v-if="detailData" class="space-y-6">
          <section class="grid gap-3 border rounded-lg p-4 text-sm md:grid-cols-2">
            <div><span class="text-gray-500">售后单号：</span>{{ detailData.afterSaleNo }}</div>
            <div><span class="text-gray-500">订单号：</span>{{ detailData.orderNo }}</div>
            <div><span class="text-gray-500">用户：</span>{{ detailData.userName || '-' }}</div>
            <div><span class="text-gray-500">售后类型：</span>{{ typeMap[detailData.type] || '未知类型' }}</div>
            <div>
              <span class="text-gray-500">售后状态：</span>
              <el-tag :type="getStatusMeta(detailData.type, detailData.status).type" size="small">
                {{ getStatusMeta(detailData.type, detailData.status).label }}
              </el-tag>
            </div>
            <div><span class="text-gray-500">退款金额：</span>{{ detailData.type === 3 ? '-' : `¥${detailData.refundAmount.toFixed(2)}` }}</div>
            <div><span class="text-gray-500">申请时间：</span>{{ formatTime(detailData.appliedAt) }}</div>
            <div><span class="text-gray-500">审核时间：</span>{{ formatTime(detailData.auditedAt) }}</div>
            <div>
              <span class="text-gray-500">{{ detailData.type === 3 ? '完成时间' : '退款时间' }}：</span>
              {{ formatTime(detailData.type === 3 ? detailData.exchangedAt : detailData.refundedAt) }}
            </div>
          </section>

          <section class="border rounded-lg p-4 text-sm">
            <div class="mb-3 font-medium">
              申请信息
            </div>
            <div class="text-gray-600 space-y-2">
              <div>售后原因：{{ detailData.reason }}</div>
              <div>问题描述：{{ detailData.description || '-' }}</div>
              <div>联系人：{{ detailData.contactName || '-' }}</div>
              <div>联系电话：{{ detailData.contactMobile || '-' }}</div>
              <div>处理备注：{{ detailData.adminRemark || '-' }}</div>
            </div>
          </section>

          <section v-if="detailData.type === 3" class="border rounded-lg p-4 text-sm">
            <div class="mb-3 font-medium">
              换货物流
            </div>
            <div class="text-gray-600 space-y-2">
              <div>物流公司：{{ detailData.exchangeShippingCompany || '-' }}</div>
              <div>运单号：{{ detailData.exchangeTrackingNo || '-' }}</div>
              <div>换货备注：{{ detailData.exchangeRemark || '-' }}</div>
            </div>
          </section>

          <section class="border rounded-lg p-4">
            <div class="mb-3 font-medium">
              关联商品
            </div>
            <el-table :data="detailData.items" size="small">
              <el-table-column prop="productName" label="商品名称" min-width="180" />
              <el-table-column prop="price" label="单价" width="100" align="right">
                <template #default="{ row }">
                  ¥{{ row.price.toFixed(2) }}
                </template>
              </el-table-column>
              <el-table-column prop="quantity" label="数量" width="80" align="center" />
            </el-table>
          </section>
        </div>
      </div>
    </el-dialog>

    <el-dialog v-model="rejectDialogVisible" title="驳回售后" width="520px">
      <el-form label-width="88px">
        <el-form-item label="处理备注">
          <el-input
            v-model="auditRemark"
            type="textarea"
            :rows="4"
            maxlength="200"
            show-word-limit
            placeholder="请输入驳回原因或处理说明"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <div class="flex justify-end gap-3">
          <el-button @click="rejectDialogVisible = false">
            取消
          </el-button>
          <el-button type="primary" :loading="actionSubmitting" @click="submitReject">
            确认驳回
          </el-button>
        </div>
      </template>
    </el-dialog>

    <el-dialog v-model="refundDialogVisible" title="执行退款" width="520px">
      <el-form label-width="88px">
        <el-form-item label="退款金额">
          <el-input-number v-model="refundAmount" :min="0.01" :precision="2" class="w-full" />
        </el-form-item>
        <el-form-item label="处理备注">
          <el-input
            v-model="auditRemark"
            type="textarea"
            :rows="4"
            maxlength="200"
            show-word-limit
            placeholder="请输入退款说明"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <div class="flex justify-end gap-3">
          <el-button @click="refundDialogVisible = false">
            取消
          </el-button>
          <el-button type="primary" :loading="actionSubmitting" @click="submitRefund">
            确认退款
          </el-button>
        </div>
      </template>
    </el-dialog>

    <el-dialog v-model="exchangeDialogVisible" title="完成换货" width="560px">
      <el-form label-width="96px">
        <el-form-item label="物流公司">
          <el-input
            v-model="exchangeForm.shippingCompany"
            maxlength="64"
            placeholder="请输入换货物流公司"
          />
        </el-form-item>
        <el-form-item label="运单号">
          <el-input
            v-model="exchangeForm.trackingNo"
            maxlength="64"
            placeholder="请输入换货运单号"
          />
        </el-form-item>
        <el-form-item label="换货备注">
          <el-input
            v-model="exchangeForm.exchangeRemark"
            type="textarea"
            :rows="3"
            maxlength="200"
            show-word-limit
            placeholder="可填写补发说明或备注"
          />
        </el-form-item>
        <el-form-item label="处理备注">
          <el-input
            v-model="auditRemark"
            type="textarea"
            :rows="3"
            maxlength="200"
            show-word-limit
            placeholder="请输入后台处理备注"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <div class="flex justify-end gap-3">
          <el-button @click="exchangeDialogVisible = false">
            取消
          </el-button>
          <el-button type="primary" :loading="actionSubmitting" @click="submitExchange">
            确认完成
          </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>
