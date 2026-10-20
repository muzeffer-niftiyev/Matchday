<script setup lang="ts">
import type { Team } from '~/types'

const props = withDefaults(defineProps<{
  team: Pick<Team, 'name' | 'color'>
  size?: 'sm' | 'md' | 'lg' | 'xl'
}>(), { size: 'md' })

const initials = computed(() => props.team.name.split(' ').map(w => w[0]).slice(0, 2).join('').toUpperCase())
</script>

<template>
  <span
    class="inline-flex shrink-0 items-center justify-center font-bold text-white shadow-sm"
    :class="{
      sm: 'h-8 w-8 rounded-lg text-[11px]',
      md: 'h-11 w-11 rounded-xl text-sm',
      lg: 'h-16 w-16 rounded-2xl text-lg',
      xl: 'h-20 w-20 rounded-3xl text-2xl',
    }[size]"
    :style="{ background: `linear-gradient(135deg, ${team.color}, ${team.color}cc)` }"
  >{{ initials }}</span>
</template>
