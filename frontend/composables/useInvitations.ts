// Invitations of the logged-in user: received (I own the away team) and sent (I own the home team).
export function useInvitations() {
  const data = useDataStore()
  const auth = useAuthStore()

  const myTeamIds = computed(() => (auth.userId ? data.ownedTeams(auth.userId).map(t => t.id) : []))

  const received = computed(() =>
    data.matches.filter(m => myTeamIds.value.includes(m.awayTeamId) && data.statusOf(m) === 'Pending'),
  )

  const sent = computed(() =>
    data.matches.filter(m => myTeamIds.value.includes(m.homeTeamId) && data.statusOf(m) === 'Pending'),
  )

  return { received, sent }
}
