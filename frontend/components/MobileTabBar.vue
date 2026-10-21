<script setup lang="ts">
const route = useRoute()
const { attentionCount } = useInvitations()
const auth = useAuthStore()

const tabs = computed(() => [
  { to: '/', label: 'nav.home', icon: 'home', exact: true },
  { to: '/matches', label: 'nav.matches', icon: 'ball' },
  { to: '/teams', label: 'nav.teams', icon: 'users' },
  { to: '/fields', label: 'nav.fields', icon: 'field' },
  auth.isLoggedIn
    ? { to: '/invitations', label: 'nav.invitationsShort', icon: 'bell', badge: attentionCount.value }
    : { to: '/login', label: 'nav.login', icon: 'user' },
])

const isActive = (to: string, exact?: boolean) => (exact ? route.path === to : route.path.startsWith(to))
</script>

<template>
  <nav class="fixed inset-x-0 bottom-0 z-40 border-t border-ink-100 bg-white/90 pb-[env(safe-area-inset-bottom)] backdrop-blur-lg md:hidden">
    <div class="grid grid-cols-5">
      <NuxtLink
        v-for="tab in tabs"
        :key="tab.to"
        :to="tab.to"
        class="relative flex flex-col items-center gap-1 py-2.5 text-[11px] font-medium transition"
        :class="isActive(tab.to, tab.exact) ? 'text-brand-600' : 'text-ink-400'"
      >
        <SvgoHome v-if="tab.icon === 'home'" class="text-xl" />
        <SvgoBall v-else-if="tab.icon === 'ball'" class="text-xl" />
        <SvgoUsers v-else-if="tab.icon === 'users'" class="text-xl" />
        <SvgoField v-else-if="tab.icon === 'field'" class="text-xl" />
        <SvgoBell v-else-if="tab.icon === 'bell'" class="text-xl" />
        <SvgoUser v-else class="text-xl" />
        {{ $t(tab.label) }}
        <span v-if="'badge' in tab && tab.badge" class="absolute left-1/2 top-1.5 ml-2 flex h-4 min-w-4 items-center justify-center rounded-full bg-rose-500 px-1 text-[10px] font-bold text-white">{{ tab.badge }}</span>
      </NuxtLink>
    </div>
  </nav>
</template>
