<script setup lang="ts">
import { useIntervalFn } from '@vueuse/core'

const props = defineProps<{ until: string }>()

const { timeLeft } = useFormat()
const text = ref(timeLeft(props.until))

useIntervalFn(() => (text.value = timeLeft(props.until)), 1000)
</script>

<template>
  <span class="inline-flex items-center gap-1 tabular-nums">
    <SvgoClock class="text-sm" />
    <template v-if="text">{{ $t('match.timeLeft', { time: text }) }}</template>
    <template v-else>{{ $t('status.Expired') }}</template>
  </span>
</template>
