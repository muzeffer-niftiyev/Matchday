// When the website opens, refresh "who am I" from the API.
// If the token has expired (1 hour), useApi gets a 401 and logs the user out.
export default defineNuxtPlugin(async () => {
  const auth = useAuthStore()
  if (!auth.token) return

  try {
    await auth.fetchMe()
  }
  catch {
    auth.logout()
  }
})
