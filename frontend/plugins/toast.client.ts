import Toast, { POSITION, type PluginOptions } from 'vue-toastification'

export default defineNuxtPlugin((nuxtApp) => {
  const options: PluginOptions = {
    position: POSITION.BOTTOM_RIGHT,
    timeout: 3000,
    hideProgressBar: true,
    closeButton: false,
    icon: true,
    transition: 'Vue-Toastification__fade',
    maxToasts: 4,
  }

  nuxtApp.vueApp.use(Toast, options)
})
