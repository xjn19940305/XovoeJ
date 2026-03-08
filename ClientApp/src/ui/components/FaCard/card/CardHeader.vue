<script setup lang="ts">
import type { HTMLAttributes } from 'vue'
import { cn } from '@/utils'

const props = defineProps<{
  class?: HTMLAttributes['class']
}>()
</script>

<template>
  <div :class="cn('flex flex-col gap-y-2 p-6 md:p-7', props.class)">
    <slot />
  </div>
</template>
