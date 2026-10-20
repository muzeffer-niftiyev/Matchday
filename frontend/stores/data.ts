import { defineStore } from 'pinia'
import { addHours, format } from 'date-fns'
import { createDummyData } from '~/data/dummy'
import type { Field, GrassType, Match, Team, TeamMember, User } from '~/types'
import { INVITATION_EXPIRY_HOURS, PLAYERS_PER_MATCH, SLOT_START_TIMES, slotEnd } from '~/utils/rules'

// Result of an action: ok, or the i18n key of an error message (like the backend's 400/403/409 answers).
export type ActionResult = { ok: true; id?: number } | { ok: false; error: string }

const ok = (id?: number): ActionResult => ({ ok: true, id })
const fail = (error: string): ActionResult => ({ ok: false, error })

export function matchStart(m: Pick<Match, 'date' | 'startTime'>) {
  return new Date(`${m.date}T${m.startTime}:00`)
}

export function matchEnd(m: Pick<Match, 'date' | 'startTime'>) {
  return new Date(`${m.date}T${slotEnd(m.startTime)}:00`)
}

export const useDataStore = defineStore('data', {
  state: () => createDummyData(),

  getters: {
    user: state => (id: number) => state.users.find(u => u.id === id),
    team: state => (id: number) => state.teams.find(t => t.id === id),
    field: state => (id: number) => state.fields.find(f => f.id === id),
    match: state => (id: number) => state.matches.find(m => m.id === id),

    teamMembers: state => (teamId: number) =>
      state.members
        .filter(m => m.teamId === teamId)
        .sort((a, b) => (a.role === 'Captain' ? -1 : b.role === 'Captain' ? 1 : a.joinedAt.localeCompare(b.joinedAt))),

    teamsOfUser: state => (userId: number) =>
      state.teams.filter(t => state.members.some(m => m.teamId === t.id && m.userId === userId)),

    ownedTeams: state => (userId: number) => state.teams.filter(t => t.ownerId === userId),

    isMember: state => (teamId: number, userId: number) =>
      state.members.some(m => m.teamId === teamId && m.userId === userId),

    // A pending invitation whose 5 hours have passed counts as expired (lazy expiry, like the backend).
    statusOf: () => (m: Match) =>
      m.status === 'Pending' && new Date(m.expiresAt) <= new Date() ? 'Expired' : m.status,

    isActive(): (m: Match) => boolean {
      return m => {
        const s = this.statusOf(m)
        return s === 'Pending' || s === 'Confirmed'
      }
    },

    matchesOfTeam: state => (teamId: number) =>
      state.matches
        .filter(m => m.homeTeamId === teamId || m.awayTeamId === teamId)
        .sort((a, b) => matchStart(b).getTime() - matchStart(a).getTime()),
  },

  actions: {
    slotsFor(fieldId: number, date: string) {
      const taken = this.matches
        .filter(m => m.fieldId === fieldId && m.date === date && this.isActive(m))
        .map(m => m.startTime)

      return SLOT_START_TIMES.map(start => ({
        startTime: start,
        endTime: slotEnd(start),
        isFree: !taken.includes(start),
      }))
    },

    // ---------- Teams ----------
    createTeam(userId: number, name: string, city: string, color: string): ActionResult {
      if (this.teams.some(t => t.name.toLowerCase() === name.trim().toLowerCase())) return fail('errors.teamNameTaken')

      const id = Math.max(0, ...this.teams.map(t => t.id)) + 1
      const now = new Date().toISOString()
      this.teams.push({ id, name: name.trim(), city, color, ownerId: userId, createdAt: now })
      this.members.push({ teamId: id, userId, role: 'Captain', joinedAt: now })
      return ok(id)
    },

    updateTeam(teamId: number, userId: number, name: string, city: string, color: string): ActionResult {
      const team = this.team(teamId)
      if (!team) return fail('errors.notFound')
      if (team.ownerId !== userId) return fail('errors.forbidden')
      if (this.teams.some(t => t.id !== teamId && t.name.toLowerCase() === name.trim().toLowerCase())) return fail('errors.teamNameTaken')

      Object.assign(team, { name: name.trim(), city, color })
      return ok(teamId)
    },

    deleteTeam(teamId: number, userId: number): ActionResult {
      const team = this.team(teamId)
      if (!team) return fail('errors.notFound')
      if (team.ownerId !== userId) return fail('errors.forbidden')

      this.teams = this.teams.filter(t => t.id !== teamId)
      this.members = this.members.filter(m => m.teamId !== teamId)
      this.matches = this.matches.filter(m => m.homeTeamId !== teamId && m.awayTeamId !== teamId)
      return ok()
    },

    joinTeam(teamId: number, userId: number): ActionResult {
      if (!this.team(teamId)) return fail('errors.notFound')
      if (this.isMember(teamId, userId)) return fail('errors.alreadyMember')

      this.members.push({ teamId, userId, role: 'Player', joinedAt: new Date().toISOString() })
      return ok()
    },

    leaveTeam(teamId: number, userId: number): ActionResult {
      const team = this.team(teamId)
      if (!team) return fail('errors.notFound')
      if (team.ownerId === userId) return fail('errors.ownerCannotLeave')
      if (!this.isMember(teamId, userId)) return fail('errors.notMember')

      this.members = this.members.filter(m => !(m.teamId === teamId && m.userId === userId))
      return ok()
    },

    kickMember(teamId: number, callerId: number, memberId: number): ActionResult {
      const team = this.team(teamId)
      if (!team) return fail('errors.notFound')
      if (team.ownerId !== callerId) return fail('errors.forbidden')
      if (memberId === callerId) return fail('errors.cannotRemoveYourself')

      this.members = this.members.filter(m => !(m.teamId === teamId && m.userId === memberId))
      return ok()
    },

    // ---------- Fields (admin) ----------
    saveField(data: Omit<Field, 'id' | 'gradient'> & { id?: number }): ActionResult {
      if (data.id) {
        const field = this.field(data.id)
        if (!field) return fail('errors.notFound')
        Object.assign(field, data)
        return ok(field.id)
      }

      const gradients = ['from-emerald-400 to-teal-600', 'from-sky-400 to-indigo-600', 'from-rose-400 to-pink-600', 'from-amber-400 to-orange-600']
      const id = Math.max(0, ...this.fields.map(f => f.id)) + 1
      this.fields.push({ ...data, id, gradient: gradients[id % gradients.length] })
      return ok(id)
    },

    deleteField(fieldId: number): ActionResult {
      this.fields = this.fields.filter(f => f.id !== fieldId)
      this.matches = this.matches.filter(m => m.fieldId !== fieldId)
      return ok()
    },

    // ---------- Matches ----------
    validateLineup(teamId: number, playerIds: number[]): ActionResult {
      if (new Set(playerIds).size !== PLAYERS_PER_MATCH) return fail('errors.lineupSize')
      if (!playerIds.every(id => this.isMember(teamId, id))) return fail('errors.lineupMembers')
      return ok()
    },

    teamBusyOnDay(teamIds: number[], date: string, exceptMatchId?: number) {
      return this.matches.some(m =>
        m.id !== exceptMatchId
        && m.date === date
        && this.isActive(m)
        && (teamIds.includes(m.homeTeamId) || teamIds.includes(m.awayTeamId)))
    },

    bookMatch(userId: number, input: { fieldId: number; date: string; startTime: string; homeTeamId: number; awayTeamId: number; playerIds: number[] }): ActionResult {
      const today = format(new Date(), 'yyyy-MM-dd')
      if (input.date <= today) return fail('errors.dateTomorrow')
      if (!SLOT_START_TIMES.includes(input.startTime)) return fail('errors.invalidSlot')
      if (input.homeTeamId === input.awayTeamId) return fail('errors.sameTeam')

      const home = this.team(input.homeTeamId)
      if (!this.field(input.fieldId) || !home || !this.team(input.awayTeamId)) return fail('errors.notFound')
      if (home.ownerId !== userId) return fail('errors.forbidden')

      const lineup = this.validateLineup(input.homeTeamId, input.playerIds)
      if (!lineup.ok) return lineup

      if (this.matches.some(m => m.homeTeamId === input.homeTeamId && this.statusOf(m) === 'Pending')) return fail('errors.pendingInvite')
      if (this.teamBusyOnDay([input.homeTeamId, input.awayTeamId], input.date)) return fail('errors.teamBusy')
      if (!this.slotsFor(input.fieldId, input.date).find(s => s.startTime === input.startTime)?.isFree) return fail('errors.slotTaken')

      const id = Math.max(0, ...this.matches.map(m => m.id)) + 1
      const now = new Date()
      this.matches.push({
        id,
        fieldId: input.fieldId,
        date: input.date,
        startTime: input.startTime,
        homeTeamId: input.homeTeamId,
        awayTeamId: input.awayTeamId,
        status: 'Pending',
        createdAt: now.toISOString(),
        expiresAt: addHours(now, INVITATION_EXPIRY_HOURS).toISOString(),
        homeScore: null,
        awayScore: null,
        homeLineup: [...input.playerIds],
        awayLineup: [],
      })
      return ok(id)
    },

    acceptMatch(matchId: number, userId: number, playerIds: number[]): ActionResult {
      const m = this.match(matchId)
      if (!m) return fail('errors.notFound')
      if (this.team(m.awayTeamId)?.ownerId !== userId) return fail('errors.forbidden')
      if (m.status !== 'Pending') return fail('errors.notWaiting')
      if (this.statusOf(m) === 'Expired') {
        m.status = 'Expired'
        return fail('errors.expired')
      }

      const lineup = this.validateLineup(m.awayTeamId, playerIds)
      if (!lineup.ok) return lineup
      if (playerIds.some(id => m.homeLineup.includes(id))) return fail('errors.bothTeams')
      if (this.teamBusyOnDay([m.homeTeamId, m.awayTeamId], m.date, m.id)) return fail('errors.teamBusy')

      m.awayLineup = [...playerIds]
      m.status = 'Confirmed'
      return ok(m.id)
    },

    declineMatch(matchId: number, userId: number): ActionResult {
      const m = this.match(matchId)
      if (!m) return fail('errors.notFound')
      if (this.team(m.awayTeamId)?.ownerId !== userId) return fail('errors.forbidden')
      if (m.status !== 'Pending') return fail('errors.notWaiting')
      if (this.statusOf(m) === 'Expired') {
        m.status = 'Expired'
        return fail('errors.expired')
      }

      m.status = 'Declined'
      return ok(m.id)
    },

    cancelMatch(matchId: number, userId: number): ActionResult {
      const m = this.match(matchId)
      if (!m) return fail('errors.notFound')
      if (this.team(m.homeTeamId)?.ownerId !== userId) return fail('errors.forbidden')
      if (m.status !== 'Pending' && m.status !== 'Confirmed') return fail('errors.cannotCancel')
      if (matchStart(m) <= new Date()) return fail('errors.alreadyStarted')

      m.status = 'Cancelled'
      return ok(m.id)
    },

    setScore(matchId: number, homeScore: number, awayScore: number): ActionResult {
      const m = this.match(matchId)
      if (!m) return fail('errors.notFound')
      if (m.status !== 'Confirmed' && m.status !== 'Completed') return fail('errors.onlyConfirmed')
      if (matchEnd(m) > new Date()) return fail('errors.notFinished')

      m.homeScore = homeScore
      m.awayScore = awayScore
      m.status = 'Completed'
      return ok(m.id)
    },

    registerUser(displayName: string, email: string): ActionResult {
      if (this.users.some(u => u.email.toLowerCase() === email.trim().toLowerCase())) return fail('errors.emailTaken')

      const id = Math.max(0, ...this.users.map(u => u.id)) + 1
      this.users.push({ id, email: email.trim().toLowerCase(), displayName: displayName.trim(), role: 'User' })
      return ok(id)
    },

    resetDemo() {
      this.$patch(createDummyData())
    },
  },

  persist: true,
})

export type { Field, GrassType, Match, Team, TeamMember, User }
