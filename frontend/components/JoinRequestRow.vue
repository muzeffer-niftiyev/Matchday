<script setup lang="ts">
import type { JoinRequest } from '~/types'

// One join request, as the team owner sees it: who asked, when, and Accept / Decline.
const props = defineProps<{ request: JoinRequest }>()

const data = useDataStore()
const auth = useAuthStore()
const notify = useNotify()
const { relative } = useFormat()

const player = computed(() => data.user(props.request.userId))

function accept() {
  notify.result(data.acceptRequest(props.request.teamId, auth.userId!, props.request.userId), 'teams.requestAccepted', { name: player.value?.displayName })
}

function decline() {
  notify.result(data.declineRequest(props.request.teamId, auth.userId!, props.request.userId), 'teams.requestDeclined', { name: player.value?.displayName })
}
</script>

<template>
  <div v-if="player" class="flex flex-col gap-3 px-5 py-4 sm:flex-row sm:items-center">
    <div class="flex min-w-0 flex-1 items-center gap-3">
      <UiAvatar :name="player.displayName" />
      <div class="min-w-0">
        <p class="truncate font-medium text-ink-800">{{ player.displayName }}</p>
        <p class="text-xs text-ink-400">{{ $t('teams.requestedAgo', { time: relative(request.createdAt) }) }}</p>
      </div>
    </div>
    <div class="flex gap-2">
      <UiButton variant="secondary" size="sm" class="flex-1 sm:flex-none" @click="decline">{{ $t('teams.decline') }}</UiButton>
      <UiButton size="sm" class="flex-1 sm:flex-none" @click="accept"><SvgoCheck class="text-base" /> {{ $t('teams.accept') }}</UiButton>
    </div>
  </div>
</template>
