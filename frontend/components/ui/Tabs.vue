<script setup lang="ts">
const model = defineModel<string>({ required: true })

defineProps<{
  tabs: { value: string; label: string; count?: number }[]
}>()
</script>

<template>
  <div class="no-scrollbar inline-flex max-w-full gap-1 overflow-x-auto rounded-2xl bg-ink-100 p-1">
    <button
      v-for="tab in tabs"
      :key="tab.value"
      class="relative flex shrink-0 items-center gap-2 rounded-xl px-4 py-2 text-sm font-medium transition"
      :class="model === tab.value ? 'bg-white text-ink-900 shadow-sm' : 'text-ink-500 hover:text-ink-800'"
      @click="model = tab.value"
    >
      {{ tab.label }}
      <span
        v-if="tab.count"
        class="rounded-full px-1.5 text-xs"
        :class="model === tab.value ? 'bg-brand-100 text-brand-700' : 'bg-ink-200 text-ink-600'"
      >{{ tab.count }}</span>
    </button>
  </div>
</template>
