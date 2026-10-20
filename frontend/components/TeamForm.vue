<script setup lang="ts">
import useVuelidate from '@vuelidate/core'
import { helpers, maxLength, required } from '@vuelidate/validators'
import { ChromePicker } from 'vue-color'
import { CITIES, TEAM_COLORS } from '~/utils/rules'

const props = defineProps<{
  initial?: { name: string; city: string; color: string }
  submitLabel: string
  loading?: boolean
}>()

const emit = defineEmits<{ submit: [value: { name: string; city: string; color: string }] }>()

const { t } = useI18n()

const form = reactive({
  name: props.initial?.name ?? '',
  city: props.initial?.city ?? 'Baku',
  color: props.initial?.color ?? TEAM_COLORS[0],
})

const customOpen = ref(false)

// Same limits as CreateTeamDto on the backend
const rules = computed(() => ({
  name: {
    required: helpers.withMessage(t('validation.required'), required),
    maxLength: helpers.withMessage(t('validation.maxLength', { n: 100 }), maxLength(100)),
  },
  city: {
    required: helpers.withMessage(t('validation.required'), required),
  },
}))

const v$ = useVuelidate(rules, form)

const preview = computed(() => ({ name: form.name || t('teams.form.previewName'), color: form.color }))

async function submit() {
  if (!(await v$.value.$validate())) return
  emit('submit', { ...form })
}
</script>

<template>
  <form class="space-y-6" novalidate @submit.prevent="submit">
    <div class="flex items-center gap-4 rounded-2xl bg-ink-50 p-4">
      <TeamBadge :team="preview" size="lg" />
      <div class="min-w-0">
        <p class="truncate font-semibold text-ink-900">{{ preview.name }}</p>
        <p class="text-sm text-ink-500">{{ form.city }}</p>
      </div>
    </div>

    <UiFormField :label="$t('teams.form.name')" :errors="v$.name.$errors">
      <input v-model="form.name" class="input" :class="{ 'input-error': v$.name.$error }" :placeholder="$t('teams.form.namePlaceholder')" @blur="v$.name.$touch()">
    </UiFormField>

    <UiFormField :label="$t('teams.form.city')" :errors="v$.city.$errors">
      <select v-model="form.city" class="input">
        <option v-for="c in CITIES" :key="c" :value="c">{{ c }}</option>
      </select>
    </UiFormField>

    <div>
      <label class="label">{{ $t('teams.form.color') }}</label>
      <div class="flex flex-wrap items-center gap-2">
        <button
          v-for="c in TEAM_COLORS"
          :key="c"
          type="button"
          class="h-9 w-9 rounded-xl transition hover:scale-105"
          :class="form.color === c ? 'ring-4 ring-offset-2' : ''"
          :style="{ background: c, '--tw-ring-color': `${c}55` }"
          @click="form.color = c"
        />
        <button
          type="button"
          class="flex h-9 items-center gap-1.5 rounded-xl border border-dashed border-ink-300 px-3 text-xs font-medium text-ink-500 transition hover:border-ink-400 hover:text-ink-700"
          @click="customOpen = !customOpen"
        >
          <SvgoPlus class="text-sm" /> {{ $t('teams.form.custom') }}
        </button>
      </div>
      <Transition name="pop">
        <div v-if="customOpen" class="mt-3">
          <ChromePicker v-model="form.color" disable-alpha :formats="['hex']" />
        </div>
      </Transition>
    </div>

    <UiButton type="submit" block size="lg" :loading="loading">{{ submitLabel }}</UiButton>
  </form>
</template>
