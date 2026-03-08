<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  title: string
  value: string | number
  description?: string
  icon: string
  tone?: 'slate' | 'teal' | 'blue' | 'amber' | 'emerald' | 'rose' | 'violet' | 'sky'
  variant?: 'default' | 'board'
}>()

const toneClassMap = {
  slate: 'is-slate',
  teal: 'is-teal',
  blue: 'is-blue',
  amber: 'is-amber',
  emerald: 'is-emerald',
  rose: 'is-rose',
  violet: 'is-violet',
  sky: 'is-sky',
} as const

const toneClass = computed(() => toneClassMap[props.tone ?? 'slate'])
const variantClass = computed(() => props.variant === 'board' ? 'is-board' : 'is-default')
</script>

<template>
  <FaCard class="admin-metric-card" :class="variantClass">
    <div class="admin-metric-card__body">
      <div class="admin-metric-card__content">
        <p class="admin-metric-card__title">
          {{ title }}
        </p>
        <p class="admin-metric-card__value">
          {{ value }}
        </p>
        <p class="admin-metric-card__description">
          {{ description || '聚焦当前页面最关键的经营指标' }}
        </p>
      </div>
      <div class="admin-metric-card__icon" :class="toneClass">
        <FaIcon :name="icon" class="size-6" />
      </div>
    </div>
  </FaCard>
</template>

<style scoped>
.admin-metric-card {
  border-radius: 24px !important;
  border: 1px solid rgb(226 232 240 / 0.9) !important;
  background:
    linear-gradient(180deg, rgb(255 255 255 / 0.98), rgb(248 250 252 / 0.96)) !important;
  box-shadow: 0 18px 50px rgb(15 23 42 / 0.06) !important;
  overflow: hidden;
}

.admin-metric-card__body {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
  min-height: 144px;
}

.admin-metric-card__content {
  display: flex;
  min-width: 0;
  flex: 1;
  flex-direction: column;
  justify-content: center;
  gap: 10px;
}

.admin-metric-card__title {
  margin: 0;
  color: rgb(71 85 105);
  font-size: 13px;
  font-weight: 600;
  letter-spacing: 0.02em;
}

.admin-metric-card__value {
  margin: 0;
  color: rgb(15 23 42);
  font-size: 32px;
  font-weight: 700;
  letter-spacing: -0.04em;
  line-height: 1;
}

.admin-metric-card__description {
  margin: 0;
  color: rgb(100 116 139);
  font-size: 12px;
  line-height: 1.7;
}

.admin-metric-card__icon {
  display: flex;
  height: 56px;
  width: 56px;
  flex-shrink: 0;
  align-items: center;
  justify-content: center;
  border-radius: 18px;
  box-shadow: inset 0 1px 0 rgb(255 255 255 / 0.5);
}

.admin-metric-card.is-board {
  overflow: hidden;
}

.admin-metric-card.is-board .admin-metric-card__body {
  position: relative;
  min-height: 188px;
  align-items: center;
  padding: 6px 0;
}

.admin-metric-card.is-board .admin-metric-card__content {
  gap: 12px;
  justify-content: center;
}

.admin-metric-card.is-board .admin-metric-card__title {
  display: inline-flex;
  width: fit-content;
  min-height: 28px;
  align-items: center;
  border-radius: 9999px;
  background: rgb(15 23 42 / 0.06);
  padding: 0 12px;
  color: rgb(51 65 85);
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.04em;
}

.admin-metric-card.is-board .admin-metric-card__value {
  font-size: 38px;
  line-height: 1;
}

.admin-metric-card.is-board .admin-metric-card__description {
  max-width: 240px;
  font-size: 13px;
  line-height: 1.75;
}

.admin-metric-card.is-board .admin-metric-card__icon {
  position: relative;
  height: 78px;
  width: 78px;
  border-radius: 24px;
  box-shadow:
    inset 0 1px 0 rgb(255 255 255 / 0.6),
    0 18px 34px rgb(15 23 42 / 0.10);
}

.admin-metric-card.is-board .admin-metric-card__icon::after {
  content: '';
  position: absolute;
  inset: auto auto -18px -18px;
  width: 54px;
  height: 54px;
  border-radius: 9999px;
  background: rgb(255 255 255 / 0.45);
  filter: blur(6px);
}

.is-slate {
  background: linear-gradient(135deg, rgb(226 232 240), rgb(203 213 225));
  color: rgb(15 23 42);
}

.is-teal {
  background: linear-gradient(135deg, rgb(204 251 241), rgb(153 246 228));
  color: rgb(15 118 110);
}

.is-blue {
  background: linear-gradient(135deg, rgb(219 234 254), rgb(191 219 254));
  color: rgb(29 78 216);
}

.is-amber {
  background: linear-gradient(135deg, rgb(254 243 199), rgb(253 230 138));
  color: rgb(180 83 9);
}

.is-emerald {
  background: linear-gradient(135deg, rgb(209 250 229), rgb(167 243 208));
  color: rgb(5 150 105);
}

.is-rose {
  background: linear-gradient(135deg, rgb(255 228 230), rgb(254 205 211));
  color: rgb(225 29 72);
}

.is-violet {
  background: linear-gradient(135deg, rgb(237 233 254), rgb(221 214 254));
  color: rgb(109 40 217);
}

.is-sky {
  background: linear-gradient(135deg, rgb(224 242 254), rgb(186 230 253));
  color: rgb(3 105 161);
}
</style>
