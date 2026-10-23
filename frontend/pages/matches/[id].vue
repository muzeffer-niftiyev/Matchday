<script setup lang="ts">
import { PLAYERS_PER_MATCH, hhmm, matchEnd, matchStart } from '~/utils/rules'
import type { MatchDetails, TeamMemberDto } from '~/types'

const api = useApi()
const route = useRoute()
const auth = useAuthStore()
const inbox = useInboxStore()
const notify = useNotify()
const { longDate, timeRange } = useFormat()
const { t } = useI18n()

const matchId = computed(() => Number(route.params.id))

// GET /matches/{id} → everything about one match, lineups included (404 → null → "not found")
const { data: match, refresh } = await useAsyncData(`match-${matchId.value}`, () =>
  api<MatchDetails>(`/matches/${matchId.value}`),
)

// The template uses team objects (TeamBadge needs name + color)
const home = computed(() => match.value && {
  id: match.value.homeTeamId, name: match.value.homeTeamName, color: match.value.homeTeamColor,
  ownerId: match.value.homeTeamOwnerId, lineup: match.value.homeLineup,
})
const away = computed(() => match.value && {
  id: match.value.awayTeamId, name: match.value.awayTeamName, color: match.value.awayTeamColor,
  ownerId: match.value.awayTeamOwnerId, lineup: match.value.awayLineup,
})
const status = computed(() => match.value?.status ?? 'Pending')

useHead({ title: () => (match.value ? `${match.value.homeTeamName} vs ${match.value.awayTeamName}` : t('nav.matches')) })

const hasScore = computed(() => match.value?.homeScore !== null && match.value?.homeScore !== undefined)
const isHomeOwner = computed(() => match.value?.homeTeamOwnerId === auth.userId)
const isAwayOwner = computed(() => match.value?.awayTeamOwnerId === auth.userId)
const isFinished = computed(() => (match.value ? matchEnd(match.value) <= new Date() : false))
const hasStarted = computed(() => (match.value ? matchStart(match.value) <= new Date() : false))

// Calls one match action (POST /matches/{id}/...), then reloads the match and the inbox.
async function run(path: string, body: object | undefined, successKey: string) {
  try {
    await api(`/matches/${matchId.value}/${path}`, { method: 'POST', body })
    notify.success(successKey)
    await refresh()
    await inbox.refresh()
    return true
  }
  catch (error) {
    notify.errorText(apiErrorMessage(error, t('errors.network')))
    return false
  }
}

// Accept with lineup: the away captain picks 8 players from their squad
const acceptOpen = ref(false)
const awayLineup = ref<number[]>([])
const awayMembers = ref<TeamMemberDto[]>([])
async function openAccept() {
  awayLineup.value = []
  awayMembers.value = await api<TeamMemberDto[]>(`/teams/${match.value!.awayTeamId}/members`)
  acceptOpen.value = true
}
async function accept() {
  if (await run('accept', { playerIds: awayLineup.value }, 'match.accepted')) acceptOpen.value = false
}

const declineOpen = ref(false)
async function decline() {
  await run('decline', undefined, 'match.declined')
  declineOpen.value = false
}

const cancelOpen = ref(false)
async function cancel() {
  await run('cancel', undefined, 'match.cancelled')
  cancelOpen.value = false
}

// Admin score
const scoreOpen = ref(false)
const score = reactive({ home: 0, away: 0 })
function openScore() {
  score.home = match.value?.homeScore ?? 0
  score.away = match.value?.awayScore ?? 0
  scoreOpen.value = true
}
async function saveScore() {
  if (await run('score', { homeScore: score.home, awayScore: score.away }, 'match.scoreSaved')) scoreOpen.value = false
}
</script>

<template>
  <div class="container-page max-w-4xl">
    <template v-if="match && home && away">
      <NuxtLink to="/matches" class="mb-4 inline-flex items-center gap-1 text-sm font-medium text-ink-500 transition hover:text-ink-800">
        <SvgoChevronLeft class="text-base" /> {{ $t('nav.matches') }}
      </NuxtLink>

      <!-- Scoreboard -->
      <section class="card relative mb-6 overflow-hidden">
        <div class="absolute inset-0 opacity-[0.07]" :style="{ background: `linear-gradient(90deg, ${home.color}, transparent 45%, transparent 55%, ${away.color})` }" />
        <div class="relative px-5 pb-6 pt-5 sm:px-8 sm:pb-8">
          <div class="mb-6 flex flex-wrap items-center justify-center gap-2 text-sm text-ink-500">
            <StatusBadge :status="status" />
            <span v-if="status === 'Pending'" class="chip bg-amber-50 text-amber-700"><Countdown :until="match.expiresAt" /></span>
          </div>

          <div class="grid grid-cols-[1fr_auto_1fr] items-center gap-3 sm:gap-6">
            <NuxtLink :to="`/teams/${home.id}`" class="group flex flex-col items-center gap-3 text-center">
              <TeamBadge :team="home" size="xl" class="transition group-hover:scale-105" />
              <div>
                <p class="font-bold text-ink-900 group-hover:text-brand-600 sm:text-lg">{{ home.name }}</p>
                <p class="text-xs text-ink-400">{{ $t('match.home') }}</p>
              </div>
            </NuxtLink>

            <div class="text-center">
              <p v-if="hasScore" class="text-4xl font-extrabold tabular-nums tracking-tight text-ink-900 sm:text-6xl">
                {{ match.homeScore }}<span class="mx-1 text-ink-300 sm:mx-2">:</span>{{ match.awayScore }}
              </p>
              <p v-else class="rounded-2xl bg-ink-100 px-4 py-2 text-lg font-bold text-ink-400">{{ hhmm(match.startTime) }}</p>
            </div>

            <NuxtLink :to="`/teams/${away.id}`" class="group flex flex-col items-center gap-3 text-center">
              <TeamBadge :team="away" size="xl" class="transition group-hover:scale-105" />
              <div>
                <p class="font-bold text-ink-900 group-hover:text-brand-600 sm:text-lg">{{ away.name }}</p>
                <p class="text-xs text-ink-400">{{ $t('match.away') }}</p>
              </div>
            </NuxtLink>
          </div>

          <div class="mt-8 flex flex-wrap items-center justify-center gap-x-6 gap-y-2 text-sm text-ink-500">
            <span class="inline-flex items-center gap-1.5 capitalize"><SvgoCalendar class="text-base" /> {{ longDate(match.date) }}</span>
            <span class="inline-flex items-center gap-1.5"><SvgoClock class="text-base" /> {{ timeRange(match.startTime) }}</span>
            <span class="inline-flex items-center gap-1.5"><SvgoPin class="text-base" /> {{ match.fieldName }}, {{ match.fieldCity }}</span>
          </div>
        </div>

        <!-- Actions for the people involved -->
        <div
          v-if="(status === 'Pending' && (isAwayOwner || isHomeOwner)) || (status === 'Confirmed' && isHomeOwner && !hasStarted) || (auth.isAdmin && (status === 'Confirmed' || status === 'Completed'))"
          class="relative flex flex-col gap-3 border-t border-ink-100 bg-ink-50/70 px-5 py-4 sm:flex-row sm:items-center sm:justify-between sm:px-8"
        >
          <p class="text-sm text-ink-600">
            <template v-if="status === 'Pending' && isAwayOwner">{{ $t('match.invitedText', { team: home.name }) }}</template>
            <template v-else-if="status === 'Pending' && isHomeOwner">{{ $t('match.waitingText', { team: away.name }) }}</template>
            <template v-else-if="auth.isAdmin && !isFinished">{{ $t('match.adminNotFinished') }}</template>
            <template v-else-if="auth.isAdmin">{{ $t('match.adminScoreText') }}</template>
            <template v-else>{{ $t('match.confirmedOwnerText') }}</template>
          </p>
          <div class="flex gap-2">
            <template v-if="status === 'Pending' && isAwayOwner">
              <UiButton variant="secondary" @click="declineOpen = true">{{ $t('match.decline') }}</UiButton>
              <UiButton @click="openAccept"><SvgoCheck class="text-base" /> {{ $t('match.accept') }}</UiButton>
            </template>
            <UiButton
              v-if="isHomeOwner && (status === 'Pending' || (status === 'Confirmed' && !hasStarted))"
              variant="ghost"
              class="!text-rose-600 hover:!bg-rose-50"
              @click="cancelOpen = true"
            >{{ $t('match.cancelMatch') }}</UiButton>
            <UiButton v-if="auth.isAdmin && (status === 'Confirmed' || status === 'Completed')" :disabled="!isFinished" @click="openScore">
              <SvgoFlag class="text-base" /> {{ hasScore ? $t('match.editScore') : $t('match.enterScore') }}
            </UiButton>
          </div>
        </div>
      </section>

      <!-- Lineups -->
      <section class="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div v-for="side in [home, away]" :key="side.id" class="card p-5">
          <div class="mb-4 flex items-center justify-between">
            <div class="flex items-center gap-2.5">
              <TeamBadge :team="side" size="sm" />
              <h3 class="font-semibold text-ink-900">{{ side.name }}</h3>
            </div>
            <span class="text-xs text-ink-400">{{ side.lineup.length }} / {{ PLAYERS_PER_MATCH }}</span>
          </div>
          <ul v-if="side.lineup.length" class="space-y-1">
            <li v-for="(player, i) in side.lineup" :key="player.userId" class="flex items-center gap-3 rounded-xl px-2 py-1.5 transition hover:bg-ink-50">
              <span class="w-5 text-right text-xs font-semibold text-ink-300">{{ i + 1 }}</span>
              <UiAvatar :name="player.displayName" size="xs" />
              <span class="text-sm text-ink-700">{{ player.displayName }}</span>
              <SvgoCrown v-if="player.userId === side.ownerId" class="text-xs text-amber-500" />
            </li>
          </ul>
          <p v-else class="rounded-xl bg-ink-50 px-4 py-6 text-center text-sm text-ink-400">{{ $t('match.lineupPending') }}</p>
        </div>
      </section>

      <!-- Accept modal -->
      <UiModal v-model="acceptOpen" :title="$t('match.acceptTitle')" :description="$t('match.acceptText', { n: PLAYERS_PER_MATCH })" size="lg">
        <LineupPicker v-model="awayLineup" :members="awayMembers" :blocked="match.homeLineup.map(p => p.userId)" />
        <template #footer>
          <UiButton variant="secondary" @click="acceptOpen = false">{{ $t('common.cancel') }}</UiButton>
          <UiButton :disabled="awayLineup.length !== PLAYERS_PER_MATCH" @click="accept"><SvgoCheck class="text-base" /> {{ $t('match.confirmGame') }}</UiButton>
        </template>
      </UiModal>

      <UiModal v-model="declineOpen" :title="$t('match.declineTitle')" :description="$t('match.declineText', { team: home.name })" size="sm">
        <template #footer>
          <UiButton variant="secondary" @click="declineOpen = false">{{ $t('common.back') }}</UiButton>
          <UiButton variant="danger" @click="decline">{{ $t('match.decline') }}</UiButton>
        </template>
      </UiModal>

      <UiModal v-model="cancelOpen" :title="$t('match.cancelTitle')" :description="$t('match.cancelText')" size="sm">
        <template #footer>
          <UiButton variant="secondary" @click="cancelOpen = false">{{ $t('common.back') }}</UiButton>
          <UiButton variant="danger" @click="cancel">{{ $t('match.cancelMatch') }}</UiButton>
        </template>
      </UiModal>

      <!-- Score modal (admin) -->
      <UiModal v-model="scoreOpen" :title="$t('match.enterScore')" size="sm">
        <div class="grid grid-cols-[1fr_auto_1fr] items-end gap-3">
          <div class="text-center">
            <TeamBadge :team="home" class="mb-2" />
            <p class="mb-2 truncate text-xs font-medium text-ink-600">{{ home.name }}</p>
            <input v-model.number="score.home" type="number" min="0" max="99" class="input text-center text-2xl font-bold">
          </div>
          <span class="pb-3 text-2xl font-bold text-ink-300">:</span>
          <div class="text-center">
            <TeamBadge :team="away" class="mb-2" />
            <p class="mb-2 truncate text-xs font-medium text-ink-600">{{ away.name }}</p>
            <input v-model.number="score.away" type="number" min="0" max="99" class="input text-center text-2xl font-bold">
          </div>
        </div>
        <template #footer>
          <UiButton variant="secondary" @click="scoreOpen = false">{{ $t('common.cancel') }}</UiButton>
          <UiButton :disabled="score.home < 0 || score.away < 0 || score.home > 99 || score.away > 99" @click="saveScore">{{ $t('common.save') }}</UiButton>
        </template>
      </UiModal>
    </template>

    <UiEmptyState v-else :title="$t('errors.notFound')">
      <UiButton to="/matches" size="sm">{{ $t('nav.matches') }}</UiButton>
    </UiEmptyState>
  </div>
</template>
