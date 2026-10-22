import type { FetchError } from 'ofetch'

// One place for every call to the backend:
//  - adds the API address in front of the path ("/auth/me" → "http://localhost:5289/auth/me")
//  - sends the JWT token in the Authorization header
//  - if the token is rejected (401) on a protected call, logs the user out
export function useApi() {
  const config = useRuntimeConfig()
  const auth = useAuthStore()

  return $fetch.create({
    baseURL: config.public.apiBase as string,

    onRequest({ options }) {
      if (auth.token) {
        options.headers.set('Authorization', `Bearer ${auth.token}`)
      }
    },

    onResponseError({ response }) {
      if (response.status === 401 && auth.token) {
        auth.logout()
        navigateTo('/login')
      }
    },
  })
}

// Turns any API error into one readable (Azerbaijani) message.
// The backend sends either a plain string ("Bu vaxt artıq tutulub.") or a validation object
// ({ title, errors: { Email: ["Düzgün e-poçt daxil edin."] } }).
export function apiErrorMessage(error: unknown, fallback: string): string {
  const data = (error as FetchError)?.data

  if (typeof data === 'string' && data.trim()) return data

  if (data && typeof data === 'object') {
    const errors = (data as { errors?: Record<string, string[]> }).errors
    const first = errors && Object.values(errors).flat()[0]
    if (first) return first
    const title = (data as { title?: string }).title
    if (title) return title
  }

  return fallback
}
