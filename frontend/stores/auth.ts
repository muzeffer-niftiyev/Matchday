import { defineStore } from 'pinia'
import { useDataStore } from './data'

// Demo login: no real passwords. Later this store will keep the JWT token from POST /auth/login.
export const useAuthStore = defineStore('auth', {
  state: () => ({
    userId: null as number | null,
  }),

  getters: {
    user(): ReturnType<ReturnType<typeof useDataStore>['user']> {
      return this.userId ? useDataStore().user(this.userId) : undefined
    },
    isLoggedIn(): boolean {
      return !!this.user
    },
    isAdmin(): boolean {
      return this.user?.role === 'Admin'
    },
  },

  actions: {
    loginAs(userId: number) {
      this.userId = userId
    },

    loginWithEmail(email: string) {
      const data = useDataStore()
      const existing = data.users.find(u => u.email.toLowerCase() === email.trim().toLowerCase())
      // Any unknown email logs in as the demo player (Ali), so the demo is easy to explore.
      this.userId = existing?.id ?? 1
    },

    logout() {
      this.userId = null
    },
  },

  persist: true,
})
