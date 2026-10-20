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
      htmlAttrs: { lang: 'en' },
      meta: [
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
        { name: 'description', content: 'Book football fields and challenge other teams in Baku.' },
      ],
      link: [
        { rel: 'icon', type: 'image/svg+xml', href: '/favicon.svg' },
        { rel: 'preconnect', href: 'https://fonts.googleapis.com' },
        { rel: 'preconnect', href: 'https://fonts.gstatic.com', crossorigin: '' },
        { rel: 'stylesheet', href: 'https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap' },
      ],
    },
  },

  svgo: {
    autoImportPath: './assets/icons/',
    defaultImport: 'componentext',
  },

  i18n: {
    strategy: 'no_prefix',
    bundle: { optimizeTranslationDirective: false },
    defaultLocale: 'en',
    langDir: 'locales',
    lazy: true,
    locales: [
      { code: 'en', name: 'English', file: 'en.json' },
      { code: 'az', name: 'Azərbaycanca', file: 'az.json' },
    ],
    detectBrowserLanguage: {
      useCookie: true,
      cookieKey: 'matchday_lang',
      redirectOn: 'root',
    },
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
