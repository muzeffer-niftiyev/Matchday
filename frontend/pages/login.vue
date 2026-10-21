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
  await new Promise(r => setTimeout(r, 500)) // pretend to call the API
  auth.loginWithEmail(form.email)
  loading.value = false
  notify.success('auth.welcomeBack', { name: auth.user?.displayName ?? '' })
  goNext()
}

function demo(userId: number) {
  auth.loginAs(userId)
  notify.success('auth.welcomeBack', { name: auth.user?.displayName ?? '' })
  goNext()
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

    <div class="my-6 flex items-center gap-3 text-xs text-ink-400">
      <span class="h-px flex-1 bg-ink-200" /> {{ $t('auth.orDemo') }} <span class="h-px flex-1 bg-ink-200" />
    </div>

    <div class="grid grid-cols-2 gap-2">
      <UiButton variant="secondary" @click="demo(1)"><SvgoUser class="text-base" /> {{ $t('auth.demoPlayer') }}</UiButton>
      <UiButton variant="secondary" @click="demo(100)"><SvgoShield class="text-base" /> {{ $t('auth.demoAdmin') }}</UiButton>
    </div>

    <p class="mt-8 text-center text-sm text-ink-500">
      {{ $t('auth.noAccount') }}
      <NuxtLink to="/register" class="font-semibold text-brand-600 hover:text-brand-700">{{ $t('nav.register') }}</NuxtLink>
    </p>
  </div>
</template>
