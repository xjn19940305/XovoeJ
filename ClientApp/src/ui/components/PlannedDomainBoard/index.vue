<script setup lang="ts">
interface PlannedSection {
  items: string[]
  title: string
}

interface Props {
  icon: string
  phase: string
  sections: PlannedSection[]
  summary: string
  title: string
}

defineProps<Props>()
</script>

<template>
  <div class="space-y-4">
    <FaCard>
      <div class="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <div class="flex items-start gap-4">
          <div class="size-14 flex items-center justify-center rounded-2xl bg-primary/8 text-primary">
            <FaIcon :name="icon" class="size-7" />
          </div>
          <div class="space-y-2">
            <h2 class="text-2xl text-stone-900 font-semibold">
              {{ title }}
            </h2>
            <p class="max-w-3xl text-sm text-stone-600 leading-6">
              {{ summary }}
            </p>
          </div>
        </div>
        <div class="inline-flex items-center rounded-full bg-stone-100 px-3 py-1 text-xs text-stone-600 font-medium">
          {{ phase }}
        </div>
      </div>
    </FaCard>

    <div class="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
      <FaCard
        v-for="section in sections"
        :key="section.title"
        class="h-full"
      >
        <div class="space-y-3">
          <h3 class="text-base text-stone-900 font-semibold">
            {{ section.title }}
          </h3>
          <ul class="space-y-2">
            <li
              v-for="item in section.items"
              :key="item"
              class="flex items-start gap-2 text-sm text-stone-600 leading-6"
            >
              <FaIcon name="i-heroicons-solid:check-circle" class="mt-1 size-4 shrink-0 text-emerald-500" />
              <span>{{ item }}</span>
            </li>
          </ul>
        </div>
      </FaCard>
    </div>
  </div>
</template>
