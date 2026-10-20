<script setup lang="ts">
import { refDebounced } from '@vueuse/core'
import { CITIES } from '~/utils/rules'

const data = useDataStore()
const auth = useAuthStore()
const { t } = useI18n()

useHead({ title: () => t('nav.teams') })

const search = ref('')
const debounced = refDebounced(search, 200)
const city = ref('')
const tab = ref('all')

const tabs = computed(() => [
  { value: 'all', label: t('teams.allTeams') },
  ...(auth.isLoggedIn ? [{ value: 'mine', label: t('teams.myTeams'), count: data.teamsOfUser(auth.userId!).length }] : []),
])

const teams = computed(() =>
  data.teams
    .filter(team => tab.value !== 'mine' || data.isMember(team.id, auth.userId!))
    .filter(team => !city.value || team.city === city.value)
    .filter(team => team.name.toLowerCase().includes(debounced.value.toLowerCase()))
    .sort((a, b) => a.name.localeCompare(b.name)),
)
</script>

<template>
  <div class="container-page">
    <PageHeader :title="$t('nav.teams')" :subtitle="$t('teams.subtitle')">
      <template #actions>
        <UiButton :to="auth.isLoggedIn ? '/teams/new' : '/login?redirect=/teams/new'"><SvgoPlus class="text-lg" /> {{ $t('teams.create') }}</UiButton>
      </template>
    </PageHeader>

    <div class="mb-6 flex flex-col gap-3 sm:flex-row sm:items-center">
      <UiTabs v-model="tab" :tabs="tabs" />
      <div class="flex flex-1 gap-2 sm:justify-end">
        <div class="relative flex-1 sm:max-w-xs">
          <SvgoSearch class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-base text-ink-400" />
          <input v-model="search" class="input pl-9" :placeholder="$t('teams.search')">
        </div>
        <select v-model="city" class="input w-auto">
          <option value="">{{ $t('common.allCities') }}</option>
          <option v-for="c in CITIES" :key="c" :value="c">{{ c }}</option>
        </select>
      </div>
    </div>

    <TransitionGroup v-if="teams.length" tag="div" name="fade" class="grid grid-cols-1 gap-4 md:grid-cols-2">
      <TeamCard v-for="team in teams" :key="team.id" :team="team" />
    </TransitionGroup>
    <UiEmptyState v-else :title="$t('teams.noneTitle')" :text="$t('teams.noneText')">
      <template #icon><SvgoUsers class="text-2xl" /></template>
      <UiButton v-if="auth.isLoggedIn" to="/teams/new" size="sm">{{ $t('teams.create') }}</UiButton>
    </UiEmptyState>
  </div>
</template>
