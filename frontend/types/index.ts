// These types mirror the backend DTOs, so swapping dummy data for the real API later is easy.

export type UserRole = 'User' | 'Admin'
export type TeamRole = 'Captain' | 'Player'
export type GrassType = 'Natural' | 'Artificial'
export type MatchStatus = 'Pending' | 'Confirmed' | 'Declined' | 'Expired' | 'Cancelled' | 'Completed'

// The same shape as UserDto from GET /auth/me
export interface User {
  id: number
  email: string
  displayName: string
  role: UserRole
  createdAt?: string
}

export interface Team {
  id: number
  name: string
  city: string
  color: string
  ownerId: number
  createdAt: string
}

export interface TeamMember {
  teamId: number
  userId: number
  role: TeamRole
  joinedAt: string
}

// A player asked to join a team. Accepted or declined requests are deleted.
export interface JoinRequest {
  teamId: number
  userId: number
  createdAt: string
}

export interface Field {
  id: number
  name: string
  city: string
  address: string
  grassType: GrassType
  pricePerHour: number
  gradient: string
}

export interface Match {
  id: number
  fieldId: number
  date: string // yyyy-MM-dd, Baku date
  startTime: string // HH:mm, one of the fixed slots
  homeTeamId: number
  awayTeamId: number
  status: MatchStatus
  createdAt: string // ISO, UTC
  expiresAt: string // ISO, UTC
  homeScore: number | null
  awayScore: number | null
  homeLineup: number[]
  awayLineup: number[]
}

export interface Slot {
  startTime: string
  endTime: string
  isFree: boolean
}
