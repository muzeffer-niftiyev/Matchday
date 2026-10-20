<script setup lang="ts">
import { CITIES } from '~/utils/rules'

const data = useDataStore()
const { t } = useI18n()

useHead({ title: () => t('nav.fields') })

const city = ref('')
const grass = ref('')
const sort = ref<'name' | 'price'>('name')

const fields = computed(() =>
  data.fields
    .filter(f => !city.value || f.city === city.value)
    .filter(f => !grass.value || f.grassType === grass.value)
    .sort((a, b) => (sort.value === 'price' ? a.pricePerHour - b.pricePerHour : a.name.localeCompare(b.name))),
)
</script>

<template>
  <div class="container-page">
    <PageHeader :title="$t('nav.fields')" :subtitle="$t('fields.subtitle')" />

    <div class="mb-6 flex flex-wrap gap-2">
      <select v-model="city" class="input w-auto">
        <option value="">{{ $t('common.allCities') }}</option>
        <option v-for="c in CITIES" :key="c" :value="c">{{ c }}</option>
      </select>
      <select v-model="grass" class="input w-auto">
        <option value="">{{ $t('fields.anyGrass') }}</option>
        <option value="Artificial">{{ $t('grass.Artificial') }}</option>
        <option value="Natural">{{ $t('grass.Natural') }}</option>
      </select>
      <select v-model="sort" class="input w-auto">
        <option value="name">{{ $t('fields.sortName') }}</option>
        <option value="price">{{ $t('fields.sortPrice') }}</option>
      </select>
    </div>

    <TransitionGroup v-if="fields.length" tag="div" name="fade" class="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
      <FieldCard v-for="f in fields" :key="f.id" :field="f" />
    </TransitionGroup>
    <UiEmptyState v-else :title="$t('fields.noneTitle')" :text="$t('fields.noneText')">
      <template #icon><SvgoField class="text-2xl" /></template>
    </UiEmptyState>
  </div>
</template>
