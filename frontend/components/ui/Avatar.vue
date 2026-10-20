<script setup lang="ts">
const props = withDefaults(defineProps<{
  name: string
  size?: 'xs' | 'sm' | 'md' | 'lg'
}>(), { size: 'md' })

const palette = ['bg-emerald-100 text-emerald-700', 'bg-sky-100 text-sky-700', 'bg-amber-100 text-amber-700', 'bg-rose-100 text-rose-700', 'bg-violet-100 text-violet-700', 'bg-teal-100 text-teal-700']

const initials = computed(() => props.name.split(' ').map(p => p[0]).slice(0, 2).join('').toUpperCase())
const color = computed(() => palette[[...props.name].reduce((sum, c) => sum + c.charCodeAt(0), 0) % palette.length])
</script>

<template>
  <span
    class="inline-flex shrink-0 items-center justify-center rounded-full font-semibold"
    :class="[color, { xs: 'h-6 w-6 text-[10px]', sm: 'h-8 w-8 text-xs', md: 'h-10 w-10 text-sm', lg: 'h-14 w-14 text-lg' }[size]]"
  >{{ initials }}</span>
</template>
