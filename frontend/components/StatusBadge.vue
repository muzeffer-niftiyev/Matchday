<script setup lang="ts">
import type { MatchStatus } from '~/types'

defineProps<{ status: MatchStatus }>()

const styles: Record<MatchStatus, string> = {
  Pending: 'bg-amber-50 text-amber-700 ring-amber-200',
  Confirmed: 'bg-brand-50 text-brand-700 ring-brand-200',
  Completed: 'bg-ink-100 text-ink-700 ring-ink-200',
  Declined: 'bg-rose-50 text-rose-600 ring-rose-200',
  Cancelled: 'bg-rose-50 text-rose-600 ring-rose-200',
  Expired: 'bg-ink-50 text-ink-500 ring-ink-200',
}
</script>

<template>
  <span class="chip ring-1 ring-inset" :class="styles[status]">
    <span class="h-1.5 w-1.5 rounded-full bg-current opacity-70" />
    {{ $t(`status.${status}`) }}
  </span>
</template>
