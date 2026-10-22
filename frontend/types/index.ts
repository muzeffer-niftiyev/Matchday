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
  ownerName?: string
  memberCount?: number
  createdAt: string
}

// GET /teams/{id}/members
export interface TeamMemberDto {
  userId: number
  displayName: string
  role: TeamRole
  joinedAt: string
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

// GET /teams/{id}/matches and GET /matches
export interface MatchSummary {
  id: number
  date: string // yyyy-MM-dd
  startTime: string // HH:mm:ss (C# TimeOnly)
  status: MatchStatus
  homeTeamId: number
  homeTeamName: string
  homeTeamColor: string
  awayTeamId: number
  awayTeamName: string
  awayTeamColor: string
  homeScore: number | null
  awayScore: number | null
  fieldName: string
  fieldCity: string
}

export interface Slot {
  startTime: string
  endTime: string
  isFree: boolean
}
