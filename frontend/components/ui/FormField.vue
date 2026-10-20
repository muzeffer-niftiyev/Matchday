<script setup lang="ts">
// Label + slot + validation error from Vuelidate.
defineProps<{
  label?: string
  errors?: { $message: string | unknown }[]
  hint?: string
}>()
</script>

<template>
  <div>
    <label v-if="label" class="label">{{ label }}</label>
    <slot />
    <Transition name="fade">
      <p v-if="errors?.length" class="mt-1.5 text-xs font-medium text-rose-500">{{ errors[0].$message }}</p>
      <p v-else-if="hint" class="mt-1.5 text-xs text-ink-400">{{ hint }}</p>
    </Transition>
  </div>
</template>
