<script setup lang="ts">
import type { JoinRequestDto } from '~/types'

// One join request, as the team owner sees it: who asked, when, and Accept / Decline.
const props = defineProps<{ teamId: number; request: JoinRequestDto }>()
const emit = defineEmits<{ handled: [] }>()

const api = useApi()
const inbox = useInboxStore()
const notify = useNotify()
const { relative } = useFormat()
const { t } = useI18n()
const loading = ref(false)

// POST /teams/{id}/join-requests/{userId}/accept or /decline
async function answer(action: 'accept' | 'decline') {
  loading.value = true
  try {
    await api(`/teams/${props.teamId}/join-requests/${props.request.userId}/${action}`, { method: 'POST' })
    notify.success(action === 'accept' ? 'teams.requestAccepted' : 'teams.requestDeclined', { name: props.request.displayName })
    // Emit first: refreshing the inbox removes this row, and an unmounted row's events are lost
    emit('handled')
    await inbox.refresh()
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
  <div class="flex flex-col gap-3 px-5 py-4 sm:flex-row sm:items-center">
    <div class="flex min-w-0 flex-1 items-center gap-3">
      <UiAvatar :name="request.displayName" />
      <div class="min-w-0">
        <p class="truncate font-medium text-ink-800">{{ request.displayName }}</p>
        <p class="text-xs text-ink-400">{{ $t('teams.requestedAgo', { time: relative(request.createdAt) }) }}</p>
      </div>
    </div>
    <div class="flex gap-2">
      <UiButton variant="secondary" size="sm" class="flex-1 sm:flex-none" :disabled="loading" @click="answer('decline')">{{ $t('teams.decline') }}</UiButton>
      <UiButton size="sm" class="flex-1 sm:flex-none" :loading="loading" @click="answer('accept')"><SvgoCheck class="text-base" /> {{ $t('teams.accept') }}</UiButton>
    </div>
  </div>
</template>
