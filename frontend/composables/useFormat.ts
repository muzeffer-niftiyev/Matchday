import { format, formatDistanceToNowStrict, isToday, isTomorrow, isYesterday, parseISO } from 'date-fns'
import { az } from 'date-fns/locale'
import moment from 'moment'
import { hhmm, slotEnd } from '~/utils/rules'

export function useFormat() {
  const { t } = useI18n()
  const dfLocale = computed(() => az)

  // "Today", "Tomorrow", or "Sat, 25 Oct"
  function niceDate(date: string) {
    const d = parseISO(date)
    if (isToday(d)) return t('common.today')
    if (isTomorrow(d)) return t('common.tomorrow')
    if (isYesterday(d)) return t('common.yesterday')
    return format(d, 'EEE, d MMM', { locale: dfLocale.value })
  }

  function longDate(date: string) {
    return format(parseISO(date), 'EEEE, d MMMM yyyy', { locale: dfLocale.value })
  }

  // "18:00 – 19:30" (accepts "18:00" or the API's "18:00:00")
  function timeRange(start: string) {
    return `${hhmm(start)} – ${slotEnd(hhmm(start))}`
  }

  function relative(iso: string) {
    return formatDistanceToNowStrict(parseISO(iso), { addSuffix: true, locale: dfLocale.value })
  }

  // "2h 14m" left until an ISO date, using moment durations
  function timeLeft(iso: string) {
    const ms = moment(iso).diff(moment())
    if (ms <= 0) return null
    const d = moment.duration(ms)
    const h = Math.floor(d.asHours())
    const m = d.minutes()
    return h > 0 ? `${h}${t('common.h')} ${m}${t('common.m')}` : `${m}${t('common.m')} ${d.seconds()}${t('common.s')}`
  }

  function money(value: number) {
    return `${value.toFixed(0)} AZN`
  }

  return { niceDate, longDate, timeRange, relative, timeLeft, money }
}
