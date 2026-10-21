<script setup lang="ts">
import { onClickOutside } from '@vueuse/core'

const auth = useAuthStore()
const notify = useNotify()
const router = useRouter()
const { attentionCount } = useInvitations()

const menuOpen = ref(false)
const menuRef = ref<HTMLElement | null>(null)
onClickOutside(menuRef, () => (menuOpen.value = false))

const links = computed(() => [
  { to: '/', label: 'nav.home', exact: true },
  { to: '/matches', label: 'nav.matches' },
  { to: '/teams', label: 'nav.teams' },
  { to: '/fields', label: 'nav.fields' },
])

const route = useRoute()
const isActive = (to: string, exact?: boolean) => (exact ? route.path === to : route.path.startsWith(to))

function logout() {
  menuOpen.value = false
  auth.logout()
  notify.info('auth.loggedOut')
  router.push('/')
}
</script>

<template>
  <header class="sticky top-0 z-40 border-b border-ink-100/80 bg-white/80 backdrop-blur-lg">
    <div class="container-page flex h-16 items-center justify-between gap-4">
      <NuxtLink to="/" class="flex items-center gap-2.5">
        <span class="flex h-9 w-9 items-center justify-center rounded-xl bg-brand-500 text-white shadow-sm shadow-brand-500/40">
          <SvgoBall class="text-xl" />
        </span>
        <span class="text-lg font-bold tracking-tight text-ink-900">Matchday</span>
      </NuxtLink>

      <nav class="hidden items-center gap-1 md:flex">
        <NuxtLink
          v-for="link in links"
          :key="link.to"
          :to="link.to"
          class="whitespace-nowrap rounded-xl px-3 py-2 text-sm font-medium transition"
          :class="isActive(link.to, link.exact) ? 'bg-brand-50 text-brand-700' : 'text-ink-500 hover:bg-ink-100 hover:text-ink-900'"
        >{{ $t(link.label) }}</NuxtLink>
      </nav>

      <div class="flex items-center gap-1">

        <template v-if="auth.user">
          <NuxtLink
            to="/invitations"
            class="relative flex h-10 w-10 items-center justify-center rounded-xl text-ink-500 transition hover:bg-ink-100 hover:text-ink-900"
            :aria-label="$t('nav.invitations')"
          >
            <SvgoBell class="text-xl" />
            <span v-if="attentionCount" class="absolute right-1.5 top-1.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-rose-500 px-1 text-[10px] font-bold text-white ring-2 ring-white">
              {{ attentionCount }}
            </span>
          </NuxtLink>

          <div ref="menuRef" class="relative">
            <button class="flex items-center gap-2 rounded-xl p-1 pr-2 transition hover:bg-ink-100" @click="menuOpen = !menuOpen">
              <UiAvatar :name="auth.user.displayName" size="sm" />
              <SvgoChevronDown class="hidden text-sm text-ink-400 sm:block" />
            </button>
            <Transition name="pop">
              <div v-if="menuOpen" class="absolute right-0 mt-2 w-60 overflow-hidden rounded-2xl border border-ink-100 bg-white p-1.5 shadow-lift">
                <div class="px-3 py-2.5">
                  <p class="truncate text-sm font-semibold text-ink-900">{{ auth.user.displayName }}</p>
                  <p class="truncate text-xs text-ink-500">{{ auth.user.email }}</p>
                </div>
                <div class="my-1 h-px bg-ink-100" />
                <NuxtLink to="/profile" class="menu-item" @click="menuOpen = false"><SvgoUser /> {{ $t('nav.profile') }}</NuxtLink>
                <NuxtLink to="/invitations" class="menu-item" @click="menuOpen = false"><SvgoInbox /> {{ $t('nav.invitations') }}</NuxtLink>
                <NuxtLink v-if="auth.isAdmin" to="/admin" class="menu-item" @click="menuOpen = false"><SvgoShield /> {{ $t('nav.admin') }}</NuxtLink>
                <div class="my-1 h-px bg-ink-100" />
                <button class="menu-item w-full text-rose-600 hover:bg-rose-50" @click="logout"><SvgoLogout /> {{ $t('nav.logout') }}</button>
              </div>
            </Transition>
          </div>
        </template>

        <template v-else>
          <UiButton to="/login" variant="ghost" size="sm" class="hidden sm:inline-flex">{{ $t('nav.login') }}</UiButton>
          <UiButton to="/register" size="sm">{{ $t('nav.register') }}</UiButton>
        </template>
      </div>
    </div>
  </header>
</template>

<style scoped>
.menu-item {
  @apply flex items-center gap-2.5 rounded-xl px-3 py-2 text-sm font-medium text-ink-700 transition hover:bg-ink-50;
}

.menu-item :deep(.nuxt-icon) {
  @apply text-base text-ink-400;
}
</style>
