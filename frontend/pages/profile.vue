<script setup lang="ts">
import { matchStart } from '~/utils/rules'
import type { MatchSummary } from '~/types'

definePageMeta({ middleware: 'auth' })

const api = useApi()
const auth = useAuthStore()
const notify = useNotify()
const router = useRouter()
const { myRequests } = useInvitations()
const { t } = useI18n()

useHead({ title: () => t('nav.profile') })

const myTeam = computed(() => auth.team)

// GET /me/matches → the matches I'm in the lineup of, newest first
const { data: myMatchesData } = await useAsyncData('my-matches', () => api<MatchSummary[]>('/me/matches'))
const myMatches = computed(() => myMatchesData.value ?? [])

const played = computed(() => myMatches.value.filter(m => m.status === 'Completed').length)
const upcoming = computed(() => myMatches.value.filter(m => m.status === 'Confirmed' && matchStart(m) > new Date()))

function logout() {
  auth.logout()
  notify.info('auth.loggedOut')
  router.push('/')
}
</script>

<template>
  <div v-if="auth.user" class="container-page">
    <div class="grid grid-cols-1 gap-6 lg:grid-cols-[320px_1fr]">
      <!-- Left: user card -->
      <aside class="space-y-4">
        <div class="card p-6 text-center">
          <UiAvatar :name="auth.user.displayName" size="lg" class="mx-auto" />
          <h1 class="mt-4 text-xl font-bold text-ink-900">{{ auth.user.displayName }}</h1>
          <p class="text-sm text-ink-500">{{ auth.user.email }}</p>
          <span v-if="auth.isAdmin" class="chip mt-3 bg-ink-900 text-white"><SvgoShield class="text-xs" /> {{ $t('profile.admin') }}</span>

          <div class="mt-6 grid grid-cols-3 gap-2 rounded-2xl bg-ink-50 p-3">
            <div><p class="text-lg font-bold text-ink-900">{{ myTeam ? myTeam.memberCount : '—' }}</p><p class="text-[11px] text-ink-500">{{ $t('profile.squad') }}</p></div>
            <div><p class="text-lg font-bold text-ink-900">{{ played }}</p><p class="text-[11px] text-ink-500">{{ $t('profile.played') }}</p></div>
            <div><p class="text-lg font-bold text-ink-900">{{ upcoming.length }}</p><p class="text-[11px] text-ink-500">{{ $t('profile.upcoming') }}</p></div>
          </div>
        </div>

        <div class="card divide-y divide-ink-100">
          <NuxtLink v-if="auth.isAdmin" to="/admin" class="flex items-center gap-2 px-5 py-4 text-sm font-medium text-ink-700 transition hover:bg-ink-50">
            <SvgoShield class="text-base text-ink-400" /> {{ $t('nav.admin') }}
          </NuxtLink>
          <button class="flex w-full items-center gap-2 px-5 py-4 text-sm font-medium text-rose-600 transition hover:bg-rose-50" @click="logout">
            <SvgoLogout class="text-base" /> {{ $t('nav.logout') }}
          </button>
        </div>
      </aside>

      <!-- Right -->
      <div class="space-y-8">
        <section>
          <h2 class="section-title mb-4">{{ $t('profile.myTeam') }}</h2>
          <TeamCard v-if="myTeam" :team="myTeam" />
          <UiEmptyState v-else :title="$t('profile.noTeams')" :text="$t('profile.noTeamsText')">
            <template #icon><SvgoUsers class="text-2xl" /></template>
            <div class="flex flex-wrap justify-center gap-2">
              <UiButton to="/teams" variant="secondary" size="sm">{{ $t('teams.findTeam') }}</UiButton>
              <UiButton to="/teams/new" size="sm"><SvgoPlus class="text-base" /> {{ $t('teams.create') }}</UiButton>
            </div>
          </UiEmptyState>
        </section>

        <!-- Open join requests (only while I'm not in a team) -->
        <section v-if="myRequests.length">
          <div class="mb-4 flex items-center justify-between">
            <h2 class="section-title">{{ $t('invitations.myRequests') }}</h2>
            <NuxtLink to="/invitations" class="text-sm font-medium text-brand-600 hover:text-brand-700">{{ $t('common.seeAll') }}</NuxtLink>
          </div>
          <div class="card divide-y divide-ink-100">
            <NuxtLink v-for="r in myRequests" :key="r.teamId" :to="`/teams/${r.teamId}`" class="flex items-center gap-3 px-5 py-3.5 transition hover:bg-ink-50">
              <TeamBadge :team="{ name: r.teamName, color: r.teamColor }" size="sm" />
              <span class="flex-1 truncate font-medium text-ink-800">{{ r.teamName }}</span>
              <span class="chip bg-amber-50 text-amber-700"><SvgoClock class="text-xs" /> {{ $t('teams.requested') }}</span>
            </NuxtLink>
          </div>
        </section>

        <section>
          <h2 class="section-title mb-4">{{ $t('profile.myMatches') }}</h2>
          <div v-if="myMatches.length" class="grid grid-cols-1 gap-3 md:grid-cols-2">
            <MatchCard v-for="m in myMatches.slice(0, 6)" :key="m.id" :match="m" />
          </div>
          <UiEmptyState v-else :title="$t('profile.noMatches')" />
        </section>
      </div>
    </div>
  </div>
</template>
