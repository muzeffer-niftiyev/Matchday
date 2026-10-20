<script setup lang="ts">
defineProps<{
  title: string
  text?: string
}>()
</script>

<template>
  <div class="flex flex-col items-center justify-center rounded-3xl border border-dashed border-ink-200 bg-white/60 px-6 py-12 text-center">
    <div class="mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-brand-50 text-brand-500">
      <slot name="icon">
        <SvgoBall class="text-2xl" />
      </slot>
    </div>
    <p class="font-semibold text-ink-800">{{ title }}</p>
    <p v-if="text" class="mt-1 max-w-sm text-sm text-ink-500">{{ text }}</p>
    <div v-if="$slots.default" class="mt-5">
      <slot />
    </div>
  </div>
</template>
