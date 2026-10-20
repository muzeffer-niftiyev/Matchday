<script setup lang="ts">
import { onKeyStroke, useScrollLock } from '@vueuse/core'

const open = defineModel<boolean>({ required: true })

withDefaults(defineProps<{
  title?: string
  description?: string
  size?: 'sm' | 'md' | 'lg'
}>(), { size: 'md' })

const scrollLocked = useScrollLock(typeof document !== 'undefined' ? document.body : null)
watch(open, value => (scrollLocked.value = value))

onKeyStroke('Escape', () => {
  if (open.value) open.value = false
})
</script>

<template>
  <Teleport to="body">
    <Transition name="fade">
      <div v-if="open" class="fixed inset-0 z-50 bg-ink-900/40 backdrop-blur-sm" @click="open = false" />
    </Transition>
    <Transition name="pop">
      <div v-if="open" class="pointer-events-none fixed inset-0 z-50 flex items-end justify-center p-0 sm:items-center sm:p-4">
        <div
          class="pointer-events-auto flex max-h-[92vh] w-full flex-col overflow-hidden rounded-t-3xl bg-white shadow-lift sm:rounded-3xl"
          :class="{ sm: 'sm:max-w-md', md: 'sm:max-w-lg', lg: 'sm:max-w-2xl' }[size]"
          role="dialog"
          aria-modal="true"
        >
          <div class="flex items-start justify-between gap-4 px-6 pb-2 pt-6">
            <div>
              <h2 v-if="title" class="text-lg font-semibold text-ink-900">{{ title }}</h2>
              <p v-if="description" class="mt-1 text-sm text-ink-500">{{ description }}</p>
            </div>
            <button class="-mr-2 rounded-xl p-2 text-ink-400 transition hover:bg-ink-100 hover:text-ink-700" :aria-label="$t('common.close')" @click="open = false">
              <SvgoX class="text-xl" />
            </button>
          </div>
          <div class="overflow-y-auto px-6 py-4">
            <slot />
          </div>
          <div v-if="$slots.footer" class="flex flex-col-reverse gap-2 border-t border-ink-100 bg-ink-50/60 px-6 py-4 sm:flex-row sm:justify-end">
            <slot name="footer" />
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>
