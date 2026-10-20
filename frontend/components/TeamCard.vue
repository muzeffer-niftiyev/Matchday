<script setup lang="ts">
import type { Team } from '~/types'

const props = defineProps<{ team: Team }>()

const data = useDataStore()
const auth = useAuthStore()

const memberCount = computed(() => data.teamMembers(props.team.id).length)
const owner = computed(() => data.user(props.team.ownerId))
const isMine = computed(() => auth.userId !== null && data.isMember(props.team.id, auth.userId))
const isOwner = computed(() => props.team.ownerId === auth.userId)
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
      </div>
      <p class="mt-1 flex items-center gap-1 text-sm text-ink-500"><SvgoPin class="text-sm" /> {{ team.city }}</p>
      <div class="mt-3 flex items-center gap-3 text-xs text-ink-500">
        <span class="inline-flex items-center gap-1"><SvgoUsers class="text-sm" /> {{ $t('teams.playersCount', { n: memberCount }) }}</span>
        <span v-if="owner" class="hidden truncate sm:inline">{{ $t('teams.captain') }}: {{ owner.displayName }}</span>
      </div>
    </div>
    <SvgoChevronRight class="text-lg text-ink-300 transition group-hover:translate-x-0.5 group-hover:text-ink-500" />
  </NuxtLink>
</template>
