<template>
  <footer class="mt-20 border-t border-ink-100 bg-white">
    <div class="container-page flex flex-col items-center justify-between gap-3 py-8 text-sm text-ink-400 sm:flex-row">
      <div class="flex items-center gap-2">
        <span class="flex h-7 w-7 items-center justify-center rounded-lg bg-brand-500 text-white">
          <SvgoBall class="text-base" />
        </span>
        <span class="font-semibold text-ink-700">Matchday</span>
        <span>· {{ $t('footer.tagline') }}</span>
      </div>
      <p>{{ $t('footer.demo') }}</p>
    </div>
  </footer>
</template>
