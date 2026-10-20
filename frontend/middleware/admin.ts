// Admin-only pages: definePageMeta({ middleware: 'admin' })
export default defineNuxtRouteMiddleware((to) => {
  const auth = useAuthStore()
  if (!auth.isLoggedIn) {
    return navigateTo({ path: '/login', query: { redirect: to.fullPath } })
  }
  if (!auth.isAdmin) {
    return navigateTo('/')
  }
})
