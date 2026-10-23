<script setup lang="ts">
import VueDatePicker from '@vuepic/vue-datepicker'
import { addDays, format, parseISO } from 'date-fns'
import { az } from 'date-fns/locale'
import { PLAYERS_PER_MATCH, citiesOf } from '~/utils/rules'
import type { Field, MatchSummary, Slot, Team, TeamMemberDto } from '~/types'

definePageMeta({ middleware: 'auth' })

const api = useApi()
const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const inbox = useInboxStore()
const notify = useNotify()
const { longDate, timeRange, money } = useFormat()
const { t } = useI18n()

useHead({ title: () => t('book.title') })

// One team per player: the home team is simply the team I own. Only its captain can book.
const myTeam = computed(() => auth.team ?? undefined)
const homeTeam = computed(() => (auth.team && auth.team.ownerId === auth.userId ? auth.team : undefined))

// GET /fields, GET /teams, and my squad for the lineup (GET /teams/{id}/members)
const { data: allFields } = await useAsyncData('book-fields', () => api<Field[]>('/fields'))
const { data: allTeams } = await useAsyncData('book-teams', () => api<Team[]>('/teams'))
const { data: homeMembers } = await useAsyncData('book-home-members', () =>
  homeTeam.value ? api<TeamMemberDto[]>(`/teams/${homeTeam.value.id}/members`) : Promise.resolve([]),
)

// ---------- The choices (the URL can pre-fill the field, e.g. from the home page) ----------
const tomorrow = addDays(new Date(), 1)
const fieldId = ref<number | null>(route.query.fieldId ? Number(route.query.fieldId) : null)
const date = ref<string>(format(tomorrow, 'yyyy-MM-dd'))
const time = ref<string | null>(null)
const awayTeamId = ref<number | null>(null)
const lineup = ref<number[]>([])

const field = computed(() => allFields.value?.find(f => f.id === fieldId.value))
const awayTeam = computed(() => allTeams.value?.find(team => team.id === awayTeamId.value))

// A different field or day means a different set of free slots.
watch([fieldId, date], () => (time.value = null))

// ---------- Step 1: field ----------
const city = ref('')
const grass = ref('')
const cities = computed(() => citiesOf(allFields.value ?? []))
const fields = computed(() =>
  (allFields.value ?? [])
    .filter(f => !city.value || f.city === city.value)
    .filter(f => !grass.value || f.grassType === grass.value)
    .sort((a, b) => a.name.localeCompare(b.name)),
)

// ---------- Step 2: day + slot (from tomorrow on, like the backend rule) ----------
const days = computed(() =>
  Array.from({ length: 14 }, (_, i) => {
    const d = addDays(tomorrow, i)
    return {
      value: format(d, 'yyyy-MM-dd'),
      weekday: format(d, 'EEE', { locale: az }),
      day: format(d, 'd'),
      month: format(d, 'MMM', { locale: az }),
    }
  }),
)

const pickerDate = computed({
  get: () => parseISO(date.value),
  set: (d: Date | null) => {
    if (d) date.value = format(d, 'yyyy-MM-dd')
  },
})

// GET /fields/{id}/slots?date=… : loads again whenever the field or the day changes
const { data: slotsData } = await useAsyncData(
  'book-slots',
  () => (fieldId.value ? api<Slot[]>(`/fields/${fieldId.value}/slots`, { query: { date: date.value } }) : Promise.resolve([])),
  { watch: [fieldId, date] },
)
const slots = computed(() => slotsData.value ?? [])
const freeCount = computed(() => slots.value.filter(s => s.isFree).length)

// ---------- Step 3: opponent ----------
const opponentSearch = ref('')
const opponents = computed(() =>
  (allTeams.value ?? [])
    .filter(team => team.id !== homeTeam.value?.id)
    .filter(team => team.name.toLowerCase().includes(opponentSearch.value.toLowerCase())),
)

// ---------- Wizard ----------
// If the field came from the URL, start directly at "day + time".
const step = ref(field.value ? 2 : 1)
const loading = ref(false)

const steps = computed(() => [
  { n: 1, label: t('book.stepField') },
  { n: 2, label: t('book.stepSlot') },
  { n: 3, label: t('book.stepOpponent') },
  { n: 4, label: t('book.stepLineup') },
  { n: 5, label: t('book.stepReview') },
])

const canNext = computed(() => {
  if (step.value === 1) return !!fieldId.value
  if (step.value === 2) return !!time.value
  if (step.value === 3) return !!awayTeamId.value
  if (step.value === 4) return lineup.value.length === PLAYERS_PER_MATCH
  return true
})

function chooseField(id: number) {
  fieldId.value = id
  step.value = 2
}

const lineupNames = computed(() =>
  lineup.value.map(id => homeMembers.value?.find(m => m.userId === id)?.displayName.split(' ')[0]).join(', '),
)

// POST /matches → 201 + the new match. The backend checks every rule (tomorrow+, free slot, 8 players...).
async function submit() {
  loading.value = true
  try {
    const match = await api<MatchSummary>('/matches', {
      method: 'POST',
      body: {
        fieldId: fieldId.value,
        date: date.value,
        startTime: time.value, // "18:00:00", exactly as the slots endpoint sent it
        homeTeamId: homeTeam.value!.id,
        awayTeamId: awayTeamId.value,
        playerIds: lineup.value,
      },
    })
    notify.success('book.sent', { team: awayTeam.value?.name })
    await inbox.refresh()
    router.push(`/matches/${match.id}`)
  }
  catch (error) {
    notify.errorText(apiErrorMessage(error, t('errors.network')))
  }
  finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="container-page max-w-4xl">
    <PageHeader :title="$t('book.title')" :subtitle="$t('book.subtitle')" />

    <!-- Not in any team yet -->
    <UiEmptyState v-if="!myTeam" :title="$t('book.noTeamTitle')" :text="$t('book.noTeamText')">
      <template #icon><SvgoUsers class="text-2xl" /></template>
      <div class="flex flex-wrap justify-center gap-2">
        <UiButton to="/teams" variant="secondary" size="sm">{{ $t('teams.findTeam') }}</UiButton>
        <UiButton to="/teams/new" size="sm">{{ $t('teams.create') }}</UiButton>
      </div>
    </UiEmptyState>

    <!-- In a team, but not its captain -->
    <UiEmptyState v-else-if="!homeTeam" :title="$t('book.onlyCaptainTitle')" :text="$t('book.onlyCaptainText', { team: myTeam.name })">
      <template #icon><SvgoCrown class="text-2xl" /></template>
      <UiButton :to="`/teams/${myTeam.id}`" size="sm">{{ myTeam.name }}</UiButton>
    </UiEmptyState>

    <template v-else>
      <!-- Summary of what's chosen so far -->
      <div class="card mb-6 flex flex-wrap items-center gap-x-6 gap-y-3 p-4 sm:p-5">
        <div class="flex items-center gap-2 text-sm">
          <TeamBadge :team="homeTeam" size="sm" /> <span class="font-medium text-ink-800">{{ homeTeam.name }}</span>
        </div>
        <div class="flex items-center gap-2 text-sm" :class="field ? 'text-ink-700' : 'text-ink-300'">
          <SvgoField class="text-base" /> {{ field?.name ?? $t('book.noFieldYet') }}
        </div>
        <div class="flex items-center gap-2 text-sm" :class="time ? 'text-ink-700' : 'text-ink-300'">
          <SvgoCalendar class="text-base" />
          <span class="capitalize">{{ time ? `${longDate(date)}, ${timeRange(time)}` : $t('book.noTimeYet') }}</span>
        </div>
      </div>

      <!-- Stepper -->
      <ol class="mb-6 grid grid-cols-5 gap-2">
        <li v-for="s in steps" :key="s.n">
          <div class="mb-2 h-1.5 rounded-full transition-colors duration-300" :class="step >= s.n ? 'bg-brand-500' : 'bg-ink-200'" />
          <p class="truncate text-[11px] font-medium sm:text-sm" :class="step >= s.n ? 'text-ink-800' : 'text-ink-400'">{{ s.label }}</p>
        </li>
      </ol>

      <div class="card p-5 sm:p-7">
        <div :key="step" class="animate-step-in">
          <!-- Step 1: field -->
          <div v-if="step === 1">
            <h2 class="section-title mb-1">{{ $t('book.chooseField') }}</h2>
            <p class="mb-5 text-sm text-ink-500">{{ $t('book.chooseFieldText') }}</p>
            <div class="mb-5 flex flex-wrap gap-2">
              <select v-model="city" class="input w-auto">
                <option value="">{{ $t('common.allCities') }}</option>
                <option v-for="c in cities" :key="c" :value="c">{{ c }}</option>
              </select>
              <select v-model="grass" class="input w-auto">
                <option value="">{{ $t('fields.anyGrass') }}</option>
                <option value="Artificial">{{ $t('grass.Artificial') }}</option>
                <option value="Natural">{{ $t('grass.Natural') }}</option>
              </select>
            </div>
            <div v-if="fields.length" class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
              <FieldCard v-for="f in fields" :key="f.id" :field="f" :selected="f.id === fieldId" @select="chooseField(f.id)" />
            </div>
            <UiEmptyState v-else :title="$t('fields.noneTitle')" :text="$t('fields.noneText')">
              <template #icon><SvgoField class="text-2xl" /></template>
            </UiEmptyState>
          </div>

          <!-- Step 2: day + time -->
          <div v-else-if="step === 2 && field">
            <div class="mb-4 flex items-center justify-between gap-3">
              <div>
                <h2 class="section-title mb-1">{{ $t('book.chooseSlot') }}</h2>
                <p class="text-sm text-ink-500">{{ field.name }} · {{ $t(`grass.${field.grassType}`) }} · {{ money(field.pricePerHour) }}/{{ $t('common.h') }}</p>
              </div>
              <ClientOnly>
                <VueDatePicker
                  v-model="pickerDate"
                  :min-date="tomorrow"
                  :enable-time-picker="false"
                  locale="az"
                  auto-apply
                  :clearable="false"
                >
                  <template #trigger>
                    <button class="inline-flex shrink-0 items-center gap-1.5 rounded-xl border border-ink-200 px-3 py-2 text-sm font-medium text-ink-600 transition hover:bg-ink-50">
                      <SvgoCalendar class="text-base" /> {{ $t('fields.otherDate') }}
                    </button>
                  </template>
                </VueDatePicker>
              </ClientOnly>
            </div>

            <div class="no-scrollbar -mx-1 mb-6 flex gap-2 overflow-x-auto px-1 pb-1">
              <button
                v-for="d in days"
                :key="d.value"
                class="flex w-16 shrink-0 flex-col items-center rounded-2xl border py-2.5 transition"
                :class="date === d.value ? 'border-brand-500 bg-brand-500 text-white shadow-md shadow-brand-500/30' : 'border-ink-100 bg-white text-ink-700 hover:border-ink-300'"
                @click="date = d.value"
              >
                <span class="text-[11px] uppercase opacity-80">{{ d.weekday }}</span>
                <span class="text-lg font-bold">{{ d.day }}</span>
                <span class="text-[11px] opacity-80">{{ d.month }}</span>
              </button>
            </div>

            <div class="mb-3 flex items-center justify-between text-sm">
              <p class="font-medium capitalize text-ink-700">{{ longDate(date) }}</p>
              <p class="text-ink-400">{{ $t('fields.freeSlots', { n: freeCount }) }}</p>
            </div>
            <SlotGrid :slots="slots" :selected="time" @select="time = $event" />
          </div>

          <!-- Step 3: opponent -->
          <div v-else-if="step === 3">
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

          <!-- Step 4: lineup -->
          <div v-else-if="step === 4">
            <h2 class="section-title mb-1">{{ $t('book.chooseLineup', { n: PLAYERS_PER_MATCH }) }}</h2>
            <p class="mb-5 text-sm text-ink-500">{{ $t('book.chooseLineupText') }}</p>
            <LineupPicker v-model="lineup" :members="homeMembers ?? []" />
          </div>

          <!-- Step 5: review -->
          <div v-else-if="step === 5 && field && time">
            <h2 class="section-title mb-5">{{ $t('book.review') }}</h2>
            <div v-if="awayTeam" class="mb-6 flex items-center justify-around rounded-2xl bg-ink-50 p-6">
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
              <div class="flex justify-between"><dt class="text-ink-500">{{ $t('fields.field') }}</dt><dd class="font-medium">{{ field.name }}, {{ field.city }}</dd></div>
              <div class="flex justify-between"><dt class="text-ink-500">{{ $t('fields.priceTotal') }}</dt><dd class="font-medium">{{ money(field.pricePerHour * 1.5) }}</dd></div>
              <div class="flex justify-between gap-6"><dt class="shrink-0 text-ink-500">{{ $t('match.lineup') }}</dt><dd class="text-right font-medium">{{ lineupNames }}</dd></div>
            </dl>
            <p class="mt-6 flex items-start gap-2 rounded-xl bg-brand-50 px-4 py-3 text-sm text-brand-800">
              <SvgoClock class="mt-0.5 text-base" /> {{ $t('book.expiryNote') }}
            </p>
            <p class="mt-3 flex items-start gap-2 text-xs text-ink-400">
              <SvgoInfo class="mt-0.5 text-sm" /> {{ $t('fields.paymentNote') }}
            </p>
          </div>
        </div>

        <div class="mt-8 flex justify-between gap-3 border-t border-ink-100 pt-5">
          <UiButton v-if="step > 1" variant="secondary" @click="step--"><SvgoChevronLeft class="text-base" /> {{ $t('common.back') }}</UiButton>
          <span v-else />
          <UiButton v-if="step < 5" :disabled="!canNext" @click="step++">{{ $t('common.next') }} <SvgoChevronRight class="text-base" /></UiButton>
          <UiButton v-else :loading="loading" @click="submit"><SvgoSend class="text-base" /> {{ $t('book.send') }}</UiButton>
        </div>
      </div>
    </template>
  </div>
</template>
