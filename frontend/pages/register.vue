<script setup lang="ts">
import useVuelidate from '@vuelidate/core'
import { email, helpers, maxLength, minLength, required, sameAs } from '@vuelidate/validators'

definePageMeta({ layout: 'auth' })

const { t } = useI18n()
useHead({ title: () => t('auth.registerTitle') })

const auth = useAuthStore()
const notify = useNotify()
const router = useRouter()

const form = reactive({ displayName: '', email: '', password: '', confirm: '' })
const loading = ref(false)

// The same limits as RegisterDto on the backend
const rules = computed(() => ({
  displayName: {
    required: helpers.withMessage(t('validation.required'), required),
    maxLength: helpers.withMessage(t('validation.maxLength', { n: 50 }), maxLength(50)),
  },
  email: {
    required: helpers.withMessage(t('validation.required'), required),
    email: helpers.withMessage(t('validation.email'), email),
  },
  password: {
    required: helpers.withMessage(t('validation.required'), required),
    minLength: helpers.withMessage(t('validation.minLength', { n: 8 }), minLength(8)),
  },
  confirm: {
    sameAs: helpers.withMessage(t('validation.passwordsMatch'), sameAs(computed(() => form.password))),
  },
}))

const v$ = useVuelidate(rules, form)

async function submit() {
  if (!(await v$.value.$validate())) return

  loading.value = true
  try {
    await auth.register(form.displayName, form.email, form.password)   // POST /auth/register, then log in
    notify.success('auth.registered')
    router.push('/')
  }
  catch (error) {
    // e.g. 409 "Bu e-poçt artıq qeydiyyatdan keçib." or a validation message from the backend
    notify.errorText(apiErrorMessage(error, t('errors.network')))
  }
  finally {
    loading.value = false
  }
}
</script>

<template>
  <div>
    <h1 class="text-2xl font-bold text-ink-900">{{ $t('auth.registerTitle') }}</h1>
    <p class="mt-1.5 text-ink-500">{{ $t('auth.registerSubtitle') }}</p>

    <form class="mt-8 space-y-4" novalidate @submit.prevent="submit">
      <UiFormField :label="$t('auth.displayName')" :errors="v$.displayName.$errors">
        <input v-model="form.displayName" class="input" :class="{ 'input-error': v$.displayName.$error }" placeholder="Əli Məmmədov" autocomplete="name" @blur="v$.displayName.$touch()">
      </UiFormField>
      <UiFormField :label="$t('auth.email')" :errors="v$.email.$errors">
        <input v-model="form.email" type="email" class="input" :class="{ 'input-error': v$.email.$error }" placeholder="ad@numune.az" autocomplete="email" @blur="v$.email.$touch()">
      </UiFormField>
      <UiFormField :label="$t('auth.password')" :errors="v$.password.$errors" :hint="$t('validation.minLength', { n: 8 })">
        <input v-model="form.password" type="password" class="input" :class="{ 'input-error': v$.password.$error }" placeholder="••••••••" autocomplete="new-password" @blur="v$.password.$touch()">
      </UiFormField>
      <UiFormField :label="$t('auth.confirmPassword')" :errors="v$.confirm.$errors">
        <input v-model="form.confirm" type="password" class="input" :class="{ 'input-error': v$.confirm.$error }" placeholder="••••••••" autocomplete="new-password" @blur="v$.confirm.$touch()">
      </UiFormField>
      <UiButton type="submit" block size="lg" :loading="loading">{{ $t('auth.registerButton') }}</UiButton>
    </form>

    <p class="mt-8 text-center text-sm text-ink-500">
      {{ $t('auth.haveAccount') }}
      <NuxtLink to="/login" class="font-semibold text-brand-600 hover:text-brand-700">{{ $t('nav.login') }}</NuxtLink>
    </p>
  </div>
</template>
