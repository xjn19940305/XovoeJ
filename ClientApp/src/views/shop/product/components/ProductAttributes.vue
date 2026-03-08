<script setup lang="ts">
interface Props {
  isHot?: boolean
  isNew?: boolean
  isRecommend?: boolean
  isEnabled?: boolean
}

defineProps<Props>()
const emit = defineEmits<{
  (e: 'update:isHot', value: boolean): void
  (e: 'update:isNew', value: boolean): void
  (e: 'update:isRecommend', value: boolean): void
  (e: 'update:isEnabled', value: boolean): void
}>()

function updateValue(key: keyof Props, value: boolean) {
  if (key === 'isHot') {
    emit('update:isHot', value)
  }
  else if (key === 'isNew') {
    emit('update:isNew', value)
  }
  else if (key === 'isRecommend') {
    emit('update:isRecommend', value)
  }
  else if (key === 'isEnabled') {
    emit('update:isEnabled', value)
  }
}
</script>

<template>
  <div class="product-attributes">
    <el-checkbox :model-value="isHot" border @change="updateValue('isHot', $event as boolean)">
      <span class="toggle-item">
        <FaIcon name="i-heroicons-solid:fire" class="size-4 text-orange-500" />
        热门
      </span>
    </el-checkbox>
    <el-checkbox :model-value="isNew" border @change="updateValue('isNew', $event as boolean)">
      <span class="toggle-item">
        <FaIcon name="i-heroicons-solid:sparkles" class="size-4 text-green-500" />
        新品
      </span>
    </el-checkbox>
    <el-checkbox :model-value="isRecommend" border @change="updateValue('isRecommend', $event as boolean)">
      <span class="toggle-item">
        <FaIcon name="i-heroicons-solid:thumb-up" class="size-4 text-blue-500" />
        推荐
      </span>
    </el-checkbox>
    <el-checkbox v-if="isEnabled !== undefined" :model-value="isEnabled" border @change="updateValue('isEnabled', $event as boolean)">
      <span class="toggle-item">
        <FaIcon name="i-heroicons-solid:check-circle" class="size-4 text-emerald-500" />
        已上架
      </span>
    </el-checkbox>
  </div>
</template>

<style scoped>
.product-attributes {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}

.product-attributes :deep(.el-checkbox) {
  margin: 0;
  padding: 8px 16px;
  border-radius: 8px;
  border: 1px solid hsl(var(--border)) !important;
  background: hsl(var(--background));
  transition: all 0.2s;
}

.product-attributes :deep(.el-checkbox:hover) {
  border-color: hsl(var(--primary) / 0.3) !important;
}

.product-attributes :deep(.el-checkbox.is-checked) {
  border-color: hsl(var(--primary) / 0.3) !important;
  background: hsl(var(--primary) / 0.05);
}

.toggle-item {
  display: flex;
  align-items: center;
  gap: 6px;
}
</style>
