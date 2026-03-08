<script setup lang="ts">
import type { HTMLAttributes } from 'vue'
import { cn } from '@/utils'

const props = defineProps<{
  class?: HTMLAttributes['class']
}>()
</script>

<template>
  <div :class="cn('p-6 pt-0 md:px-7 md:pb-7', props.class)">
    <slot />
  </div>
</template>
