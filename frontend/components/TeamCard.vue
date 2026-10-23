<script setup lang="ts">
import type { Team } from '~/types'

import { MAX_SQUAD_SIZE } from '~/utils/rules'

const props = defineProps<{ team: Team }>()

const auth = useAuthStore()
const inbox = useInboxStore()

// memberCount and ownerName come from the API (TeamDto), no lookups needed.
// isMine: GET /me/team (auth store), requested: GET /me/join-requests (inbox store)
const isMine = computed(() => auth.team?.id === props.team.id)
const isOwner = computed(() => props.team.ownerId === auth.userId)
const requested = computed(() => inbox.myRequests.some(r => r.teamId === props.team.id))
</script>

<template>
  <NuxtLink :to="`/teams/${team.id}`" class="card card-hover group flex items-center gap-4 p-5">
    <TeamBadge :team="team" size="md" class="sm:hidden" />
    <TeamBadge :team="team" size="lg" class="hidden sm:inline-flex" />
    <div class="min-w-0 flex-1">
      <div class="flex items-center gap-2">
        <h3 class="truncate font-semibold text-ink-900 transition group-hover:text-brand-600">{{ team.name }}</h3>
        <span v-if="isOwner" class="chip bg-amber-50 text-amber-700"><SvgoCrown class="text-xs" /> {{ $t('teams.owner') }}</span>
        <span v-else-if="isMine" class="chip bg-brand-50 text-brand-700">{{ $t('teams.member') }}</span>
        <span v-else-if="requested" class="chip bg-amber-50 text-amber-700"><SvgoClock class="text-xs" /> {{ $t('teams.requested') }}</span>
      </div>
      <p class="mt-1 flex items-center gap-1 text-sm text-ink-500"><SvgoPin class="text-sm" /> {{ team.city }}</p>
      <div class="mt-3 flex items-center gap-3 text-xs text-ink-500">
        <span class="inline-flex items-center gap-1"><SvgoUsers class="text-sm" /> {{ $t('teams.squadSize', { n: team.memberCount, max: MAX_SQUAD_SIZE }) }}</span>
        <span v-if="team.ownerName" class="hidden truncate sm:inline">{{ $t('teams.captain') }}: {{ team.ownerName }}</span>
      </div>
    </div>
    <SvgoChevronRight class="text-lg text-ink-300 transition group-hover:translate-x-0.5 group-hover:text-ink-500" />
  </NuxtLink>
</template>
