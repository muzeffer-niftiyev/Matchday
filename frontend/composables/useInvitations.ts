// Everything waiting for the logged-in user's attention:
// - match invitations received / sent (only team owners answer or send them)
// - join requests to my team (owner) and my own open requests (player without a team)
export function useInvitations() {
  const data = useDataStore()
  const auth = useAuthStore()

  const myTeam = computed(() => (auth.userId ? data.teamOfUser(auth.userId) : undefined))
  const ownedTeam = computed(() => (auth.userId ? data.ownedTeam(auth.userId) : undefined))

  const received = computed(() =>
    ownedTeam.value
      ? data.matches.filter(m => m.awayTeamId === ownedTeam.value!.id && data.statusOf(m) === 'Pending')
      : [],
  )

  const sent = computed(() =>
    ownedTeam.value
      ? data.matches.filter(m => m.homeTeamId === ownedTeam.value!.id && data.statusOf(m) === 'Pending')
      : [],
  )

  const joinRequests = computed(() => (ownedTeam.value ? data.requestsOfTeam(ownedTeam.value.id) : []))

  const myRequests = computed(() => (auth.userId && !myTeam.value ? data.requestsOfUser(auth.userId) : []))

  // The red badge on the bell: things I must answer
  const attentionCount = computed(() => received.value.length + joinRequests.value.length)

  return { myTeam, ownedTeam, received, sent, joinRequests, myRequests, attentionCount }
}
