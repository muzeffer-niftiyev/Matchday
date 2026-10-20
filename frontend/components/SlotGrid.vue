<script setup lang="ts">
import type { Slot } from '~/types'

defineProps<{
  slots: Slot[]
  selected?: string | null
  disabled?: boolean
}>()

const emit = defineEmits<{ select: [startTime: string] }>()
</script>

<template>
  <div class="grid grid-cols-2 gap-2.5 sm:grid-cols-3">
    <button
      v-for="slot in slots"
      :key="slot.startTime"
      :disabled="!slot.isFree || disabled"
      class="group relative flex flex-col items-start rounded-2xl border px-4 py-3 text-left transition duration-150"
      :class="[
        !slot.isFree
          ? 'cursor-not-allowed border-ink-100 bg-ink-50 text-ink-300'
          : selected === slot.startTime
            ? 'border-brand-500 bg-brand-50 text-brand-800 ring-4 ring-brand-100'
            : 'border-ink-200 bg-white text-ink-800 hover:-translate-y-0.5 hover:border-brand-300 hover:shadow-soft',
        disabled && slot.isFree ? 'cursor-default opacity-60' : '',
      ]"
      @click="emit('select', slot.startTime)"
    >
      <span class="text-base font-semibold tabular-nums">{{ slot.startTime }}</span>
      <span class="text-xs" :class="slot.isFree ? 'text-ink-400' : 'text-ink-300'">{{ $t('slots.until', { time: slot.endTime }) }}</span>
      <span
        class="absolute right-3 top-3 h-2 w-2 rounded-full"
        :class="slot.isFree ? 'bg-brand-400' : 'bg-ink-200'"
      />
      <span v-if="!slot.isFree" class="mt-1 text-[11px] font-medium uppercase tracking-wide">{{ $t('slots.taken') }}</span>
    </button>
  </div>
</template>
