// The same rules as the backend (TeamRules, BookingSlots).

export const PLAYERS_PER_MATCH = 8
export const MAX_SQUAD_SIZE = 25
export const MATCH_DURATION_MINUTES = 90
export const INVITATION_EXPIRY_HOURS = 5

export const CITIES = ['Bakı', 'Sumqayıt', 'Gəncə']

export const TEAM_COLORS = ['#10b981', '#0ea5e9', '#6366f1', '#f43f5e', '#f59e0b', '#14b8a6', '#8b5cf6', '#ef4444', '#64748b', '#22c55e']

// Fields have no photo, so each one gets a gradient picked by its id.
export const FIELD_GRADIENTS = [
  'from-emerald-400 to-teal-600',
  'from-sky-400 to-indigo-600',
  'from-lime-400 to-emerald-700',
  'from-cyan-400 to-blue-600',
  'from-violet-400 to-purple-700',
  'from-amber-400 to-orange-600',
]

export function fieldGradient(fieldId: number) {
  return FIELD_GRADIENTS[fieldId % FIELD_GRADIENTS.length]
}

// The API sends times as "18:00:00" (C# TimeOnly) → "18:00"
export function hhmm(time: string) {
  return time.slice(0, 5)
}

export function slotEnd(start: string): string {
  const [h, m] = start.split(':').map(Number)
  const total = h * 60 + m + MATCH_DURATION_MINUTES
  return `${String(Math.floor(total / 60)).padStart(2, '0')}:${String(total % 60).padStart(2, '0')}`
}

// Start / end of a match as a Date (in the browser's time zone, like the demo did)
export function matchStart(m: { date: string; startTime: string }) {
  return new Date(`${m.date}T${hhmm(m.startTime)}:00`)
}

export function matchEnd(m: { date: string; startTime: string }) {
  return new Date(`${m.date}T${slotEnd(hhmm(m.startTime))}:00`)
}

// The cities that appear in a list (the database may spell them differently from CITIES)
export function citiesOf(items: { city: string }[]) {
  return [...new Set(items.map(x => x.city))].sort((a, b) => a.localeCompare(b))
}
