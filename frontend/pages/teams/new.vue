<script setup lang="ts">
definePageMeta({ middleware: 'auth' })

const { t } = useI18n()
useHead({ title: () => t('teams.create') })

const data = useDataStore()
const auth = useAuthStore()
const notify = useNotify()
const router = useRouter()
const loading = ref(false)

// One team per player: you can only create a team if you're not in one.
const myTeam = computed(() => data.teamOfUser(auth.userId!))
const myRequestCount = computed(() => data.requestsOfUser(auth.userId!).length)

async function create(value: { name: string; city: string; color: string }) {
  loading.value = true
  await new Promise(r => setTimeout(r, 400))
  const result = data.createTeam(auth.userId!, value.name, value.city, value.color)
  loading.value = false

  if (notify.result(result, 'teams.created', { name: value.name })) {
    router.push(`/teams/${result.ok ? result.id : ''}`)
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
