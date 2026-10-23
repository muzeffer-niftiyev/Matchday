// Everything waiting for the logged-in user's attention (data lives in the inbox store):
// - match invitations received / sent (only team owners answer or send them)
// - join requests to my team (owner) and my own open requests (player without a team)
export function useInvitations() {
  const auth = useAuthStore()
  const inbox = useInboxStore()

  const myTeam = computed(() => auth.team ?? undefined)
  const ownedTeam = computed(() => (auth.team && auth.team.ownerId === auth.userId ? auth.team : undefined))

  const received = computed(() => inbox.invitations.received)
  const sent = computed(() => inbox.invitations.sent)
  const joinRequests = computed(() => inbox.teamRequests)
  const myRequests = computed(() => (myTeam.value ? [] : inbox.myRequests))
  const attentionCount = computed(() => inbox.attentionCount)

  return { myTeam, ownedTeam, received, sent, joinRequests, myRequests, attentionCount }
}
