<script setup lang="ts">
definePageMeta({ middleware: 'auth' })

const data = useDataStore()
const auth = useAuthStore()
const notify = useNotify()
const { myTeam, ownedTeam, received, sent, joinRequests, myRequests } = useInvitations()
const { niceDate, timeRange, relative } = useFormat()
const { t } = useI18n()

useHead({ title: () => t('nav.invitations') })

// Which tabs make sense depends on who I am:
// captain → match invitations + join requests; no team → my own join requests; player → nothing to answer
const tabs = computed(() => {
  if (ownedTeam.value) {
    return [
      { value: 'received', label: t('invitations.received'), count: received.value.length },
      { value: 'sent', label: t('invitations.sent'), count: sent.value.length },
      { value: 'joinRequests', label: t('invitations.joinRequests'), count: joinRequests.value.length },
    ]
  }
  if (!myTeam.value) {
    return [{ value: 'myRequests', label: t('invitations.myRequests'), count: myRequests.value.length }]
  }
  return []
})

const tab = ref('')
watchEffect(() => {
  if (!tabs.value.some(x => x.value === tab.value)) tab.value = tabs.value[0]?.value ?? ''
})

const matchList = computed(() => (tab.value === 'received' ? received.value : tab.value === 'sent' ? sent.value : []))

function cancelMyRequest(teamId: number) {
  notify.result(data.cancelRequest(teamId, auth.userId!), 'teams.requestCancelled')
}
</script>

<template>
  <div class="container-page max-w-3xl">
    <PageHeader :title="$t('nav.invitations')" :subtitle="$t('invitations.subtitle')" />

    <!-- A player in someone else's team: the captain answers invitations -->
    <UiEmptyState v-if="!tabs.length && myTeam" :title="$t('invitations.playerTitle')" :text="$t('invitations.playerText', { team: myTeam.name })">
      <template #icon><SvgoCrown class="text-2xl" /></template>
      <UiButton :to="`/teams/${myTeam.id}`" size="sm">{{ myTeam.name }}</UiButton>
    </UiEmptyState>

    <template v-else>
      <UiTabs v-if="tabs.length > 1" v-model="tab" :tabs="tabs" class="mb-6" />

      <!-- Match invitations: received / sent -->
      <template v-if="tab === 'received' || tab === 'sent'">
        <TransitionGroup v-if="matchList.length" tag="div" name="fade" class="space-y-3">
          <NuxtLink
            v-for="m in matchList"
            :key="m.id"
            :to="`/matches/${m.id}`"
            class="card card-hover flex flex-col gap-4 p-5 sm:flex-row sm:items-center"
          >
            <div class="flex items-center gap-3">
              <TeamBadge :team="data.team(tab === 'received' ? m.homeTeamId : m.awayTeamId)!" />
              <div class="min-w-0">
                <p class="text-sm text-ink-500">
                  <template v-if="tab === 'received'">{{ $t('invitations.challengedBy') }}</template>
                  <template v-else>{{ $t('invitations.youChallenged') }}</template>
                </p>
                <p class="truncate font-semibold text-ink-900">{{ data.team(tab === 'received' ? m.homeTeamId : m.awayTeamId)?.name }}</p>
              </div>
            </div>

            <div class="flex flex-1 flex-wrap gap-x-4 gap-y-1 text-sm text-ink-500 sm:justify-center">
              <span class="inline-flex items-center gap-1"><SvgoCalendar class="text-sm" /> {{ niceDate(m.date) }}</span>
              <span class="inline-flex items-center gap-1"><SvgoClock class="text-sm" /> {{ timeRange(m.startTime) }}</span>
              <span class="inline-flex items-center gap-1"><SvgoPin class="text-sm" /> {{ data.field(m.fieldId)?.name }}</span>
            </div>

            <div class="flex items-center justify-between gap-3 sm:flex-col sm:items-end">
              <span class="chip bg-amber-50 text-amber-700"><Countdown :until="m.expiresAt" /></span>
              <span class="text-xs text-ink-400">{{ relative(m.createdAt) }}</span>
            </div>
          </NuxtLink>
        </TransitionGroup>

        <UiEmptyState
          v-else
          :title="tab === 'received' ? $t('invitations.noneReceived') : $t('invitations.noneSent')"
          :text="tab === 'received' ? $t('invitations.noneReceivedText') : $t('invitations.noneSentText')"
        >
          <template #icon><SvgoInbox class="text-2xl" /></template>
          <UiButton v-if="tab === 'sent'" to="/matches/new" size="sm">{{ $t('matches.book') }}</UiButton>
        </UiEmptyState>

        <p class="mt-8 flex items-start gap-2 text-sm text-ink-400">
          <SvgoInfo class="mt-0.5 text-base" /> {{ $t('invitations.note') }}
        </p>
      </template>

      <!-- Join requests to my team (captain) -->
      <template v-else-if="tab === 'joinRequests'">
        <div v-if="joinRequests.length" class="card divide-y divide-ink-100">
          <JoinRequestRow v-for="r in joinRequests" :key="r.userId" :request="r" />
        </div>
        <UiEmptyState v-else :title="$t('teams.requestsEmpty')" :text="$t('teams.requestsEmptyText')">
          <template #icon><SvgoUsers class="text-2xl" /></template>
        </UiEmptyState>
      </template>

      <!-- My own join requests (player without a team) -->
      <template v-else-if="tab === 'myRequests'">
        <h2 class="section-title mb-1">{{ $t('invitations.myRequests') }}</h2>
        <p class="mb-5 text-sm text-ink-500">{{ $t('invitations.myRequestsText') }}</p>
        <div v-if="myRequests.length" class="card divide-y divide-ink-100">
          <div v-for="r in myRequests" :key="r.teamId" class="flex items-center gap-3 px-5 py-4">
            <TeamBadge :team="data.team(r.teamId)!" size="sm" />
            <div class="min-w-0 flex-1">
              <NuxtLink :to="`/teams/${r.teamId}`" class="truncate font-medium text-ink-800 hover:text-brand-600">{{ data.team(r.teamId)?.name }}</NuxtLink>
              <p class="text-xs text-ink-400">{{ $t('teams.requestedAgo', { time: relative(r.createdAt) }) }}</p>
            </div>
            <span class="chip hidden bg-amber-50 text-amber-700 sm:inline-flex"><SvgoClock class="text-xs" /> {{ $t('teams.requested') }}</span>
            <UiButton variant="ghost" size="sm" @click="cancelMyRequest(r.teamId)">{{ $t('common.cancel') }}</UiButton>
          </div>
        </div>
        <UiEmptyState v-else :title="$t('invitations.noMyRequests')" :text="$t('invitations.noMyRequestsText')">
          <template #icon><SvgoSend class="text-2xl" /></template>
          <div class="flex flex-wrap justify-center gap-2">
            <UiButton to="/teams" variant="secondary" size="sm">{{ $t('teams.findTeam') }}</UiButton>
            <UiButton to="/teams/new" size="sm">{{ $t('teams.create') }}</UiButton>
          </div>
        </UiEmptyState>
      </template>
    </template>
  </div>
</template>
