<script setup lang="ts">
definePageMeta({ middleware: 'auth' })

const { t } = useI18n()
useHead({ title: () => t('teams.create') })

const data = useDataStore()
const auth = useAuthStore()
const notify = useNotify()
const router = useRouter()
const loading = ref(false)

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
    <div class="card p-6 sm:p-8">
      <TeamForm :submit-label="$t('teams.create')" :loading="loading" @submit="create" />
    </div>
  </div>
</template>
