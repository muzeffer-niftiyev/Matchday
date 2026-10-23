// These types mirror the backend DTOs (what the API sends and receives).

export type UserRole = 'User' | 'Admin'
export type TeamRole = 'Captain' | 'Player'
export type GrassType = 'Natural' | 'Artificial'
export type MatchStatus = 'Pending' | 'Confirmed' | 'Declined' | 'Expired' | 'Cancelled' | 'Completed'

// GET /auth/me (UserDto)
export interface User {
  id: number
  email: string
  displayName: string
  role: UserRole
  createdAt?: string
}

// GET /teams, GET /teams/{id}, GET /me/team (TeamDto)
export interface Team {
  id: number
  name: string
  city: string
  color: string
  ownerId: number
  ownerName: string
  memberCount: number
  createdAt: string
}

// GET /teams/{id}/members
export interface TeamMemberDto {
  userId: number
  displayName: string
  role: TeamRole
  joinedAt: string
}

// GET /me/join-requests: teams I asked to join
export interface MyJoinRequest {
  teamId: number
  teamName: string
  teamCity: string
  teamColor: string
  createdAt: string
}

// GET /teams/{id}/join-requests: players who asked to join my team (owner only)
export interface JoinRequestDto {
  userId: number
  displayName: string
  createdAt: string
}

// GET /fields (FieldDto)
export interface Field {
  id: number
  name: string
  city: string
  grassType: GrassType
  pricePerHour: number
}

// GET /fields/{id}/slots?date=yyyy-MM-dd
export interface Slot {
  startTime: string // HH:mm:ss
  endTime: string
  isFree: boolean
}

// GET /teams/{id}/matches, GET /matches, GET /me/matches
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

export interface LineupPlayer {
  userId: number
  displayName: string
}

// GET /matches/{id}
export interface MatchDetails {
  id: number
  date: string
  startTime: string
  endTime: string
  status: MatchStatus
  fieldName: string
  fieldCity: string
  homeTeamId: number
  homeTeamName: string
  homeTeamColor: string
  homeTeamOwnerId: number
  awayTeamId: number
  awayTeamName: string
  awayTeamColor: string
  awayTeamOwnerId: number
  homeScore: number | null
  awayScore: number | null
  homeLineup: LineupPlayer[]
  awayLineup: LineupPlayer[]
  expiresAt: string
}

// GET /me/invitations → { received, sent }
export interface Invitation {
  matchId: number
  date: string
  startTime: string
  fieldName: string
  opponentTeamId: number
  opponentTeamName: string
  opponentTeamColor: string
  createdAt: string
  expiresAt: string
}

export interface MyInvitations {
  received: Invitation[]
  sent: Invitation[]
}
