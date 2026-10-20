<script setup lang="ts">
import { PLAYERS_PER_MATCH } from '~/utils/rules'

const props = defineProps<{
  teamId: number
  blocked?: number[] // players already in the other lineup
}>()

const selected = defineModel<number[]>({ required: true })

const data = useDataStore()
const search = ref('')

const players = computed(() =>
  data.teamMembers(props.teamId)
    .map(m => ({ ...m, user: data.user(m.userId)! }))
    .filter(m => m.user.displayName.toLowerCase().includes(search.value.toLowerCase())),
)

const full = computed(() => selected.value.length >= PLAYERS_PER_MATCH)

function toggle(userId: number) {
  if (selected.value.includes(userId)) {
    selected.value = selected.value.filter(id => id !== userId)
  }
  else if (!full.value) {
    selected.value = [...selected.value, userId]
  }
}
</script>

<template>
  <div>
    <div class="mb-3 flex items-center justify-between gap-3">
      <div class="relative flex-1">
        <SvgoSearch class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-base text-ink-400" />
        <input v-model="search" class="input pl-9" :placeholder="$t('lineup.search')">
      </div>
      <div
        class="shrink-0 rounded-xl px-3 py-2 text-sm font-semibold tabular-nums transition"
        :class="selected.length === PLAYERS_PER_MATCH ? 'bg-brand-500 text-white' : 'bg-ink-100 text-ink-600'"
      >
        {{ selected.length }} / {{ PLAYERS_PER_MATCH }}
      </div>
    </div>

    <div class="mb-3 h-1.5 overflow-hidden rounded-full bg-ink-100">
      <div class="h-full rounded-full bg-brand-500 transition-all duration-300" :style="{ width: `${(selected.length / PLAYERS_PER_MATCH) * 100}%` }" />
    </div>

    <ul class="grid grid-cols-1 gap-2 sm:grid-cols-2">
      <li v-for="p in players" :key="p.userId">
        <button
          type="button"
          class="flex w-full items-center gap-3 rounded-2xl border px-3 py-2.5 text-left transition"
          :disabled="blocked?.includes(p.userId) || (full && !selected.includes(p.userId))"
          :class="[
            selected.includes(p.userId)
              ? 'border-brand-400 bg-brand-50'
              : 'border-ink-100 bg-white hover:border-ink-300',
            blocked?.includes(p.userId) ? 'cursor-not-allowed opacity-40' : '',
            full && !selected.includes(p.userId) && !blocked?.includes(p.userId) ? 'opacity-50' : '',
          ]"
          @click="toggle(p.userId)"
        >
          <UiAvatar :name="p.user.displayName" size="sm" />
          <div class="min-w-0 flex-1">
            <p class="truncate text-sm font-medium text-ink-800">{{ p.user.displayName }}</p>
            <p class="text-xs text-ink-400">
              <template v-if="blocked?.includes(p.userId)">{{ $t('lineup.inOtherTeam') }}</template>
              <template v-else>{{ $t(`roles.${p.role}`) }}</template>
            </p>
          </div>
          <span
            class="flex h-6 w-6 items-center justify-center rounded-full border-2 transition"
            :class="selected.includes(p.userId) ? 'border-brand-500 bg-brand-500 text-white' : 'border-ink-200'"
          >
            <SvgoCheck v-if="selected.includes(p.userId)" class="text-sm" />
          </span>
        </button>
      </li>
    </ul>
    <p v-if="players.length < PLAYERS_PER_MATCH && !search" class="mt-3 rounded-xl bg-amber-50 px-3 py-2 text-sm text-amber-700">
      {{ $t('lineup.notEnough', { n: PLAYERS_PER_MATCH }) }}
    </p>
  </div>
</template>
