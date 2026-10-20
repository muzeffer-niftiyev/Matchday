<script setup lang="ts">
import { PLAYERS_PER_MATCH } from '~/utils/rules'

definePageMeta({ middleware: 'auth' })

const route = useRoute()
const router = useRouter()
const data = useDataStore()
const auth = useAuthStore()
const notify = useNotify()
const { longDate, timeRange, money } = useFormat()
const { t } = useI18n()

useHead({ title: () => t('book.title') })

const fieldId = Number(route.query.fieldId)
const date = String(route.query.date ?? '')
const time = String(route.query.time ?? '')
const field = computed(() => data.field(fieldId))

const myTeams = computed(() => data.ownedTeams(auth.userId!))

const step = ref(1)
const homeTeamId = ref<number | null>(myTeams.value.length === 1 ? myTeams.value[0].id : null)
const awayTeamId = ref<number | null>(null)
const lineup = ref<number[]>([])
const opponentSearch = ref('')
const loading = ref(false)

watch(homeTeamId, () => (lineup.value = []))

const homeTeam = computed(() => (homeTeamId.value ? data.team(homeTeamId.value) : undefined))
const awayTeam = computed(() => (awayTeamId.value ? data.team(awayTeamId.value) : undefined))

const opponents = computed(() =>
  data.teams
    .filter(team => team.id !== homeTeamId.value)
    .filter(team => team.name.toLowerCase().includes(opponentSearch.value.toLowerCase())),
)

const steps = computed(() => [
  { n: 1, label: t('book.stepTeam') },
  { n: 2, label: t('book.stepOpponent') },
  { n: 3, label: t('book.stepLineup') },
  { n: 4, label: t('book.stepReview') },
])

const canNext = computed(() => {
  if (step.value === 1) return !!homeTeamId.value
  if (step.value === 2) return !!awayTeamId.value
  if (step.value === 3) return lineup.value.length === PLAYERS_PER_MATCH
  return true
})

async function submit() {
  loading.value = true
  await new Promise(r => setTimeout(r, 600))
  const result = data.bookMatch(auth.userId!, {
    fieldId,
    date,
    startTime: time,
    homeTeamId: homeTeamId.value!,
    awayTeamId: awayTeamId.value!,
    playerIds: lineup.value,
  })
  loading.value = false

  if (notify.result(result, 'book.sent', { team: awayTeam.value?.name }) && result.ok) {
    router.push(`/matches/${result.id}`)
  }
}
</script>

<template>
  <div class="container-page max-w-3xl">
    <PageHeader :title="$t('book.title')" :subtitle="$t('book.subtitle')" :back="field ? `/fields/${field.id}?date=${date}` : '/fields'" />

    <UiEmptyState v-if="!field || !date || !time" :title="$t('book.missingTitle')" :text="$t('book.missingText')">
      <UiButton to="/fields" size="sm">{{ $t('nav.fields') }}</UiButton>
    </UiEmptyState>

    <UiEmptyState v-else-if="!myTeams.length" :title="$t('book.noTeamTitle')" :text="$t('book.noTeamText')">
      <template #icon><SvgoUsers class="text-2xl" /></template>
      <UiButton to="/teams/new" size="sm">{{ $t('teams.create') }}</UiButton>
    </UiEmptyState>

    <template v-else>
      <!-- Where and when -->
      <div class="card mb-6 flex flex-wrap items-center gap-x-6 gap-y-3 p-5">
        <div class="flex items-center gap-3">
          <span class="flex h-11 w-11 items-center justify-center rounded-2xl bg-gradient-to-br text-white" :class="field.gradient"><SvgoField class="text-xl" /></span>
          <div>
            <p class="font-semibold text-ink-900">{{ field.name }}</p>
            <p class="text-xs text-ink-500">{{ field.city }}</p>
          </div>
        </div>
        <div class="flex items-center gap-2 text-sm text-ink-600"><SvgoCalendar class="text-base text-ink-400" /> <span class="capitalize">{{ longDate(date) }}</span></div>
        <div class="flex items-center gap-2 text-sm text-ink-600"><SvgoClock class="text-base text-ink-400" /> {{ timeRange(time) }}</div>
      </div>

      <!-- Stepper -->
      <ol class="mb-6 grid grid-cols-4 gap-2">
        <li v-for="s in steps" :key="s.n">
          <div class="mb-2 h-1.5 rounded-full transition-colors duration-300" :class="step >= s.n ? 'bg-brand-500' : 'bg-ink-200'" />
          <p class="text-xs font-medium sm:text-sm" :class="step >= s.n ? 'text-ink-800' : 'text-ink-400'">{{ s.label }}</p>
        </li>
      </ol>

      <div class="card p-5 sm:p-7">
        <div :key="step" class="animate-step-in">
          <!-- Step 1: your team -->
          <div v-if="step === 1" key="1">
            <h2 class="section-title mb-1">{{ $t('book.chooseTeam') }}</h2>
            <p class="mb-5 text-sm text-ink-500">{{ $t('book.chooseTeamText') }}</p>
            <div class="grid grid-cols-1 gap-3 sm:grid-cols-2">
              <button
                v-for="team in myTeams"
                :key="team.id"
                class="flex items-center gap-3 rounded-2xl border p-4 text-left transition"
                :class="homeTeamId === team.id ? 'border-brand-400 bg-brand-50 ring-4 ring-brand-100' : 'border-ink-200 hover:border-ink-300'"
                @click="homeTeamId = team.id"
              >
                <TeamBadge :team="team" />
                <div>
                  <p class="font-semibold text-ink-900">{{ team.name }}</p>
                  <p class="text-xs text-ink-500">{{ $t('teams.playersCount', { n: data.teamMembers(team.id).length }) }}</p>
                </div>
              </button>
            </div>
          </div>

          <!-- Step 2: opponent -->
          <div v-else-if="step === 2" key="2">
            <h2 class="section-title mb-1">{{ $t('book.chooseOpponent') }}</h2>
            <p class="mb-5 text-sm text-ink-500">{{ $t('book.chooseOpponentText') }}</p>
            <div class="relative mb-4">
              <SvgoSearch class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-base text-ink-400" />
              <input v-model="opponentSearch" class="input pl-9" :placeholder="$t('teams.search')">
            </div>
            <div class="grid grid-cols-1 max-h-96 gap-2 overflow-y-auto sm:grid-cols-2">
              <button
                v-for="team in opponents"
                :key="team.id"
                class="flex items-center gap-3 rounded-2xl border p-3 text-left transition"
                :class="awayTeamId === team.id ? 'border-brand-400 bg-brand-50 ring-4 ring-brand-100' : 'border-ink-100 hover:border-ink-300'"
                @click="awayTeamId = team.id"
              >
                <TeamBadge :team="team" size="sm" />
                <div class="min-w-0">
                  <p class="truncate text-sm font-semibold text-ink-900">{{ team.name }}</p>
                  <p class="text-xs text-ink-500">{{ team.city }}</p>
                </div>
              </button>
            </div>
          </div>

          <!-- Step 3: lineup -->
          <div v-else-if="step === 3" key="3">
            <h2 class="section-title mb-1">{{ $t('book.chooseLineup', { n: PLAYERS_PER_MATCH }) }}</h2>
            <p class="mb-5 text-sm text-ink-500">{{ $t('book.chooseLineupText') }}</p>
            <LineupPicker v-model="lineup" :team-id="homeTeamId!" />
          </div>

          <!-- Step 4: review -->
          <div v-else key="4">
            <h2 class="section-title mb-5">{{ $t('book.review') }}</h2>
            <div v-if="homeTeam && awayTeam" class="mb-6 flex items-center justify-around rounded-2xl bg-ink-50 p-6">
              <div class="flex flex-col items-center gap-2 text-center">
                <TeamBadge :team="homeTeam" size="lg" />
                <p class="text-sm font-semibold">{{ homeTeam.name }}</p>
              </div>
              <span class="rounded-xl bg-white px-3 py-1.5 text-sm font-bold text-ink-400 shadow-sm">{{ $t('match.vs') }}</span>
              <div class="flex flex-col items-center gap-2 text-center">
                <TeamBadge :team="awayTeam" size="lg" />
                <p class="text-sm font-semibold">{{ awayTeam.name }}</p>
              </div>
            </div>
            <dl class="space-y-3 text-sm">
              <div class="flex justify-between"><dt class="text-ink-500">{{ $t('fields.date') }}</dt><dd class="font-medium capitalize">{{ longDate(date) }}</dd></div>
              <div class="flex justify-between"><dt class="text-ink-500">{{ $t('fields.time') }}</dt><dd class="font-medium">{{ timeRange(time) }}</dd></div>
              <div class="flex justify-between"><dt class="text-ink-500">{{ $t('fields.field') }}</dt><dd class="font-medium">{{ field.name }}</dd></div>
              <div class="flex justify-between"><dt class="text-ink-500">{{ $t('fields.priceTotal') }}</dt><dd class="font-medium">{{ money(field.pricePerHour * 1.5) }}</dd></div>
              <div class="flex justify-between gap-6"><dt class="shrink-0 text-ink-500">{{ $t('match.lineup') }}</dt><dd class="text-right font-medium">{{ lineup.map(id => data.user(id)?.displayName.split(' ')[0]).join(', ') }}</dd></div>
            </dl>
            <p class="mt-6 flex items-start gap-2 rounded-xl bg-brand-50 px-4 py-3 text-sm text-brand-800">
              <SvgoClock class="mt-0.5 text-base" /> {{ $t('book.expiryNote') }}
            </p>
          </div>
        </div>

        <div class="mt-8 flex justify-between gap-3 border-t border-ink-100 pt-5">
          <UiButton v-if="step > 1" variant="secondary" @click="step--"><SvgoChevronLeft class="text-base" /> {{ $t('common.back') }}</UiButton>
          <span v-else />
          <UiButton v-if="step < 4" :disabled="!canNext" @click="step++">{{ $t('common.next') }} <SvgoChevronRight class="text-base" /></UiButton>
          <UiButton v-else :loading="loading" @click="submit"><SvgoSend class="text-base" /> {{ $t('book.send') }}</UiButton>
        </div>
      </div>
    </template>
  </div>
</template>
