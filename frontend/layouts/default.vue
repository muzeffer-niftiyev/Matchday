<template>
  <div class="flex min-h-screen flex-col">
    <AppHeader />
    <main class="flex-1 pb-24 pt-8 md:pb-0 sm:pt-10">
      <div :key="$route.path" class="animate-step-in">
        <slot />
      </div>
    </main>
    <AppFooter />
    <MobileTabBar />
  </div>
</template>
