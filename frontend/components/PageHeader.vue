<script setup lang="ts">
defineProps<{
  title: string
  subtitle?: string
  back?: string
}>()
</script>

<template>
  <div class="mb-8 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
    <div>
      <NuxtLink v-if="back" :to="back" class="mb-3 inline-flex items-center gap-1 text-sm font-medium text-ink-500 transition hover:text-ink-800">
        <SvgoChevronLeft class="text-base" /> {{ $t('common.back') }}
      </NuxtLink>
      <h1 class="text-2xl font-bold tracking-tight text-ink-900 sm:text-3xl">{{ title }}</h1>
      <p v-if="subtitle" class="mt-1.5 text-ink-500">{{ subtitle }}</p>
    </div>
    <div v-if="$slots.actions" class="flex flex-wrap gap-2">
      <slot name="actions" />
    </div>
  </div>
</template>
