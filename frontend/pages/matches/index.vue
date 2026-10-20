<script setup lang="ts">
import { matchStart } from '~/stores/data'

const data = useDataStore()
const auth = useAuthStore()
const route = useRoute()
const { t } = useI18n()

useHead({ title: () => t('nav.matches') })

const tab = ref(typeof route.query.tab === 'string' ? route.query.tab : 'upcoming')
const onlyMine = ref(false)

const myTeamIds = computed(() => (auth.userId ? data.teamsOfUser(auth.userId).map(t => t.id) : []))

const filtered = computed(() => {
  const now = new Date()
  return data.matches
    .filter(m => !onlyMine.value || myTeamIds.value.includes(m.homeTeamId) || myTeamIds.value.includes(m.awayTeamId))
    .filter((m) => {
      const s = data.statusOf(m)
      if (tab.value === 'upcoming') return (s === 'Confirmed' || s === 'Pending') && matchStart(m) > now
      if (tab.value === 'results') return s === 'Completed'
      return true
    })
    .sort((a, b) => (tab.value === 'upcoming' ? 1 : -1) * (matchStart(a).getTime() - matchStart(b).getTime()))
})

const tabs = computed(() => [
  { value: 'upcoming', label: t('matches.upcoming') },
  { value: 'results', label: t('matches.results') },
  { value: 'all', label: t('matches.all') },
])
</script>

<template>
  <div class="container-page">
    <PageHeader :title="$t('nav.matches')" :subtitle="$t('matches.subtitle')">
      <template #actions>
        <UiButton to="/fields"><SvgoPlus class="text-lg" /> {{ $t('matches.book') }}</UiButton>
      </template>
    </PageHeader>

    <div class="mb-6 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
      <UiTabs v-model="tab" :tabs="tabs" />
      <label v-if="auth.isLoggedIn" class="inline-flex cursor-pointer items-center gap-3 text-sm font-medium text-ink-600">
        <span>{{ $t('matches.onlyMine') }}</span>
        <button
          type="button"
          role="switch"
          :aria-checked="onlyMine"
          class="relative h-6 w-11 rounded-full transition"
          :class="onlyMine ? 'bg-brand-500' : 'bg-ink-200'"
          @click="onlyMine = !onlyMine"
        >
          <span class="absolute top-0.5 h-5 w-5 rounded-full bg-white shadow transition-all" :class="onlyMine ? 'left-[22px]' : 'left-0.5'" />
        </button>
      </label>
    </div>

    <TransitionGroup v-if="filtered.length" tag="div" name="fade" class="grid grid-cols-1 gap-3 md:grid-cols-2">
      <MatchCard v-for="m in filtered" :key="m.id" :match="m" />
    </TransitionGroup>
    <UiEmptyState v-else :title="$t('matches.noneTitle')" :text="$t('matches.noneText')">
      <UiButton to="/fields" size="sm">{{ $t('matches.book') }}</UiButton>
    </UiEmptyState>
  </div>
</template>
