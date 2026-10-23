import { defineStore } from 'pinia'
import type { JoinRequestDto, MyInvitations, MyJoinRequest } from '~/types'

// Everything waiting for the logged-in user's attention, in one place,
// so the bell badge, the home page and the invitations page show the same numbers:
//   GET /me/invitations          → match invitations received / sent (my team is the owner's)
//   GET /teams/{id}/join-requests → players who want to join my team (owner only)
//   GET /me/join-requests        → teams I asked to join
// Call refresh() after any action that changes one of these.
export const useInboxStore = defineStore('inbox', {
  state: () => ({
    invitations: { received: [], sent: [] } as MyInvitations,
    teamRequests: [] as JoinRequestDto[],
    myRequests: [] as MyJoinRequest[],
  }),

  getters: {
    // The red badge on the bell: things I must answer
    attentionCount: state => state.invitations.received.length + state.teamRequests.length,
  },

  actions: {
    async refresh() {
      const auth = useAuthStore()
      if (!auth.isLoggedIn) return this.clear()

      const api = useApi()
      const ownedTeam = auth.team && auth.team.ownerId === auth.userId ? auth.team : null

      const [invitations, teamRequests, myRequests] = await Promise.all([
        api<MyInvitations>('/me/invitations'),
        ownedTeam ? api<JoinRequestDto[]>(`/teams/${ownedTeam.id}/join-requests`) : Promise.resolve([]),
        api<MyJoinRequest[]>('/me/join-requests'),
      ])

      this.invitations = invitations
      this.teamRequests = teamRequests
      this.myRequests = myRequests
    },

    clear() {
      this.invitations = { received: [], sent: [] }
      this.teamRequests = []
      this.myRequests = []
    },
  },
})
