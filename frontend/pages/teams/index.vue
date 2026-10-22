<script setup lang="ts">
import { refDebounced } from "@vueuse/core";
import { CITIES } from "~/utils/rules";
import type { Team } from "~/types";

const api = useApi();
const auth = useAuthStore();
const { t } = useI18n();

const { data: teams } = await useAsyncData("teams", () => api<Team[]>("/teams"));

useHead({ title: () => t("nav.teams") });

const search = ref("");
const debounced = refDebounced(search, 200);
const city = ref("");

// GET /me/team, loaded in the auth store
const myTeam = computed(() => auth.team);

const filteredTeams = computed(() =>
  (teams.value ?? [])
    .filter((team) => !city.value || team.city === city.value)
    .filter((team) => team.name.toLowerCase().includes(debounced.value.toLowerCase()))
    .sort((a, b) => a.name.localeCompare(b.name)),
);
</script>

<template>
  <div class="container-page">
    <PageHeader :title="$t('nav.teams')" :subtitle="$t('teams.subtitle')">
      <template #actions>
        <UiButton v-if="myTeam" :to="`/teams/${myTeam.id}`" variant="secondary"
          ><TeamBadge :team="myTeam" size="sm" class="!h-6 !w-6 !rounded-md !text-[9px]" />
          {{ $t("teams.myTeam") }}</UiButton
        >
        <UiButton v-else :to="auth.isLoggedIn ? '/teams/new' : '/login?redirect=/teams/new'"
          ><SvgoPlus class="text-lg" /> {{ $t("teams.create") }}</UiButton
        >
      </template>
    </PageHeader>

    <div class="mb-6 flex flex-col gap-3 sm:flex-row sm:items-center">
      <div class="flex flex-1 gap-2">
        <div class="relative flex-1 sm:max-w-xs">
          <SvgoSearch
            class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-base text-ink-400"
          />
          <input v-model="search" class="input pl-9" :placeholder="$t('teams.search')" />
        </div>
        <select v-model="city" class="input w-auto">
          <option value="">{{ $t("common.allCities") }}</option>
          <option v-for="c in CITIES" :key="c" :value="c">{{ c }}</option>
        </select>
      </div>
    </div>

    <TransitionGroup
      v-if="filteredTeams.length"
      tag="div"
      name="fade"
      class="grid grid-cols-1 gap-4 md:grid-cols-2"
    >
      <TeamCard v-for="team in filteredTeams" :key="team.id" :team="team" />
    </TransitionGroup>
    <UiEmptyState v-else :title="$t('teams.noneTitle')" :text="$t('teams.noneText')">
      <template #icon><SvgoUsers class="text-2xl" /></template>
      <UiButton v-if="auth.isLoggedIn" to="/teams/new" size="sm">{{ $t("teams.create") }}</UiButton>
    </UiEmptyState>
  </div>
</template>
