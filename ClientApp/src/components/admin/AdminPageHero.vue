<script setup lang="ts">
defineProps<{
  eyebrow?: string
  title: string
  description: string
}>()

defineSlots<{
  actions?: () => VNode
  default?: () => VNode
}>()
</script>

<template>
  <FaCard class="admin-page-hero">
    <div class="admin-page-hero__surface" />
    <div class="admin-page-hero__layout">
      <div class="admin-page-hero__main">
        <p v-if="eyebrow" class="admin-page-hero__eyebrow">
          {{ eyebrow }}
        </p>
        <h2 class="admin-page-hero__title">
          {{ title }}
        </h2>
        <p class="admin-page-hero__description">
          {{ description }}
        </p>
      </div>

      <div v-if="$slots.actions" class="admin-page-hero__actions">
        <slot name="actions" />
      </div>
    </div>

    <div v-if="$slots.default" class="admin-page-hero__extra">
      <slot />
    </div>
  </FaCard>
</template>

<style scoped>
.admin-page-hero {
  position: relative;
  overflow: hidden;
  border-radius: 28px !important;
  border: 1px solid rgb(226 232 240 / 0.9) !important;
  background:
    linear-gradient(135deg, rgb(255 255 255 / 0.98), rgb(248 250 252 / 0.94)) !important;
  box-shadow: 0 22px 60px rgb(15 23 42 / 0.07) !important;
}

.admin-page-hero__surface {
  position: absolute;
  inset: 0;
  background:
    radial-gradient(circle at top right, rgb(14 165 233 / 0.14), transparent 28%),
    radial-gradient(circle at left center, rgb(15 118 110 / 0.12), transparent 30%),
    linear-gradient(180deg, rgb(255 255 255 / 0.2), transparent);
  pointer-events: none;
}

.admin-page-hero__layout,
.admin-page-hero__extra {
  position: relative;
  z-index: 1;
}

.admin-page-hero__layout {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 20px;
}

.admin-page-hero__main {
  max-width: 760px;
}

.admin-page-hero__eyebrow {
  margin: 0 0 12px;
  color: rgb(15 118 110);
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.14em;
  text-transform: uppercase;
}

.admin-page-hero__title {
  margin: 0;
  color: rgb(15 23 42);
  font-size: 30px;
  font-weight: 700;
  letter-spacing: -0.04em;
}

.admin-page-hero__description {
  margin: 12px 0 0;
  color: rgb(71 85 105);
  font-size: 14px;
  line-height: 1.85;
}

.admin-page-hero__actions {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 12px;
}

.admin-page-hero__extra {
  margin-top: 22px;
}

@media (max-width: 960px) {
  .admin-page-hero__layout {
    flex-direction: column;
  }

  .admin-page-hero__actions {
    justify-content: flex-start;
  }
}
</style>
