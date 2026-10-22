export default defineNuxtConfig({
  compatibilityDate: '2025-01-15',
  devtools: { enabled: false },
  ssr: false,

  modules: [
    '@nuxtjs/tailwindcss',
    '@nuxtjs/i18n',
    '@pinia/nuxt',
    'pinia-plugin-persistedstate/nuxt',
    'nuxt-svgo',
  ],

  css: [
    '~/assets/css/main.css',
    'vue-toastification/dist/index.css',
    '@vuepic/vue-datepicker/dist/main.css',
    'vue-color/style.css',
  ],

  app: {
    // Pages fade in with a CSS animation (see layouts), so navigation is never blocked.
    pageTransition: false,
    head: {
      title: 'Matchday',
      htmlAttrs: { lang: 'az' },
      meta: [
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
        { name: 'description', content: 'Futbol meydançası rezerv edin və digər komandalara çağırış göndərin.' },
      ],
      link: [
        { rel: 'icon', type: 'image/svg+xml', href: '/favicon.svg' },
        { rel: 'preconnect', href: 'https://fonts.googleapis.com' },
        { rel: 'preconnect', href: 'https://fonts.gstatic.com', crossorigin: '' },
        { rel: 'stylesheet', href: 'https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap' },
      ],
    },
  },

  // Where the backend API runs. Can be overridden with the env var NUXT_PUBLIC_API_BASE.
  runtimeConfig: {
    public: {
      apiBase: 'http://localhost:5289',
    },
  },

  svgo: {
    autoImportPath: './assets/icons/',
    defaultImport: 'componentext',
  },

  i18n: {
    strategy: 'no_prefix',
    bundle: { optimizeTranslationDirective: false },
    // The site is Azerbaijani only. All texts live in i18n/locales/az.json.
    defaultLocale: 'az',
    langDir: 'locales',
    locales: [{ code: 'az', name: 'Azərbaycanca', file: 'az.json' }],
    detectBrowserLanguage: false,
  },

  piniaPluginPersistedstate: {
    storage: 'localStorage',
  },

  // Pre-bundle these once, so the dev server doesn't reload the page every time it discovers one.
  vite: {
    optimizeDeps: {
      include: [
        'date-fns',
        'date-fns/locale',
        'moment',
        '@vueuse/core',
        '@vuepic/vue-datepicker',
        'vue-color',
        'vue-toastification',
        '@vuelidate/core',
        '@vuelidate/validators',
      ],
    },
  },

  typescript: {
    strict: true,
  },
})
