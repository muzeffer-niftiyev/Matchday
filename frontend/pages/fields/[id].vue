<script setup lang="ts">
import VueDatePicker from '@vuepic/vue-datepicker'
import { addDays, format, parseISO } from 'date-fns'
import { az, enUS } from 'date-fns/locale'

const route = useRoute()
const router = useRouter()
const data = useDataStore()
const auth = useAuthStore()
const { locale, t } = useI18n()
const { money, longDate } = useFormat()

const fieldId = computed(() => Number(route.params.id))
const field = computed(() => data.field(fieldId.value))

useHead({ title: () => field.value?.name ?? t('nav.fields') })

// Booking is possible from tomorrow on (same rule as the backend).
const tomorrow = addDays(new Date(), 1)
const selectedDate = ref<string>(typeof route.query.date === 'string' ? route.query.date : format(tomorrow, 'yyyy-MM-dd'))
const selectedTime = ref<string | null>(null)

const dfLocale = computed(() => (locale.value === 'az' ? az : enUS))
const days = computed(() =>
  Array.from({ length: 14 }, (_, i) => {
    const d = addDays(tomorrow, i)
    return {
      value: format(d, 'yyyy-MM-dd'),
      weekday: format(d, 'EEE', { locale: dfLocale.value }),
      day: format(d, 'd'),
      month: format(d, 'MMM', { locale: dfLocale.value }),
    }
  }),
)

const pickerDate = computed({
  get: () => parseISO(selectedDate.value),
  set: (d: Date | null) => {
    if (d) selectedDate.value = format(d, 'yyyy-MM-dd')
  },
})

watch(selectedDate, () => (selectedTime.value = null))

const slots = computed(() => data.slotsFor(fieldId.value, selectedDate.value))
const freeCount = computed(() => slots.value.filter(s => s.isFree).length)

function book() {
  const query = { fieldId: fieldId.value, date: selectedDate.value, time: selectedTime.value }
  if (!auth.isLoggedIn) {
    router.push({ path: '/login', query: { redirect: router.resolve({ path: '/matches/new', query }).fullPath } })
    return
  }
  router.push({ path: '/matches/new', query })
}
</script>

<template>
  <div class="container-page">
    <template v-if="field">
      <NuxtLink to="/fields" class="mb-4 inline-flex items-center gap-1 text-sm font-medium text-ink-500 transition hover:text-ink-800">
        <SvgoChevronLeft class="text-base" /> {{ $t('nav.fields') }}
      </NuxtLink>

      <div class="grid grid-cols-1 gap-6 lg:grid-cols-[1fr_380px]">
        <!-- Left: info + slots -->
        <div class="min-w-0 space-y-6">
          <section class="card overflow-hidden">
            <div class="relative h-48 bg-gradient-to-br sm:h-60" :class="field.gradient">
              <svg class="absolute inset-0 h-full w-full text-white/35" viewBox="0 0 200 100" preserveAspectRatio="none" fill="none" stroke="currentColor" stroke-width="1">
                <rect x="8" y="8" width="184" height="84" rx="3" />
                <line x1="100" y1="8" x2="100" y2="92" />
                <circle cx="100" cy="50" r="13" />
                <rect x="8" y="30" width="22" height="40" />
                <rect x="170" y="30" width="22" height="40" />
              </svg>
              <div class="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/40 to-transparent p-6 text-white">
                <h1 class="text-2xl font-bold sm:text-3xl">{{ field.name }}</h1>
                <p class="mt-1 flex items-center gap-1 text-white/90"><SvgoPin class="text-base" /> {{ field.address }}, {{ field.city }}</p>
              </div>
            </div>
            <div class="grid grid-cols-3 divide-x divide-ink-100">
              <div class="p-4 text-center">
                <p class="text-xs text-ink-400">{{ $t('fields.price') }}</p>
                <p class="mt-1 font-semibold text-ink-900">{{ money(field.pricePerHour) }}<span class="text-xs font-normal text-ink-400">/{{ $t('common.h') }}</span></p>
              </div>
              <div class="p-4 text-center">
                <p class="text-xs text-ink-400">{{ $t('fields.surface') }}</p>
                <p class="mt-1 font-semibold text-ink-900">{{ $t(`grass.${field.grassType}`) }}</p>
              </div>
              <div class="p-4 text-center">
                <p class="text-xs text-ink-400">{{ $t('fields.format') }}</p>
                <p class="mt-1 font-semibold text-ink-900">8 × 8 · 90′</p>
              </div>
            </div>
          </section>

          <section class="card p-5 sm:p-6">
            <div class="mb-4 flex items-center justify-between gap-3">
              <h2 class="section-title whitespace-nowrap">{{ $t('fields.pickDay') }}</h2>
              <ClientOnly>
                <VueDatePicker
                  v-model="pickerDate"
                  :min-date="tomorrow"
                  :enable-time-picker="false"
                  :locale="locale"
                  auto-apply
                  :clearable="false"
                  :format="(d: Date) => format(d, 'd MMM yyyy')"
                >
                  <template #trigger>
                    <button class="inline-flex items-center gap-1.5 rounded-xl border border-ink-200 px-3 py-2 text-sm font-medium text-ink-600 transition hover:bg-ink-50">
                      <SvgoCalendar class="text-base" /> {{ $t('fields.otherDate') }}
                    </button>
                  </template>
                </VueDatePicker>
              </ClientOnly>
            </div>

            <div class="no-scrollbar -mx-1 mb-6 flex gap-2 overflow-x-auto px-1 pb-1">
              <button
                v-for="d in days"
                :key="d.value"
                class="flex w-16 shrink-0 flex-col items-center rounded-2xl border py-2.5 transition"
                :class="selectedDate === d.value ? 'border-brand-500 bg-brand-500 text-white shadow-md shadow-brand-500/30' : 'border-ink-100 bg-white text-ink-700 hover:border-ink-300'"
                @click="selectedDate = d.value"
              >
                <span class="text-[11px] uppercase opacity-80">{{ d.weekday }}</span>
                <span class="text-lg font-bold">{{ d.day }}</span>
                <span class="text-[11px] opacity-80">{{ d.month }}</span>
              </button>
            </div>

            <div class="mb-3 flex items-center justify-between text-sm">
              <p class="font-medium capitalize text-ink-700">{{ longDate(selectedDate) }}</p>
              <p class="text-ink-400">{{ $t('fields.freeSlots', { n: freeCount }) }}</p>
            </div>
            <SlotGrid :slots="slots" :selected="selectedTime" @select="selectedTime = $event" />
          </section>
        </div>

        <!-- Right: booking summary -->
        <aside class="lg:sticky lg:top-24 lg:self-start">
          <div class="card p-6">
            <h2 class="section-title">{{ $t('fields.bookingTitle') }}</h2>
            <p class="mt-1 text-sm text-ink-500">{{ $t('fields.bookingText') }}</p>

            <dl class="my-6 space-y-3 text-sm">
              <div class="flex justify-between"><dt class="text-ink-500">{{ $t('fields.date') }}</dt><dd class="font-medium capitalize text-ink-900">{{ longDate(selectedDate) }}</dd></div>
              <div class="flex justify-between"><dt class="text-ink-500">{{ $t('fields.time') }}</dt><dd class="font-medium text-ink-900">{{ selectedTime ? `${selectedTime} – ${slots.find(s => s.startTime === selectedTime)?.endTime}` : '—' }}</dd></div>
              <div class="flex justify-between"><dt class="text-ink-500">{{ $t('fields.priceTotal') }}</dt><dd class="font-medium text-ink-900">{{ money(field.pricePerHour * 1.5) }}</dd></div>
            </dl>

            <UiButton block size="lg" :disabled="!selectedTime" @click="book">
              {{ selectedTime ? $t('fields.continue') : $t('fields.selectSlot') }} <SvgoArrowRight v-if="selectedTime" class="text-lg" />
            </UiButton>
            <p class="mt-3 flex items-start gap-1.5 text-xs text-ink-400">
              <SvgoInfo class="mt-0.5 text-sm" /> {{ $t('fields.paymentNote') }}
            </p>
          </div>
        </aside>
      </div>
    </template>

    <UiEmptyState v-else :title="$t('errors.notFound')">
      <UiButton to="/fields" size="sm">{{ $t('nav.fields') }}</UiButton>
    </UiEmptyState>
  </div>
</template>
