<template>
  <div class="grid min-h-screen lg:grid-cols-2">
    <div class="relative hidden overflow-hidden bg-gradient-to-br from-brand-500 via-emerald-600 to-teal-700 lg:block">
      <svg class="absolute inset-0 h-full w-full text-white/15" viewBox="0 0 400 600" preserveAspectRatio="xMidYMid slice" fill="none" stroke="currentColor" stroke-width="2">
        <rect x="30" y="30" width="340" height="540" rx="6" />
        <line x1="30" y1="300" x2="370" y2="300" />
        <circle cx="200" cy="300" r="60" />
        <rect x="120" y="30" width="160" height="80" />
        <rect x="120" y="490" width="160" height="80" />
      </svg>
      <div class="relative flex h-full flex-col p-12 text-white">
        <NuxtLink to="/" class="flex items-center gap-2.5">
          <span class="flex h-10 w-10 items-center justify-center rounded-xl bg-white/20 backdrop-blur">
            <SvgoBall class="text-2xl" />
          </span>
          <span class="text-xl font-bold">Matchday</span>
        </NuxtLink>
        <!-- my-auto: vertically centered between the logo and the bottom -->
        <div class="my-auto max-w-md">
          <h2 class="text-4xl font-bold leading-tight">{{ $t('auth.sideTitle') }}</h2>
          <p class="mt-4 text-lg text-white/85">{{ $t('auth.sideText') }}</p>
          <ul class="mt-8 space-y-4">
            <li v-for="n in 3" :key="n" class="flex items-start gap-3">
              <span class="mt-0.5 flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-white/20">
                <SvgoCheck class="text-sm" />
              </span>
              <span class="text-white/90">{{ $t(`auth.sidePoint${n}`) }}</span>
            </li>
          </ul>
        </div>
      </div>
    </div>

    <div class="flex flex-col">
      <div class="flex items-center justify-between p-4 sm:p-6">
        <NuxtLink to="/" class="flex items-center gap-2 lg:invisible">
          <span class="flex h-9 w-9 items-center justify-center rounded-xl bg-brand-500 text-white"><SvgoBall class="text-xl" /></span>
          <span class="text-lg font-bold">Matchday</span>
        </NuxtLink>
      </div>
      <div class="flex flex-1 items-center justify-center px-4 pb-16">
        <div class="w-full max-w-sm">
          <slot />
        </div>
      </div>
    </div>
  </div>
</template>
