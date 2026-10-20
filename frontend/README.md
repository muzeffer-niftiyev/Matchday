# Matchday frontend

Nuxt 3 + TypeScript + Tailwind. **This is a UI demo:** it does not call the API yet. All data comes from `data/dummy.ts` and lives in Pinia, saved in your browser's localStorage.

## Run it

```bash
npm install
npm run dev
```

Open http://localhost:3000. On the login page, click **As a player** (Ali, owner of Sabah Amateurs) or **As admin**.
Profile → **Reset demo data** restores the sample data.

## Where things are

| Folder | What |
|---|---|
| `pages/` | One file per page (the file path is the URL) |
| `components/` | Reusable pieces. `components/ui/*` are generic (`<UiButton>`, `<UiModal>`, ...) |
| `stores/data.ts` | All dummy data and actions. They use **the same rules as the backend** (8 players, slots, 5-hour expiry, one game per day...) |
| `stores/auth.ts` | Who is logged in (later: the JWT token) |
| `types/index.ts` | TypeScript types that mirror the backend DTOs |
| `utils/rules.ts` | Constants: players per match, slot times, cities |
| `i18n/locales/` | English and Azerbaijani texts |
| `assets/icons/` | SVG icons, used as `<SvgoName />` |

## Connecting the real API later

Every action in `stores/data.ts` (`bookMatch`, `acceptMatch`, `joinTeam`, ...) matches one backend endpoint.
Replacing the body of each action with a `$fetch` call to the API is the next step.
