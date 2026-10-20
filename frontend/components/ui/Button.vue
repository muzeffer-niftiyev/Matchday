<script setup lang="ts">
import type { RouteLocationRaw } from 'vue-router'

const props = withDefaults(defineProps<{
  variant?: 'primary' | 'secondary' | 'ghost' | 'danger' | 'soft'
  size?: 'sm' | 'md' | 'lg'
  to?: RouteLocationRaw
  type?: 'button' | 'submit'
  disabled?: boolean
  loading?: boolean
  block?: boolean
}>(), {
  variant: 'primary',
  size: 'md',
  type: 'button',
})

const classes = computed(() => [
  'inline-flex items-center justify-center gap-2 rounded-xl font-semibold transition duration-150 ease-out select-none',
  'focus-visible:outline-none focus-visible:ring-4 active:scale-[0.98] disabled:pointer-events-none disabled:opacity-50',
  {
    sm: 'h-9 px-3.5 text-sm',
    md: 'h-11 px-5 text-sm',
    lg: 'h-12 px-6 text-base',
  }[props.size],
  {
    primary: 'bg-brand-500 text-white shadow-sm shadow-brand-500/30 hover:bg-brand-600 focus-visible:ring-brand-200',
    secondary: 'border border-ink-200 bg-white text-ink-700 hover:border-ink-300 hover:bg-ink-50 focus-visible:ring-ink-100',
    ghost: 'text-ink-600 hover:bg-ink-100 focus-visible:ring-ink-100',
    danger: 'bg-rose-500 text-white shadow-sm shadow-rose-500/30 hover:bg-rose-600 focus-visible:ring-rose-200',
    soft: 'bg-brand-50 text-brand-700 hover:bg-brand-100 focus-visible:ring-brand-100',
  }[props.variant],
  props.block ? 'w-full' : '',
])
</script>

<template>
  <NuxtLink v-if="to && !disabled" :to="to" :class="classes">
    <slot />
  </NuxtLink>
  <button v-else :type="type" :disabled="disabled || loading" :class="classes">
    <span v-if="loading" class="h-4 w-4 animate-spin rounded-full border-2 border-current border-r-transparent" />
    <slot />
  </button>
</template>
