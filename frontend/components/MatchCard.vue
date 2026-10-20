<script setup lang="ts">
import type { Match } from '~/types'

const props = defineProps<{ match: Match }>()

const data = useDataStore()
const { niceDate } = useFormat()

const home = computed(() => data.team(props.match.homeTeamId))
const away = computed(() => data.team(props.match.awayTeamId))
const field = computed(() => data.field(props.match.fieldId))
const status = computed(() => data.statusOf(props.match))
const hasScore = computed(() => props.match.homeScore !== null && props.match.awayScore !== null)
</script>

<template>
  <NuxtLink :to="`/matches/${match.id}`" class="card card-hover block p-4 sm:p-5">
    <div class="mb-4 flex items-center justify-between gap-2 text-xs text-ink-500">
      <span class="inline-flex items-center gap-1.5 font-medium">
        <SvgoCalendar class="text-sm" />
        {{ niceDate(match.date) }} · {{ match.startTime }}
      </span>
      <StatusBadge :status="status" />
    </div>

    <div class="grid grid-cols-[1fr_auto_1fr] items-center gap-3">
      <div v-if="home" class="flex min-w-0 items-center gap-2.5">
        <TeamBadge :team="home" size="sm" />
        <span class="line-clamp-2 text-sm font-semibold leading-tight text-ink-800">{{ home.name }}</span>
      </div>

      <div class="rounded-xl px-3 py-1.5 text-center" :class="hasScore ? 'bg-ink-900 text-white' : 'bg-ink-100 text-ink-500'">
        <span v-if="hasScore" class="text-base font-bold tabular-nums">{{ match.homeScore }} : {{ match.awayScore }}</span>
        <span v-else class="text-xs font-semibold uppercase tracking-wide">{{ $t('match.vs') }}</span>
      </div>

      <div v-if="away" class="flex min-w-0 items-center justify-end gap-2.5">
        <span class="line-clamp-2 text-right text-sm font-semibold leading-tight text-ink-800">{{ away.name }}</span>
        <TeamBadge :team="away" size="sm" />
      </div>
    </div>

    <div v-if="field" class="mt-4 flex items-center gap-1.5 text-xs text-ink-400">
      <SvgoPin class="text-sm" /> {{ field.name }}, {{ field.city }}
    </div>
  </NuxtLink>
</template>
