<script setup lang="ts">
import type { TagProps } from 'element-plus'
import dayjs from 'dayjs'
import { ElMessage, ElMessageBox } from 'element-plus'
import orderApi from '@/api/modules/order'

defineOptions({
  name: 'ShopOrderList',
})

const searchForm = reactive({
  keyword: '',
  status: undefined as number | undefined,
})

const loading = ref(false)
const tableData = ref<Api.Order.Order[]>([])
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(20)

const detailDialogVisible = ref(false)
const detailLoading = ref(false)
const orderDetail = ref<Api.Order.OrderDetail | null>(null)

const shipDialogVisible = ref(false)
const shipSubmitting = ref(false)
const shippingOrder = ref<Api.Order.Order | null>(null)
const shipForm = reactive<Api.Order.ShipOrderRequest>({
  shippingCompany: '',
  trackingNo: '',
  shippingRemark: '',
})

const statusMap: Record<number, { label: string, type: TagProps['type'] }> = {
  0: { label: '待付款', type: 'warning' },
  1: { label: '待发货', type: 'primary' },
  2: { label: '待收货', type: 'info' },
  3: { label: '已完成', type: 'success' },
  4: { label: '已取消', type: 'danger' },
  5: { label: '退款中', type: 'warning' },
}

async function getOrderList() {
  loading.value = true
  try {
    const res = await orderApi.getList({
      page: currentPage.value,
      pageSize: pageSize.value,
      keyword: searchForm.keyword || undefined,
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
  getOrderList()
}

function handleReset() {
  searchForm.keyword = ''
  searchForm.status = undefined
  currentPage.value = 1
  getOrderList()
}

async function handleViewDetail(row: Api.Order.Order) {
  detailDialogVisible.value = true
  detailLoading.value = true
  try {
    const res = await orderApi.getDetail(row.id)
    orderDetail.value = res.data
  }
  finally {
    detailLoading.value = false
  }
}

async function handleDelete(id: string) {
  await ElMessageBox.confirm('确定要删除这笔订单吗？', '删除订单', {
    type: 'warning',
  })
  await orderApi.delete(id)
  ElMessage.success('删除成功')
  getOrderList()
}

function openShipDialog(row: Api.Order.Order) {
  shippingOrder.value = row
  shipForm.shippingCompany = row.shippingCompany || ''
  shipForm.trackingNo = row.trackingNo || ''
  shipForm.shippingRemark = row.shippingRemark || ''
  shipDialogVisible.value = true
}

async function submitShip() {
  if (!shippingOrder.value) {
    return
  }
  if (!shipForm.shippingCompany.trim()) {
    ElMessage.warning('请输入物流公司')
    return
  }
  if (!shipForm.trackingNo.trim()) {
    ElMessage.warning('请输入运单号')
    return
  }

  shipSubmitting.value = true
  try {
    await orderApi.ship(shippingOrder.value.id, {
      shippingCompany: shipForm.shippingCompany.trim(),
      trackingNo: shipForm.trackingNo.trim(),
      shippingRemark: shipForm.shippingRemark?.trim() || undefined,
    })
    ElMessage.success('发货成功')
    shipDialogVisible.value = false
    getOrderList()
    if (detailDialogVisible.value && orderDetail.value?.id === shippingOrder.value.id) {
      await handleViewDetail(shippingOrder.value)
    }
  }
  finally {
    shipSubmitting.value = false
  }
}

async function handleCancel(row: Api.Order.Order) {
  await ElMessageBox.confirm('确定要取消这笔订单吗？', '取消订单', {
    type: 'warning',
  })
  await orderApi.update(row.id, { status: 4 })
  ElMessage.success('订单已取消')
  getOrderList()
}

function handlePageChange(page: number) {
  currentPage.value = page
  getOrderList()
}

function handleSizeChange(size: number) {
  pageSize.value = size
  currentPage.value = 1
  getOrderList()
}

function formatDateTime(value?: string) {
  return value ? dayjs(value).format('YYYY-MM-DD HH:mm:ss') : '-'
}

onMounted(() => {
  getOrderList()
})
</script>

<template>
  <div class="shop-order-list">
    <FaCard class="search-card mb-4">
      <div class="search-header">
        <div class="search-title">
          <FaIcon name="i-heroicons-solid:magnifying-glass" class="size-5" />
          <span>订单筛选</span>
        </div>
      </div>
      <div class="search-body">
        <div class="search-grid">
          <div class="search-field">
            <label class="search-label">关键字</label>
            <el-input
              v-model="searchForm.keyword"
              placeholder="搜索订单号、用户名或手机号"
              clearable
              @keyup.enter="handleSearch"
            />
          </div>
          <div class="search-field">
            <label class="search-label">订单状态</label>
            <el-select v-model="searchForm.status" placeholder="全部状态" clearable class="w-full">
              <el-option label="待付款" :value="0" />
              <el-option label="待发货" :value="1" />
              <el-option label="待收货" :value="2" />
              <el-option label="已完成" :value="3" />
              <el-option label="已取消" :value="4" />
              <el-option label="退款中" :value="5" />
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

    <FaCard>
      <el-table v-loading="loading" :data="tableData">
        <el-table-column prop="orderNo" label="订单号" min-width="180" />
        <el-table-column prop="userName" label="用户" width="140" />
        <el-table-column prop="totalAmount" label="订单金额" width="120" align="right">
          <template #default="{ row }">
            ¥{{ row.totalAmount.toFixed(2) }}
          </template>
        </el-table-column>
        <el-table-column label="订单状态" width="110" align="center">
          <template #default="{ row }">
            <el-tag :type="statusMap[row.status]?.type">
              {{ statusMap[row.status]?.label || '未知状态' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="收货人" width="120">
          <template #default="{ row }">
            {{ row.receiverName || '-' }}
          </template>
        </el-table-column>
        <el-table-column label="物流信息" min-width="180">
          <template #default="{ row }">
            <div v-if="row.shippingCompany || row.trackingNo" class="text-sm">
              <div>{{ row.shippingCompany || '-' }}</div>
              <div class="text-gray-500">
                {{ row.trackingNo || '-' }}
              </div>
            </div>
            <span v-else class="text-gray-400">未发货</span>
          </template>
        </el-table-column>
        <el-table-column label="创建时间" min-width="180">
          <template #default="{ row }">
            {{ formatDateTime(row.createdAt) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="240" fixed="right">
          <template #default="{ row }">
            <div class="flex flex-wrap gap-2">
              <FaButton variant="ghost" size="sm" @click="handleViewDetail(row)">
                <template #icon>
                  <FaIcon name="i-iconoir:eye" />
                </template>
                详情
              </FaButton>
              <FaButton v-if="row.status === 1" variant="ghost" size="sm" @click="openShipDialog(row)">
                发货
              </FaButton>
              <FaButton v-if="row.status === 0" variant="ghost" size="sm" class="text-red-500" @click="handleCancel(row)">
                取消
              </FaButton>
              <FaButton variant="ghost" size="sm" class="text-red-500" @click="handleDelete(row.id)">
                <template #icon>
                  <FaIcon name="i-iconoir:trash" />
                </template>
                删除
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

    <el-dialog v-model="detailDialogVisible" title="订单详情" width="760px">
      <div v-loading="detailLoading">
        <div v-if="orderDetail" class="space-y-6">
          <section class="grid gap-3 border rounded-lg p-4 text-sm md:grid-cols-2">
            <div>
              <span class="text-gray-500">订单号：</span>{{ orderDetail.orderNo }}
            </div>
            <div>
              <span class="text-gray-500">订单状态：</span>
              <el-tag :type="statusMap[orderDetail.status]?.type" size="small">
                {{ statusMap[orderDetail.status]?.label || '未知状态' }}
              </el-tag>
            </div>
            <div>
              <span class="text-gray-500">订单金额：</span>
              <span class="text-red-500 font-medium">¥{{ orderDetail.totalAmount.toFixed(2) }}</span>
            </div>
            <div>
              <span class="text-gray-500">创建时间：</span>{{ formatDateTime(orderDetail.createdAt) }}
            </div>
            <div>
              <span class="text-gray-500">支付时间：</span>{{ formatDateTime(orderDetail.paidAt) }}
            </div>
            <div>
              <span class="text-gray-500">发货时间：</span>{{ formatDateTime(orderDetail.shippedAt) }}
            </div>
          </section>

          <section class="border rounded-lg p-4 text-sm">
            <div class="mb-3 font-medium">
              收货信息
            </div>
            <div class="text-gray-600 space-y-2">
              <div>收货人：{{ orderDetail.receiverName || '-' }}</div>
              <div>联系电话：{{ orderDetail.receiverPhone || '-' }}</div>
              <div>收货地址：{{ orderDetail.receiverAddress || '-' }}</div>
            </div>
          </section>

          <section class="border rounded-lg p-4 text-sm">
            <div class="mb-3 font-medium">
              物流信息
            </div>
            <div class="text-gray-600 space-y-2">
              <div>物流公司：{{ orderDetail.shippingCompany || '-' }}</div>
              <div>运单号：{{ orderDetail.trackingNo || '-' }}</div>
              <div>发货备注：{{ orderDetail.shippingRemark || '-' }}</div>
            </div>
          </section>

          <section class="border rounded-lg p-4 text-sm">
            <div class="mb-3 font-medium">
              发票信息
            </div>
            <div class="text-gray-600 space-y-2">
              <div>是否开票：{{ orderDetail.needInvoice ? '是' : '否' }}</div>
              <div>发票类型：{{ orderDetail.needInvoice ? (orderDetail.invoiceType === 2 ? '企业发票' : '个人发票') : '-' }}</div>
              <div>发票抬头：{{ orderDetail.invoiceTitle || '-' }}</div>
              <div>税号：{{ orderDetail.invoiceTaxNo || '-' }}</div>
              <div>发票邮箱：{{ orderDetail.invoiceEmail || '-' }}</div>
            </div>
          </section>

          <section class="border rounded-lg p-4">
            <div class="mb-3 font-medium">
              商品列表
            </div>
            <el-table :data="orderDetail.items" size="small">
              <el-table-column prop="productName" label="商品名称" min-width="180" />
              <el-table-column prop="price" label="单价" width="100" align="right">
                <template #default="{ row }">
                  ¥{{ row.price.toFixed(2) }}
                </template>
              </el-table-column>
              <el-table-column prop="quantity" label="数量" width="80" align="center" />
              <el-table-column label="小计" width="100" align="right">
                <template #default="{ row }">
                  ¥{{ (row.price * row.quantity).toFixed(2) }}
                </template>
              </el-table-column>
            </el-table>
          </section>

          <section v-if="orderDetail.remark" class="border rounded-lg p-4 text-sm">
            <div class="mb-2 font-medium">
              订单备注
            </div>
            <div class="text-gray-600">
              {{ orderDetail.remark }}
            </div>
          </section>
        </div>
      </div>
    </el-dialog>

    <el-dialog v-model="shipDialogVisible" title="订单发货" width="520px">
      <el-form label-width="88px">
        <el-form-item label="物流公司" required>
          <el-input v-model="shipForm.shippingCompany" placeholder="请输入物流公司" maxlength="64" />
        </el-form-item>
        <el-form-item label="运单号" required>
          <el-input v-model="shipForm.trackingNo" placeholder="请输入运单号" maxlength="64" />
        </el-form-item>
        <el-form-item label="发货备注">
          <el-input
            v-model="shipForm.shippingRemark"
            type="textarea"
            :rows="3"
            maxlength="200"
            show-word-limit
            placeholder="可选，填写发货说明或备注"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <div class="flex justify-end gap-3">
          <el-button @click="shipDialogVisible = false">
            取消
          </el-button>
          <el-button type="primary" :loading="shipSubmitting" @click="submitShip">
            确认发货
          </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>
