<script setup lang="ts">
import type { Field } from '~/types'

defineProps<{ field: Field }>()

const { money } = useFormat()
</script>

<template>
  <NuxtLink :to="`/fields/${field.id}`" class="card card-hover group block overflow-hidden">
    <div class="relative h-36 bg-gradient-to-br" :class="field.gradient">
      <!-- A simple pitch drawing instead of a photo -->
      <svg class="absolute inset-0 h-full w-full text-white/35" viewBox="0 0 200 100" preserveAspectRatio="none" fill="none" stroke="currentColor" stroke-width="1.2">
        <rect x="8" y="8" width="184" height="84" rx="3" />
        <line x1="100" y1="8" x2="100" y2="92" />
        <circle cx="100" cy="50" r="13" />
        <rect x="8" y="30" width="22" height="40" />
        <rect x="170" y="30" width="22" height="40" />
      </svg>
      <span class="chip absolute left-3 top-3 bg-white/90 text-ink-700 backdrop-blur">
        <SvgoLeaf class="text-sm" /> {{ $t(`grass.${field.grassType}`) }}
      </span>
      <span class="chip absolute right-3 top-3 bg-ink-900/70 text-white backdrop-blur">8 × 8</span>
    </div>
    <div class="p-5">
      <div class="flex items-start justify-between gap-3">
        <div class="min-w-0">
          <h3 class="truncate font-semibold text-ink-900 transition group-hover:text-brand-600">{{ field.name }}</h3>
          <p class="mt-1 flex items-center gap-1 truncate text-sm text-ink-500">
            <SvgoPin class="text-sm" /> {{ field.city }} · {{ field.address }}
          </p>
        </div>
        <div class="shrink-0 text-right">
          <p class="font-bold text-ink-900">{{ money(field.pricePerHour) }}</p>
          <p class="text-xs text-ink-400">{{ $t('fields.perHour') }}</p>
        </div>
      </div>
    </div>
  </NuxtLink>
</template>
