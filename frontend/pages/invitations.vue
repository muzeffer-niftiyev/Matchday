<script setup lang="ts">
definePageMeta({ middleware: 'auth' })

const data = useDataStore()
const { received, sent } = useInvitations()
const { niceDate, timeRange, relative } = useFormat()
const { t } = useI18n()

useHead({ title: () => t('nav.invitations') })

const tab = ref('received')
const tabs = computed(() => [
  { value: 'received', label: t('invitations.received'), count: received.value.length },
  { value: 'sent', label: t('invitations.sent'), count: sent.value.length },
])

const list = computed(() => (tab.value === 'received' ? received.value : sent.value))
</script>

<template>
  <div class="container-page max-w-3xl">
    <PageHeader :title="$t('nav.invitations')" :subtitle="$t('invitations.subtitle')" />

    <UiTabs v-model="tab" :tabs="tabs" class="mb-6" />

    <TransitionGroup v-if="list.length" tag="div" name="fade" class="space-y-3">
      <NuxtLink
        v-for="m in list"
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
      <UiButton v-if="tab === 'sent'" to="/fields" size="sm">{{ $t('matches.book') }}</UiButton>
    </UiEmptyState>

    <p class="mt-8 flex items-start gap-2 text-sm text-ink-400">
      <SvgoInfo class="mt-0.5 text-base" /> {{ $t('invitations.note') }}
    </p>
  </div>
</template>
