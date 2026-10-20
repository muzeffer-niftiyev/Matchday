<script setup lang="ts">
import { matchStart } from '~/stores/data'

definePageMeta({ middleware: 'auth' })

const data = useDataStore()
const auth = useAuthStore()
const notify = useNotify()
const router = useRouter()
const { locale, locales, setLocale, t } = useI18n()

useHead({ title: () => t('nav.profile') })

const myTeams = computed(() => data.teamsOfUser(auth.userId!))

const myMatches = computed(() =>
  data.matches
    .filter(m => [...m.homeLineup, ...m.awayLineup].includes(auth.userId!))
    .sort((a, b) => matchStart(b).getTime() - matchStart(a).getTime()),
)

const played = computed(() => myMatches.value.filter(m => m.status === 'Completed').length)
const upcoming = computed(() => myMatches.value.filter(m => data.statusOf(m) === 'Confirmed' && matchStart(m) > new Date()))

const resetOpen = ref(false)

function logout() {
  auth.logout()
  notify.info('auth.loggedOut')
  router.push('/')
}

function resetDemo() {
  const userId = auth.userId
  data.resetDemo()
  if (userId && !data.user(userId)) auth.loginAs(1)
  resetOpen.value = false
  notify.success('profile.resetDone')
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
            <div><p class="text-lg font-bold text-ink-900">{{ myTeams.length }}</p><p class="text-[11px] text-ink-500">{{ $t('profile.teams') }}</p></div>
            <div><p class="text-lg font-bold text-ink-900">{{ played }}</p><p class="text-[11px] text-ink-500">{{ $t('profile.played') }}</p></div>
            <div><p class="text-lg font-bold text-ink-900">{{ upcoming.length }}</p><p class="text-[11px] text-ink-500">{{ $t('profile.upcoming') }}</p></div>
          </div>
        </div>

        <div class="card divide-y divide-ink-100">
          <div class="flex items-center justify-between px-5 py-4">
            <span class="flex items-center gap-2 text-sm font-medium text-ink-700"><SvgoGlobe class="text-base text-ink-400" /> {{ $t('profile.language') }}</span>
            <div class="flex gap-1 rounded-xl bg-ink-100 p-1">
              <button
                v-for="l in locales"
                :key="l.code"
                class="rounded-lg px-2.5 py-1 text-xs font-semibold uppercase transition"
                :class="locale === l.code ? 'bg-white text-ink-900 shadow-sm' : 'text-ink-500'"
                @click="setLocale(l.code)"
              >{{ l.code }}</button>
            </div>
          </div>
          <NuxtLink v-if="auth.isAdmin" to="/admin" class="flex items-center gap-2 px-5 py-4 text-sm font-medium text-ink-700 transition hover:bg-ink-50">
            <SvgoShield class="text-base text-ink-400" /> {{ $t('nav.admin') }}
          </NuxtLink>
          <button class="flex w-full items-center gap-2 px-5 py-4 text-sm font-medium text-ink-700 transition hover:bg-ink-50" @click="resetOpen = true">
            <SvgoSettings class="text-base text-ink-400" /> {{ $t('profile.resetDemo') }}
          </button>
          <button class="flex w-full items-center gap-2 px-5 py-4 text-sm font-medium text-rose-600 transition hover:bg-rose-50" @click="logout">
            <SvgoLogout class="text-base" /> {{ $t('nav.logout') }}
          </button>
        </div>
      </aside>

      <!-- Right -->
      <div class="space-y-8">
        <section>
          <div class="mb-4 flex items-center justify-between">
            <h2 class="section-title">{{ $t('profile.myTeams') }}</h2>
            <UiButton to="/teams/new" variant="soft" size="sm"><SvgoPlus class="text-base" /> {{ $t('teams.create') }}</UiButton>
          </div>
          <div v-if="myTeams.length" class="grid grid-cols-1 gap-3 md:grid-cols-2">
            <TeamCard v-for="team in myTeams" :key="team.id" :team="team" />
          </div>
          <UiEmptyState v-else :title="$t('profile.noTeams')" :text="$t('profile.noTeamsText')">
            <template #icon><SvgoUsers class="text-2xl" /></template>
            <UiButton to="/teams" size="sm">{{ $t('nav.teams') }}</UiButton>
          </UiEmptyState>
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

    <UiModal v-model="resetOpen" :title="$t('profile.resetDemo')" :description="$t('profile.resetText')" size="sm">
      <template #footer>
        <UiButton variant="secondary" @click="resetOpen = false">{{ $t('common.cancel') }}</UiButton>
        <UiButton @click="resetDemo">{{ $t('profile.resetConfirm') }}</UiButton>
      </template>
    </UiModal>
  </div>
</template>
