<script setup lang="ts">
const { locale, locales, setLocale } = useI18n()

const other = computed(() => locales.value.find(l => l.code !== locale.value))
</script>

<template>
  <button
    v-if="other"
    class="inline-flex h-10 items-center gap-1.5 rounded-xl px-3 text-sm font-medium text-ink-600 transition hover:bg-ink-100 hover:text-ink-900"
    :title="other.name"
    @click="setLocale(other.code)"
  >
    <SvgoGlobe class="text-base" />
    <span class="uppercase">{{ other.code }}</span>
  </button>
</template>
