import { defineStore } from 'pinia'
import type { User } from '~/types'

// Real authentication against the backend:
//   POST /auth/login    → { token }
//   GET  /auth/me       → the current user (read from the database, so the role is always fresh)
//   POST /auth/register → creates the user (then we log in)
export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: null as string | null,
    user: null as User | null,
  }),

  getters: {
    userId: state => state.user?.id ?? null,
    isLoggedIn: state => !!state.token && !!state.user,
    isAdmin: state => state.user?.role === 'Admin',
  },

  actions: {
    async login(email: string, password: string) {
      const api = useApi()
      const { token } = await api<{ token: string }>('/auth/login', {
        method: 'POST',
        body: { email, password },
      })
      this.token = token
      await this.fetchMe()
    },

    async register(displayName: string, email: string, password: string) {
      const api = useApi()
      await api('/auth/register', {
        method: 'POST',
        body: { displayName, email, password },
      })
      // Register only creates the account (201). Log in right after, to get a token.
      await this.login(email, password)
    },

    async fetchMe() {
      const api = useApi()
      this.user = await api<User>('/auth/me')
    },

    logout() {
      this.token = null
      this.user = null
    },
  },

  persist: true,
})
