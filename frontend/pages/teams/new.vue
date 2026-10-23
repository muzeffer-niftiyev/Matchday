<script setup lang="ts">
import type { MyJoinRequest, Team } from '~/types'

definePageMeta({ middleware: 'auth' })

const { t } = useI18n()
useHead({ title: () => t('teams.create') })

const api = useApi()
const auth = useAuthStore()
const notify = useNotify()
const router = useRouter()
const loading = ref(false)

// One team per player: you can only create a team if you're not in one.
const myTeam = computed(() => auth.team)

// Creating a team deletes my open join requests (the backend does it), so warn about them.
const { data: myRequests } = await useAsyncData('my-join-requests', () => api<MyJoinRequest[]>('/me/join-requests'))
const myRequestCount = computed(() => myRequests.value?.length ?? 0)

async function create(value: { name: string; city: string; color: string }) {
  loading.value = true
  try {
    // POST /teams → 201 + the new team (TeamDto)
    const team = await api<Team>('/teams', { method: 'POST', body: value })

    // I'm in a team now → refresh "my team"
    await auth.fetchMyTeam()

    notify.success('teams.created', { name: team.name })
    router.push(`/teams/${team.id}`)
  }
  catch (error) {
    // 409: name taken / already in a team, 400: validation → show the backend's message
    notify.errorText(apiErrorMessage(error, t('errors.network')))
  }
  finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="container-page max-w-xl">
    <PageHeader :title="$t('teams.create')" :subtitle="$t('teams.createSubtitle')" back="/teams" />

    <UiEmptyState v-if="myTeam" :title="$t('teams.alreadyInTeamTitle')" :text="$t('teams.alreadyInTeamText', { team: myTeam.name })">
      <template #icon><SvgoUsers class="text-2xl" /></template>
      <UiButton :to="`/teams/${myTeam.id}`" size="sm">{{ myTeam.name }}</UiButton>
    </UiEmptyState>

    <template v-else>
      <p v-if="myRequestCount" class="mb-4 flex items-start gap-2 rounded-xl bg-amber-50 px-4 py-3 text-sm text-amber-700">
        <SvgoInfo class="mt-0.5 text-base" /> {{ $t('teams.createCancelsRequests', { n: myRequestCount }) }}
      </p>
      <div class="card p-6 sm:p-8">
        <TeamForm :submit-label="$t('teams.create')" :loading="loading" @submit="create" />
      </div>
    </template>
  </div>
</template>
