<script setup lang="ts">
import useVuelidate from '@vuelidate/core'
import { email, helpers, required } from '@vuelidate/validators'

definePageMeta({ layout: 'auth' })

const { t } = useI18n()
useHead({ title: () => t('auth.loginTitle') })

const auth = useAuthStore()
const notify = useNotify()
const route = useRoute()
const router = useRouter()

const form = reactive({ email: '', password: '' })
const loading = ref(false)

const rules = computed(() => ({
  email: {
    required: helpers.withMessage(t('validation.required'), required),
    email: helpers.withMessage(t('validation.email'), email),
  },
  password: { required: helpers.withMessage(t('validation.required'), required) },
}))

const v$ = useVuelidate(rules, form)

function goNext() {
  const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/'
  router.push(redirect)
}

async function submit() {
  if (!(await v$.value.$validate())) return

  loading.value = true
  try {
    await auth.login(form.email, form.password)   // POST /auth/login, then GET /auth/me
    notify.success('auth.welcomeBack', { name: auth.user?.displayName ?? '' })
    goNext()
  }
  catch (error) {
    // 401 has no body: the backend never says WHICH part was wrong (no user enumeration!)
    const status = (error as { statusCode?: number }).statusCode
    notify.errorText(status === 401 ? t('auth.wrongCredentials') : apiErrorMessage(error, t('errors.network')))
  }
  finally {
    loading.value = false
  }
}
</script>

<template>
  <div>
    <h1 class="text-2xl font-bold text-ink-900">{{ $t('auth.loginTitle') }}</h1>
    <p class="mt-1.5 text-ink-500">{{ $t('auth.loginSubtitle') }}</p>

    <form class="mt-8 space-y-4" novalidate @submit.prevent="submit">
      <UiFormField :label="$t('auth.email')" :errors="v$.email.$errors">
        <input v-model="form.email" type="email" class="input" :class="{ 'input-error': v$.email.$error }" placeholder="ad@numune.az" autocomplete="email" @blur="v$.email.$touch()">
      </UiFormField>
      <UiFormField :label="$t('auth.password')" :errors="v$.password.$errors">
        <input v-model="form.password" type="password" class="input" :class="{ 'input-error': v$.password.$error }" placeholder="••••••••" autocomplete="current-password" @blur="v$.password.$touch()">
      </UiFormField>
      <UiButton type="submit" block size="lg" :loading="loading">{{ $t('auth.loginButton') }}</UiButton>
    </form>

    <p class="mt-8 text-center text-sm text-ink-500">
      {{ $t('auth.noAccount') }}
      <NuxtLink to="/register" class="font-semibold text-brand-600 hover:text-brand-700">{{ $t('nav.register') }}</NuxtLink>
    </p>
  </div>
</template>
