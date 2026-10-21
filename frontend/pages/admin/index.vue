<script setup lang="ts">
import useVuelidate from '@vuelidate/core'
import { between, helpers, maxLength, required } from '@vuelidate/validators'
import { matchEnd } from '~/stores/data'
import type { Field, GrassType } from '~/types'
import { CITIES } from '~/utils/rules'

definePageMeta({ middleware: 'admin' })

const data = useDataStore()
const notify = useNotify()
const { money, niceDate } = useFormat()
const { t } = useI18n()

useHead({ title: () => t('nav.admin') })

const tab = ref('scores')

// ---------- Scores ----------
const waitingForScore = computed(() =>
  data.matches
    .filter(m => m.status === 'Confirmed' && matchEnd(m) <= new Date())
    .sort((a, b) => matchEnd(a).getTime() - matchEnd(b).getTime()),
)
const recentCompleted = computed(() =>
  data.matches
    .filter(m => m.status === 'Completed')
    .sort((a, b) => matchEnd(b).getTime() - matchEnd(a).getTime())
    .slice(0, 6),
)

const tabs = computed(() => [
  { value: 'scores', label: t('admin.scores'), count: waitingForScore.value.length },
  { value: 'fields', label: t('admin.fields'), count: data.fields.length },
])

const scores = reactive<Record<number, { home: number | null; away: number | null }>>({})
watchEffect(() => {
  for (const m of [...waitingForScore.value, ...recentCompleted.value]) {
    scores[m.id] ??= { home: m.homeScore, away: m.awayScore }
  }
})

function saveScore(matchId: number) {
  const s = scores[matchId]
  if (s.home === null || s.away === null || s.home < 0 || s.away < 0) return notify.error('admin.scoreInvalid')
  notify.result(data.setScore(matchId, s.home, s.away), 'match.scoreSaved')
}

// ---------- Fields ----------
const fieldOpen = ref(false)
const deleteTarget = ref<Field | null>(null)
const deleteOpen = computed({ get: () => !!deleteTarget.value, set: v => !v && (deleteTarget.value = null) })

const form = reactive({
  id: undefined as number | undefined,
  name: '',
  city: 'Bakı',
  address: '',
  grassType: 'Artificial' as GrassType,
  pricePerHour: 60,
})

// Same limits as CreateFieldDto on the backend
const rules = computed(() => ({
  name: { required: helpers.withMessage(t('validation.required'), required), maxLength: helpers.withMessage(t('validation.maxLength', { n: 100 }), maxLength(100)) },
  city: { required: helpers.withMessage(t('validation.required'), required) },
  address: { required: helpers.withMessage(t('validation.required'), required) },
  pricePerHour: { between: helpers.withMessage(t('validation.between', { min: 1, max: 10000 }), between(1, 10000)) },
}))
const v$ = useVuelidate(rules, form)

function openField(field?: Field) {
  Object.assign(form, field
    ? { id: field.id, name: field.name, city: field.city, address: field.address, grassType: field.grassType, pricePerHour: field.pricePerHour }
    : { id: undefined, name: '', city: 'Bakı', address: '', grassType: 'Artificial', pricePerHour: 60 })
  v$.value.$reset()
  fieldOpen.value = true
}

async function saveField() {
  if (!(await v$.value.$validate())) return
  if (notify.result(data.saveField({ ...form }), form.id ? 'admin.fieldUpdated' : 'admin.fieldCreated', { name: form.name })) {
    fieldOpen.value = false
  }
}

function deleteField() {
  if (!deleteTarget.value) return
  const name = deleteTarget.value.name
  notify.result(data.deleteField(deleteTarget.value.id), 'admin.fieldDeleted', { name })
  deleteTarget.value = null
}
</script>

<template>
  <div class="container-page">
    <PageHeader :title="$t('nav.admin')" :subtitle="$t('admin.subtitle')">
      <template #actions>
        <UiButton v-if="tab === 'fields'" @click="openField()"><SvgoPlus class="text-lg" /> {{ $t('admin.addField') }}</UiButton>
      </template>
    </PageHeader>

    <UiTabs v-model="tab" :tabs="tabs" class="mb-6" />

    <!-- Scores -->
    <div v-if="tab === 'scores'" class="space-y-8">
      <section>
        <h2 class="section-title mb-1">{{ $t('admin.waiting') }}</h2>
        <p class="mb-4 text-sm text-ink-500">{{ $t('admin.waitingText') }}</p>

        <div v-if="waitingForScore.length" class="space-y-3">
          <div v-for="m in waitingForScore" :key="m.id" class="card flex flex-col gap-4 p-5 md:flex-row md:items-center">
            <div class="flex-1">
              <p class="mb-2 text-xs text-ink-400">{{ niceDate(m.date) }} · {{ m.startTime }} · {{ data.field(m.fieldId)?.name }}</p>
              <div class="flex items-center gap-2 text-sm font-semibold text-ink-800">
                <TeamBadge :team="data.team(m.homeTeamId)!" size="sm" /> {{ data.team(m.homeTeamId)?.name }}
                <span class="text-ink-300">vs</span>
                {{ data.team(m.awayTeamId)?.name }} <TeamBadge :team="data.team(m.awayTeamId)!" size="sm" />
              </div>
            </div>
            <div class="flex items-center gap-2">
              <input v-model.number="scores[m.id].home" type="number" min="0" max="99" class="input w-16 text-center font-bold" placeholder="0">
              <span class="font-bold text-ink-300">:</span>
              <input v-model.number="scores[m.id].away" type="number" min="0" max="99" class="input w-16 text-center font-bold" placeholder="0">
              <UiButton @click="saveScore(m.id)"><SvgoCheck class="text-base" /> {{ $t('common.save') }}</UiButton>
            </div>
          </div>
        </div>
        <UiEmptyState v-else :title="$t('admin.allDone')" :text="$t('admin.allDoneText')">
          <template #icon><SvgoCheck class="text-2xl" /></template>
        </UiEmptyState>
      </section>

      <section>
        <h2 class="section-title mb-4">{{ $t('admin.recent') }}</h2>
        <div class="grid grid-cols-1 gap-3 md:grid-cols-2">
          <MatchCard v-for="m in recentCompleted" :key="m.id" :match="m" />
        </div>
      </section>
    </div>

    <!-- Fields -->
    <div v-else class="card overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead class="bg-ink-50 text-left text-xs uppercase tracking-wide text-ink-500">
            <tr>
              <th class="px-5 py-3 font-semibold">{{ $t('admin.colField') }}</th>
              <th class="px-5 py-3 font-semibold">{{ $t('admin.colCity') }}</th>
              <th class="px-5 py-3 font-semibold">{{ $t('admin.colSurface') }}</th>
              <th class="px-5 py-3 font-semibold">{{ $t('admin.colPrice') }}</th>
              <th class="px-5 py-3" />
            </tr>
          </thead>
          <tbody class="divide-y divide-ink-100">
            <tr v-for="f in data.fields" :key="f.id" class="transition hover:bg-ink-50/60">
              <td class="px-5 py-3.5">
                <div class="flex items-center gap-3">
                  <span class="h-9 w-9 shrink-0 rounded-xl bg-gradient-to-br" :class="f.gradient" />
                  <div>
                    <p class="font-medium text-ink-900">{{ f.name }}</p>
                    <p class="text-xs text-ink-400">{{ f.address }}</p>
                  </div>
                </div>
              </td>
              <td class="px-5 py-3.5 text-ink-600">{{ f.city }}</td>
              <td class="px-5 py-3.5 text-ink-600">{{ $t(`grass.${f.grassType}`) }}</td>
              <td class="px-5 py-3.5 font-medium text-ink-900">{{ money(f.pricePerHour) }}</td>
              <td class="px-5 py-3.5">
                <div class="flex justify-end gap-1">
                  <button class="rounded-lg p-2 text-ink-400 transition hover:bg-ink-100 hover:text-ink-700" :title="$t('common.edit')" @click="openField(f)"><SvgoEdit class="text-base" /></button>
                  <button class="rounded-lg p-2 text-ink-400 transition hover:bg-rose-50 hover:text-rose-500" :title="$t('common.delete')" @click="deleteTarget = f"><SvgoTrash class="text-base" /></button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Field modal -->
    <UiModal v-model="fieldOpen" :title="form.id ? $t('admin.editField') : $t('admin.addField')">
      <form id="field-form" class="space-y-4" novalidate @submit.prevent="saveField">
        <UiFormField :label="$t('admin.colField')" :errors="v$.name.$errors">
          <input v-model="form.name" class="input" :class="{ 'input-error': v$.name.$error }" @blur="v$.name.$touch()">
        </UiFormField>
        <div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <UiFormField :label="$t('admin.colCity')" :errors="v$.city.$errors">
            <select v-model="form.city" class="input">
              <option v-for="c in CITIES" :key="c" :value="c">{{ c }}</option>
            </select>
          </UiFormField>
          <UiFormField :label="$t('admin.colSurface')">
            <select v-model="form.grassType" class="input">
              <option value="Artificial">{{ $t('grass.Artificial') }}</option>
              <option value="Natural">{{ $t('grass.Natural') }}</option>
            </select>
          </UiFormField>
        </div>
        <UiFormField :label="$t('admin.address')" :errors="v$.address.$errors">
          <input v-model="form.address" class="input" :class="{ 'input-error': v$.address.$error }" @blur="v$.address.$touch()">
        </UiFormField>
        <UiFormField :label="$t('admin.colPrice')" :errors="v$.pricePerHour.$errors">
          <div class="relative">
            <input v-model.number="form.pricePerHour" type="number" min="1" class="input pr-16" :class="{ 'input-error': v$.pricePerHour.$error }" @blur="v$.pricePerHour.$touch()">
            <span class="pointer-events-none absolute right-3.5 top-1/2 -translate-y-1/2 text-sm text-ink-400">AZN / {{ $t('common.h') }}</span>
          </div>
        </UiFormField>
      </form>
      <template #footer>
        <UiButton variant="secondary" @click="fieldOpen = false">{{ $t('common.cancel') }}</UiButton>
        <UiButton @click="saveField">{{ $t('common.save') }}</UiButton>
      </template>
    </UiModal>

    <UiModal v-model="deleteOpen" :title="$t('admin.deleteField')" :description="$t('admin.deleteFieldText', { name: deleteTarget?.name })" size="sm">
      <template #footer>
        <UiButton variant="secondary" @click="deleteTarget = null">{{ $t('common.cancel') }}</UiButton>
        <UiButton variant="danger" @click="deleteField">{{ $t('common.delete') }}</UiButton>
      </template>
    </UiModal>
  </div>
</template>
