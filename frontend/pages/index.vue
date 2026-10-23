<script setup lang="ts">
import type { Field, MatchSummary } from '~/types'

const api = useApi()
const auth = useAuthStore()
const { myTeam, attentionCount } = useInvitations()
const { t } = useI18n()

useHead({ title: () => t('nav.home') })

// Upcoming: logged in → my team's games only (none without a team), logged out → everyone's.
// GET /matches?when=upcoming is already sorted nearest first.
const { data: upcomingData } = await useAsyncData('home-upcoming', () => {
  if (auth.isLoggedIn && !myTeam.value) return Promise.resolve([])
  return api<MatchSummary[]>('/matches', { query: { when: 'upcoming', teamId: auth.isLoggedIn ? myTeam.value?.id : undefined } })
})
const upcoming = computed(() => (upcomingData.value ?? []).filter(m => m.status === 'Confirmed').slice(0, 4))

// GET /matches?when=results → newest first
const { data: resultsData } = await useAsyncData('home-results', () =>
  api<MatchSummary[]>('/matches', { query: { when: 'results' } }),
)
const results = computed(() => (resultsData.value ?? []).slice(0, 4))

const { data: fields } = await useAsyncData('home-fields', () => api<Field[]>('/fields'))

const stats = computed(() => [
  { label: 'home.statSquad', value: myTeam.value ? myTeam.value.memberCount : '—', to: myTeam.value ? `/teams/${myTeam.value.id}` : '/teams' },
  { label: 'home.statUpcoming', value: upcoming.value.length, to: '/matches' },
  { label: 'home.statInvites', value: attentionCount.value, to: '/invitations' },
])

const steps = [
  { icon: 'users', title: 'home.step1Title', text: 'home.step1Text' },
  { icon: 'calendar', title: 'home.step2Title', text: 'home.step2Text' },
  { icon: 'ball', title: 'home.step3Title', text: 'home.step3Text' },
]
</script>

<template>
  <div class="container-page space-y-12">
    <!-- Hero -->
    <section class="relative overflow-hidden rounded-[2rem] bg-gradient-to-br from-brand-500 via-emerald-600 to-teal-700 px-6 py-10 text-white shadow-lift sm:px-10 sm:py-14">
      <svg class="absolute -right-16 -top-10 h-80 w-80 text-white/10" viewBox="0 0 200 200" fill="none" stroke="currentColor" stroke-width="3">
        <circle cx="100" cy="100" r="90" /><circle cx="100" cy="100" r="30" /><line x1="10" y1="100" x2="190" y2="100" />
      </svg>
      <div class="relative max-w-xl">
        <p v-if="auth.user" class="mb-2 text-sm font-medium text-white/80">{{ $t('home.hello', { name: auth.user.displayName.split(' ')[0] }) }} 👋</p>
        <h1 class="text-3xl font-bold leading-tight sm:text-4xl">{{ $t('home.heroTitle') }}</h1>
        <p class="mt-3 text-white/85 sm:text-lg">{{ $t('home.heroText') }}</p>
        <div class="mt-7 flex flex-wrap gap-3">
          <UiButton to="/matches/new" variant="secondary" size="lg" class="!border-transparent">
            <SvgoCalendar class="text-lg" /> {{ $t('home.ctaBook') }}
          </UiButton>
          <UiButton v-if="myTeam" :to="`/teams/${myTeam.id}`" size="lg" class="!bg-white/15 !shadow-none backdrop-blur hover:!bg-white/25">
            <SvgoUsers class="text-lg" /> {{ myTeam.name }}
          </UiButton>
          <UiButton v-else :to="auth.isLoggedIn ? '/teams/new' : '/register'" size="lg" class="!bg-white/15 !shadow-none backdrop-blur hover:!bg-white/25">
            <SvgoPlus class="text-lg" /> {{ $t('home.ctaTeam') }}
          </UiButton>
        </div>
      </div>
    </section>

    <!-- Logged in: quick stats -->
    <section v-if="auth.isLoggedIn" class="grid grid-cols-3 gap-3 sm:gap-4">
      <NuxtLink v-for="s in stats" :key="s.label" :to="s.to" class="card card-hover p-4 sm:p-6">
        <p class="text-2xl font-bold text-ink-900 sm:text-3xl">{{ s.value }}</p>
        <p class="mt-1 text-xs text-ink-500 sm:text-sm">{{ $t(s.label) }}</p>
      </NuxtLink>
    </section>

    <!-- Invitation banner -->
    <NuxtLink
      v-if="attentionCount"
      to="/invitations"
      class="card card-hover flex items-center gap-4 border-amber-200 bg-amber-50/70 p-5"
    >
      <span class="flex h-12 w-12 items-center justify-center rounded-2xl bg-amber-100 text-amber-600"><SvgoBell class="text-2xl" /></span>
      <div class="flex-1">
        <p class="font-semibold text-ink-900">{{ $t('home.inviteBannerTitle', { n: attentionCount }) }}</p>
        <p class="text-sm text-ink-500">{{ $t('home.inviteBannerText') }}</p>
      </div>
      <SvgoChevronRight class="text-xl text-ink-400" />
    </NuxtLink>

    <!-- Logged out: how it works -->
    <section v-if="!auth.isLoggedIn">
      <h2 class="section-title mb-5">{{ $t('home.howTitle') }}</h2>
      <div class="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <div v-for="(step, i) in steps" :key="step.title" class="card p-6">
          <div class="mb-4 flex items-center gap-3">
            <span class="flex h-11 w-11 items-center justify-center rounded-2xl bg-brand-50 text-brand-600">
              <SvgoUsers v-if="step.icon === 'users'" class="text-xl" />
              <SvgoCalendar v-else-if="step.icon === 'calendar'" class="text-xl" />
              <SvgoBall v-else class="text-xl" />
            </span>
            <span class="text-sm font-semibold text-ink-300">0{{ i + 1 }}</span>
          </div>
          <h3 class="font-semibold text-ink-900">{{ $t(step.title) }}</h3>
          <p class="mt-1.5 text-sm text-ink-500">{{ $t(step.text) }}</p>
        </div>
      </div>
    </section>

    <!-- Upcoming + results -->
    <!-- Both columns stretch to the same height, so two empty states line up -->
    <div class="grid grid-cols-1 gap-10 lg:grid-cols-2">
      <section class="flex flex-col">
        <div class="mb-4 flex items-center justify-between">
          <h2 class="section-title">{{ auth.isLoggedIn ? $t('home.myUpcoming') : $t('home.upcoming') }}</h2>
          <NuxtLink v-if="upcoming.length" to="/matches" class="text-sm font-medium text-brand-600 hover:text-brand-700">{{ $t('common.seeAll') }}</NuxtLink>
        </div>
        <div v-if="upcoming.length" class="space-y-3">
          <MatchCard v-for="m in upcoming" :key="m.id" :match="m" />
        </div>
        <UiEmptyState v-else class="flex-1" :title="$t('home.noUpcoming')" :text="$t('home.noUpcomingText')">
          <UiButton to="/matches/new" size="sm">{{ $t('home.ctaBook') }}</UiButton>
        </UiEmptyState>
      </section>

      <section class="flex flex-col">
        <div class="mb-4 flex items-center justify-between">
          <h2 class="section-title">{{ $t('home.latestResults') }}</h2>
          <NuxtLink v-if="results.length" to="/matches?tab=results" class="text-sm font-medium text-brand-600 hover:text-brand-700">{{ $t('common.seeAll') }}</NuxtLink>
        </div>
        <div v-if="results.length" class="space-y-3">
          <MatchCard v-for="m in results" :key="m.id" :match="m" />
        </div>
        <UiEmptyState v-else class="flex-1" :title="$t('home.noResults')" :text="$t('home.noResultsText')">
          <template #icon><SvgoFlag class="text-2xl" /></template>
        </UiEmptyState>
      </section>
    </div>

    <!-- Fields -->
    <section>
      <div class="mb-4 flex items-center justify-between">
        <h2 class="section-title">{{ $t('home.popularFields') }}</h2>
        <NuxtLink to="/matches/new" class="text-sm font-medium text-brand-600 hover:text-brand-700">{{ $t('home.bookNow') }}</NuxtLink>
      </div>
      <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <FieldCard v-for="f in (fields ?? []).slice(0, 3)" :key="f.id" :field="f" :to="`/matches/new?fieldId=${f.id}`" />
      </div>
    </section>
  </div>
</template>
