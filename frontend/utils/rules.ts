// The same rules as the backend (TeamRules, BookingSlots).

export const PLAYERS_PER_MATCH = 8
export const MAX_SQUAD_SIZE = 25
export const MATCH_DURATION_MINUTES = 90
export const INVITATION_EXPIRY_HOURS = 5

export const SLOT_START_TIMES = ['09:00', '10:30', '12:00', '13:30', '15:00', '16:30', '18:00', '19:30', '21:00']

export const CITIES = ['Bakı', 'Sumqayıt', 'Gəncə']

export const TEAM_COLORS = ['#10b981', '#0ea5e9', '#6366f1', '#f43f5e', '#f59e0b', '#14b8a6', '#8b5cf6', '#ef4444', '#64748b', '#22c55e']

export function slotEnd(start: string): string {
  const [h, m] = start.split(':').map(Number)
  const total = h * 60 + m + MATCH_DURATION_MINUTES
  return `${String(Math.floor(total / 60)).padStart(2, '0')}:${String(total % 60).padStart(2, '0')}`
}
