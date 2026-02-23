<script setup lang="ts">
interface Props {
  title: string
  icon?: string
  tips?: string
}

defineProps<Props>()
</script>

<template>
  <div class="form-section">
    <div class="form-section-header">
      <div v-if="icon" class="section-icon">
        <FaIcon :name="icon" class="size-4" />
      </div>
      <span class="section-title">{{ title }}</span>
    </div>
    <div class="form-section-body">
      <slot />
    </div>
    <div v-if="tips" class="form-section-tips">
      <div class="tip-item">
        <FaIcon name="i-heroicons-solid:light-bulb" class="size-4" />
        <span>{{ tips }}</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.form-section {
  border-radius: 12px;
  border: 1px solid hsl(var(--border));
  overflow: hidden;
  background: hsl(var(--card));
  margin-bottom: 24px;
}

.form-section-header {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 14px 20px;
  background: linear-gradient(90deg, hsl(var(--primary) / 0.08) 0%, hsl(var(--muted) / 0.15) 100%);
  border-bottom: 1px solid hsl(var(--border));
}

.section-icon {
  width: 28px;
  height: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: hsl(var(--primary) / 0.1);
  border-radius: 8px;
  color: hsl(var(--primary));
}

.section-title {
  font-size: 14px;
  font-weight: 600;
  color: hsl(var(--foreground) / 0.9);
}

.form-section-body {
  padding: 20px;
}

.form-section-tips {
  padding: 12px 20px;
  background: hsl(var(--muted) / 0.2);
  border-top: 1px solid hsl(var(--border) / 0.6);
}

.tip-item {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: hsl(var(--foreground) / 0.6);
}
</style>
