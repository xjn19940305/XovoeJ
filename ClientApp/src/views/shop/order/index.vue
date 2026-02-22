<script setup lang="ts">
import { ElMessage, ElMessageBox } from 'element-plus'
import orderApi from '@/api/modules/order'
import dayjs from 'dayjs'

defineOptions({
  name: 'ShopOrderList',
})

// 搜索表单
const searchForm = ref({
  keyword: '',
  status: undefined as number | undefined,
})

// 表格数据
const tableData = ref<Api.Order.Order[]>([])
const loading = ref(false)
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(20)

// 订单详情对话框
const detailDialogVisible = ref(false)
const detailLoading = ref(false)
const orderDetail = ref<Api.Order.OrderDetail | null>(null)

// 订单状态映射
const statusMap: Record<number, { label: string; type: any }> = {
  0: { label: '待付款', type: 'warning' },
  1: { label: '待发货', type: 'primary' },
  2: { label: '待收货', type: 'info' },
  3: { label: '已完成', type: 'success' },
  4: { label: '已取消', type: 'danger' },
}

// 获取订单列表
async function getOrderList() {
  loading.value = true
  try {
    const params: Api.Order.OrderListParams = {
      page: currentPage.value,
      pageSize: pageSize.value,
      ...searchForm.value,
    }
    const res = await orderApi.getList(params)
    tableData.value = res.data.items
    total.value = res.data.total
  }
  finally {
    loading.value = false
  }
}

// 搜索
function handleSearch() {
  currentPage.value = 1
  getOrderList()
}

// 重置搜索
function handleReset() {
  searchForm.value = {
    keyword: '',
    status: undefined,
  }
  currentPage.value = 1
  getOrderList()
}

// 查看订单详情
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

// 删除订单
async function handleDelete(id: string) {
  await ElMessageBox.confirm('确定要删除该订单吗？', '提示', {
    type: 'warning',
  })
  await orderApi.delete(id)
  ElMessage.success('删除成功')
  getOrderList()
}

// 发货
async function handleShip(row: Api.Order.Order) {
  await ElMessageBox.confirm('确定要发货吗？', '提示', {
    type: 'warning',
  })
  await orderApi.ship(row.id)
  ElMessage.success('发货成功')
  getOrderList()
}

// 取消订单
async function handleCancel(row: Api.Order.Order) {
  await ElMessageBox.confirm('确定要取消该订单吗？', '提示', {
    type: 'warning',
  })
  await orderApi.update(row.id, { status: 4 })
  ElMessage.success('订单已取消')
  getOrderList()
}

// 分页改变
function handlePageChange(page: number) {
  currentPage.value = page
  getOrderList()
}

function handleSizeChange(size: number) {
  pageSize.value = size
  currentPage.value = 1
  getOrderList()
}

onMounted(() => {
  getOrderList()
})
</script>

<template>
  <div class="shop-order-list">
    <!-- 搜索表单 -->
    <FaCard class="mb-4">
      <div class="p-4">
        <el-form :model="searchForm" inline>
          <el-form-item label="关键字">
            <el-input v-model="searchForm.keyword" placeholder="订单号/用户名" clearable />
          </el-form-item>
          <el-form-item label="订单状态">
            <el-select v-model="searchForm.status" placeholder="请选择" clearable class="w-32">
              <el-option label="待付款" :value="0" />
              <el-option label="待发货" :value="1" />
              <el-option label="待收货" :value="2" />
              <el-option label="已完成" :value="3" />
              <el-option label="已取消" :value="4" />
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

    <!-- 表格 -->
    <FaCard>
      <el-table
        v-loading="loading"
        :data="tableData"
      >
        <el-table-column prop="orderNo" label="订单号" min-width="180" />
        <el-table-column prop="userName" label="用户" width="120" />
        <el-table-column prop="totalAmount" label="订单金额" width="120" align="right">
          <template #default="{ row }">
            ¥{{ row.totalAmount.toFixed(2) }}
          </template>
        </el-table-column>
        <el-table-column label="订单状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="statusMap[row.status]?.type">
              {{ statusMap[row.status]?.label }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="收货人" width="120">
          <template #default="{ row }">
            {{ row.receiverName || '-' }}
          </template>
        </el-table-column>
        <el-table-column label="创建时间" min-width="180">
          <template #default="{ row }">
            {{ row.createdAt ? dayjs(row.createdAt).format('YYYY-MM-DD HH:mm:ss') : '-' }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <FaButton variant="ghost" size="sm" @click="handleViewDetail(row)">
              <template #icon>
                <FaIcon name="i-iconoir:eye" />
              </template>
              详情
            </FaButton>
            <FaButton v-if="row.status === 1" variant="ghost" size="sm" @click="handleShip(row)">
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

    <!-- 订单详情对话框 -->
    <el-dialog
      v-model="detailDialogVisible"
      title="订单详情"
      width="700px"
    >
      <div v-loading="detailLoading">
        <div v-if="orderDetail" class="space-y-4">
          <!-- 订单基本信息 -->
          <div class="border-b pb-4">
            <h4 class="mb-2 font-medium">订单信息</h4>
            <div class="grid grid-cols-2 gap-2 text-sm">
              <div class="text-gray-600">订单号：</div>
              <div>{{ orderDetail.orderNo }}</div>
              <div class="text-gray-600">订单状态：</div>
              <div>
                <el-tag :type="statusMap[orderDetail.status]?.type" size="small">
                  {{ statusMap[orderDetail.status]?.label }}
                </el-tag>
              </div>
              <div class="text-gray-600">订单金额：</div>
              <div class="font-medium text-red-500">¥{{ orderDetail.totalAmount.toFixed(2) }}</div>
              <div class="text-gray-600">创建时间：</div>
              <div>{{ dayjs(orderDetail.createdAt).format('YYYY-MM-DD HH:mm:ss') }}</div>
            </div>
          </div>

          <!-- 收货信息 -->
          <div class="border-b pb-4">
            <h4 class="mb-2 font-medium">收货信息</h4>
            <div class="text-sm">
              <div class="text-gray-600">收货人：{{ orderDetail.receiverName || '-' }}</div>
              <div class="text-gray-600">联系电话：{{ orderDetail.receiverPhone || '-' }}</div>
              <div class="text-gray-600">收货地址：{{ orderDetail.receiverAddress || '-' }}</div>
            </div>
          </div>

          <!-- 商品列表 -->
          <div>
            <h4 class="mb-2 font-medium">商品列表</h4>
            <el-table :data="orderDetail.items" size="small">
              <el-table-column prop="productName" label="商品名称" min-width="150" />
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
          </div>

          <!-- 备注 -->
          <div v-if="orderDetail.remark">
            <h4 class="mb-2 font-medium">备注</h4>
            <div class="text-sm text-gray-600">{{ orderDetail.remark }}</div>
          </div>
        </div>
      </div>
    </el-dialog>
  </div>
</template>
