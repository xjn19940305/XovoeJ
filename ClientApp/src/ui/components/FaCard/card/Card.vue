<script setup lang="ts">
import type { HTMLAttributes } from 'vue'
import { cn } from '@/utils'

const props = defineProps<{
  class?: HTMLAttributes['class']
}>()
</script>

<template>
  <div
    :class="
      cn(
        'rounded-[24px] border border-slate-200/90 bg-white/95 text-card-foreground shadow-[0_18px_50px_rgba(15,23,42,0.06)] backdrop-blur',
        props.class,
      )
    "
  >
    <slot />
  </div>
</template>
