<script setup lang="ts">
useHead({
  htmlAttrs: { lang: 'az' },
  titleTemplate: title => (title ? `${title} · Matchday` : 'Matchday'),
})
</script>

<template>
  <NuxtLayout>
    <NuxtPage />
  </NuxtLayout>
</template>
