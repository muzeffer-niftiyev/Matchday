import { addDays, addHours, format, subDays } from 'date-fns'
import type { Field, JoinRequest, Match, Team, TeamMember, User } from '~/types'
import { PLAYERS_PER_MATCH } from '~/utils/rules'

// Dates are generated relative to "today", so the demo always looks current.
const day = (offset: number) => format(offset >= 0 ? addDays(new Date(), offset) : subDays(new Date(), -offset), 'yyyy-MM-dd')
const isoAgo = (hours: number) => addHours(new Date(), -hours).toISOString()
const isoIn = (hours: number) => addHours(new Date(), hours).toISOString()

const firstNames = [
  'Əli', 'Vüsal', 'Nigar', 'Orxan', 'Kamran', 'Elvin', 'Rauf', 'Tural', 'Murad', 'Samir',
  'Aydın', 'Fuad', 'Rəşad', 'Elnur', 'Fərid', 'Kənan', 'Emin', 'İlkin', 'Cavid', 'Nicat',
  'Ramin', 'Tofiq', 'Zaur', 'Anar', 'Elçin', 'Mahir', 'Ruslan', 'Şahin', 'Vüqar', 'Yusif',
  'Bəhruz', 'Cavad', 'Eldar', 'Hikmət', 'İlqar', 'Kərim', 'Nurlan', 'Oqtay', 'Pərviz', 'Sabir',
  'Teymur', 'Ülvi', 'Xəyal', 'Zamiq', 'Akif', 'Bayram', 'Daşqın', 'Firuz', 'Gündüz', 'Hüseyn',
  'İmran', 'Cəlal', 'Laçın', 'Mirzə', 'Natiq', 'Oruc', 'Polad', 'Rüfət', 'Seymur', 'Təbriz',
]
const lastNames = [
  'Məmmədov', 'Əliyev', 'Hüseynov', 'Həsənov', 'Quliyev', 'İsmayılov', 'Abbasov', 'Rzayev',
  'Kərimov', 'Babayev', 'Cəfərov', 'Mustafayev', 'Vəliyev', 'Sadıqov', 'Nəcəfov',
]

export function createDummyData() {
  const users: User[] = firstNames.map((name, i) => ({
    id: i + 1,
    email: `oyuncu${i + 1}@matchday.az`,
    displayName: `${name} ${lastNames[i % lastNames.length]}`,
    role: 'User',
  }))
  users.push({ id: 100, email: 'admin@matchday.az', displayName: 'Matchday Admini', role: 'Admin' })

  const teams: Team[] = [
    { id: 1, name: 'Sabah Həvəskarları', city: 'Bakı', color: '#10b981', ownerId: 1, createdAt: isoAgo(24 * 60) },
    { id: 2, name: 'Neftçi Veteranları', city: 'Bakı', color: '#0ea5e9', ownerId: 11, createdAt: isoAgo(24 * 50) },
    { id: 3, name: 'Qarabağ Dostları', city: 'Bakı', color: '#f59e0b', ownerId: 19, createdAt: isoAgo(24 * 45) },
    { id: 4, name: 'Ofis FK', city: 'Bakı', color: '#6366f1', ownerId: 27, createdAt: isoAgo(24 * 30) },
    { id: 5, name: 'Bakı Hücumçuları', city: 'Bakı', color: '#f43f5e', ownerId: 33, createdAt: isoAgo(24 * 20) },
    { id: 6, name: 'Gəncə Birliyi', city: 'Gəncə', color: '#8b5cf6', ownerId: 41, createdAt: isoAgo(24 * 12) },
    { id: 7, name: 'Sumqayıt Sahil', city: 'Sumqayıt', color: '#14b8a6', ownerId: 49, createdAt: isoAgo(24 * 5) },
  ]

  // Squads (user ids per team). One team per player: nobody appears twice.
  // Users 53-60 are free agents (no team) - some of them have sent join requests.
  const squads: Record<number, number[]> = {
    1: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10],
    2: [11, 12, 13, 14, 15, 16, 17, 18],
    3: [19, 20, 21, 22, 23, 24, 25, 26],
    4: [27, 28, 29, 30, 31, 32, 51, 52],
    5: [33, 34, 35, 36, 37, 38, 39, 40],
    6: [41, 42, 43, 44, 45, 46, 47, 48],
    7: [49, 50],
  }

  const joinRequests: JoinRequest[] = [
    { teamId: 1, userId: 53, createdAt: isoAgo(5) },
    { teamId: 1, userId: 54, createdAt: isoAgo(26) },
    { teamId: 3, userId: 53, createdAt: isoAgo(3) },
    { teamId: 7, userId: 55, createdAt: isoAgo(30) },
    { teamId: 7, userId: 56, createdAt: isoAgo(8) },
  ]

  const members: TeamMember[] = Object.entries(squads).flatMap(([teamId, ids]) =>
    ids.map((userId, i) => ({
      teamId: Number(teamId),
      userId,
      role: teams.find(t => t.id === Number(teamId))!.ownerId === userId ? 'Captain' : 'Player',
      joinedAt: isoAgo(24 * (40 - i)),
    })),
  )

  const lineup = (teamId: number, exclude: number[] = []) =>
    squads[teamId].filter(id => !exclude.includes(id)).slice(0, PLAYERS_PER_MATCH)

  const fields: Field[] = [
    { id: 1, name: 'Mərkəzi Arena', city: 'Bakı', address: 'Nizami rayonu, 28 May küç.', grassType: 'Artificial', pricePerHour: 60, gradient: 'from-emerald-400 to-teal-600' },
    { id: 2, name: 'Koroğlu Arena', city: 'Bakı', address: 'Nərimanov rayonu, Koroğlu pr.', grassType: 'Artificial', pricePerHour: 80, gradient: 'from-sky-400 to-indigo-600' },
    { id: 3, name: 'Yaşıl Park Stadionu', city: 'Bakı', address: 'Yasamal rayonu, Şərifzadə küç.', grassType: 'Natural', pricePerHour: 120, gradient: 'from-lime-400 to-emerald-700' },
    { id: 4, name: 'Dənizkənarı Meydança', city: 'Bakı', address: 'Bayıl, Bulvar', grassType: 'Artificial', pricePerHour: 70, gradient: 'from-cyan-400 to-blue-600' },
    { id: 5, name: 'Gəncə İdman Zalı', city: 'Gəncə', address: 'Cavad xan küç.', grassType: 'Artificial', pricePerHour: 45, gradient: 'from-violet-400 to-purple-700' },
    { id: 6, name: 'Sumqayıt Meydançası 2', city: 'Sumqayıt', address: 'Sülh küç. 14', grassType: 'Natural', pricePerHour: 55, gradient: 'from-amber-400 to-orange-600' },
  ]

  let id = 0
  const match = (m: Omit<Match, 'id' | 'homeLineup' | 'awayLineup' | 'createdAt' | 'expiresAt'> & Partial<Match>): Match => {
    const homeLineup = m.homeLineup ?? lineup(m.homeTeamId)
    const awayLineup = m.awayLineup ?? (m.status === 'Pending' || m.status === 'Declined' || m.status === 'Expired' ? [] : lineup(m.awayTeamId, homeLineup))
    return {
      id: ++id,
      createdAt: m.createdAt ?? isoAgo(30),
      expiresAt: m.expiresAt ?? isoAgo(25),
      homeLineup,
      awayLineup,
      ...m,
    } as Match
  }

  const matches: Match[] = [
    // Past results
    match({ fieldId: 1, date: day(-3), startTime: '18:00', homeTeamId: 1, awayTeamId: 2, status: 'Completed', homeScore: 3, awayScore: 2 }),
    match({ fieldId: 2, date: day(-7), startTime: '19:30', homeTeamId: 4, awayTeamId: 1, status: 'Completed', homeScore: 1, awayScore: 1, homeLineup: [27, 28, 29, 30, 31, 32, 51, 52], awayLineup: [1, 2, 3, 4, 5, 6, 7, 8] }),
    match({ fieldId: 3, date: day(-10), startTime: '16:30', homeTeamId: 3, awayTeamId: 1, status: 'Completed', homeScore: 4, awayScore: 0 }),
    match({ fieldId: 1, date: day(-5), startTime: '21:00', homeTeamId: 2, awayTeamId: 3, status: 'Completed', homeScore: 2, awayScore: 1 }),
    match({ fieldId: 4, date: day(-4), startTime: '12:00', homeTeamId: 6, awayTeamId: 5, status: 'Completed', homeScore: 0, awayScore: 2 }),
    // Finished, waiting for the admin to enter the score
    match({ fieldId: 2, date: day(-1), startTime: '21:00', homeTeamId: 2, awayTeamId: 4, status: 'Confirmed', homeScore: null, awayScore: null }),
    match({ fieldId: 3, date: day(-2), startTime: '18:00', homeTeamId: 5, awayTeamId: 3, status: 'Confirmed', homeScore: null, awayScore: null }),
    // Upcoming
    match({ fieldId: 1, date: day(2), startTime: '18:00', homeTeamId: 1, awayTeamId: 2, status: 'Confirmed', homeScore: null, awayScore: null }),
    match({ fieldId: 4, date: day(1), startTime: '16:30', homeTeamId: 5, awayTeamId: 6, status: 'Confirmed', homeScore: null, awayScore: null }),
    match({ fieldId: 2, date: day(4), startTime: '19:30', homeTeamId: 4, awayTeamId: 3, status: 'Confirmed', homeScore: null, awayScore: null }),
    // Invitations
    match({ fieldId: 3, date: day(3), startTime: '19:30', homeTeamId: 3, awayTeamId: 1, status: 'Pending', homeScore: null, awayScore: null, createdAt: isoAgo(2), expiresAt: isoIn(3) }),
    match({ fieldId: 1, date: day(5), startTime: '12:00', homeTeamId: 4, awayTeamId: 5, status: 'Pending', homeScore: null, awayScore: null, createdAt: isoAgo(1), expiresAt: isoIn(4) }),
    // History of other outcomes
    match({ fieldId: 2, date: day(-6), startTime: '10:30', homeTeamId: 1, awayTeamId: 3, status: 'Declined', homeScore: null, awayScore: null }),
    match({ fieldId: 4, date: day(-8), startTime: '15:00', homeTeamId: 2, awayTeamId: 1, status: 'Cancelled', homeScore: null, awayScore: null }),
  ]

  return { users, teams, members, joinRequests, fields, matches }
}

export type DummyData = ReturnType<typeof createDummyData>
