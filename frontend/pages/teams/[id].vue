<script setup lang="ts">
import { matchStart } from "~/stores/data";
import { MAX_SQUAD_SIZE, PLAYERS_PER_MATCH } from "~/utils/rules";
import type { MatchSummary, Team, TeamMemberDto } from "~/types";

const api = useApi();
const route = useRoute();
const router = useRouter();
const data = useDataStore();
const auth = useAuthStore();
const notify = useNotify();
const { relative } = useFormat();
const { t } = useI18n();

const teamId = computed(() => Number(route.params.id));
const { data: team } = await useAsyncData(`team-${teamId.value}`, () =>
  api<Team>(`/teams/${teamId.value}`),
);

useHead({ title: () => team.value?.name ?? t("nav.teams") });

// GET /teams/{id}/members → [{ userId, displayName, role, joinedAt }]
const { data: membersData } = await useAsyncData(`team-members-${teamId.value}`, () =>
  api<TeamMemberDto[]>(`/teams/${teamId.value}/members`),
);
// null until the answer arrives (or if the team doesn't exist) → use an empty list instead
const members = computed(() => membersData.value ?? []);

const isOwner = computed(() => team.value?.ownerId === auth.userId);
const isMember = computed(() => auth.userId !== null && members.value.some((m) => m.userId === auth.userId));
const canPlay = computed(() => members.value.length >= PLAYERS_PER_MATCH);
const isFull = computed(() => members.value.length >= MAX_SQUAD_SIZE);

// One team per player: if I'm already in another team, I can't ask to join this one.
const myOtherTeam = computed(() => {
  if (!auth.userId || isMember.value) return undefined;
  return auth.team ?? undefined;
});
const hasRequested = computed(
  () => auth.userId !== null && data.hasRequested(teamId.value, auth.userId),
);
const requests = computed(() => (isOwner.value ? data.requestsOfTeam(teamId.value) : []));

// GET /teams/{id}/matches → newest first
const { data: matchesData } = await useAsyncData(`team-matches-${teamId.value}`, () =>
  api<MatchSummary[]>(`/teams/${teamId.value}/matches`),
);
const matches = computed(() => matchesData.value ?? []);
// Pending or Confirmed, and not started yet. reverse() → the nearest game first.
const upcoming = computed(() =>
  matches.value
    .filter((m) => (m.status === "Pending" || m.status === "Confirmed") && matchStart(m) > new Date())
    .reverse(),
);
const past = computed(() => matches.value.filter((m) => m.status === "Completed"));

const tab = ref("squad");
const tabs = computed(() => [
  { value: "squad", label: t("teams.squad"), count: members.value.length },
  ...(isOwner.value
    ? [{ value: "requests", label: t("teams.requests"), count: requests.value.length }]
    : []),
  { value: "upcoming", label: t("teams.upcoming"), count: upcoming.value.length },
  { value: "results", label: t("teams.results"), count: past.value.length },
]);

function resultFor(m: (typeof past.value)[number]) {
  const mine = m.homeTeamId === teamId.value ? m.homeScore! : m.awayScore!;
  const theirs = m.homeTeamId === teamId.value ? m.awayScore! : m.homeScore!;
  return mine > theirs ? "W" : mine < theirs ? "L" : "D";
}

// Actions
const editOpen = ref(false);
const deleteOpen = ref(false);
const kickTarget = ref<{ userId: number; name: string } | null>(null);
const kickOpen = computed({
  get: () => !!kickTarget.value,
  set: (v) => !v && (kickTarget.value = null),
});

function requestToJoin() {
  if (!auth.isLoggedIn) return router.push(`/login?redirect=${route.fullPath}`);
  notify.result(data.requestToJoin(teamId.value, auth.userId!), "teams.requestSent", {
    name: team.value?.name,
  });
}

function cancelRequest() {
  notify.result(data.cancelRequest(teamId.value, auth.userId!), "teams.requestCancelled");
}

function leave() {
  notify.result(data.leaveTeam(teamId.value, auth.userId!), "teams.left", {
    name: team.value?.name,
  });
}

function save(value: { name: string; city: string; color: string }) {
  if (
    notify.result(
      data.updateTeam(teamId.value, auth.userId!, value.name, value.city, value.color),
      "teams.updated",
    )
  ) {
    editOpen.value = false;
  }
}

function remove() {
  const name = team.value?.name;
  if (notify.result(data.deleteTeam(teamId.value, auth.userId!), "teams.deleted", { name })) {
    router.push("/teams");
  }
}

function kick() {
  if (!kickTarget.value) return;
  const name = kickTarget.value.name;
  notify.result(
    data.kickMember(teamId.value, auth.userId!, kickTarget.value.userId),
    "teams.kicked",
    { name },
  );
  kickTarget.value = null;
}
</script>

<template>
  <div class="container-page">
    <template v-if="team">
      <NuxtLink
        to="/teams"
        class="mb-4 inline-flex items-center gap-1 text-sm font-medium text-ink-500 transition hover:text-ink-800"
      >
        <SvgoChevronLeft class="text-base" /> {{ $t("nav.teams") }}
      </NuxtLink>

      <!-- Header -->
      <section class="card relative mb-8 overflow-hidden p-6 sm:p-8">
        <div
          class="absolute inset-x-0 top-0 h-24 opacity-15"
          :style="{ background: `linear-gradient(90deg, ${team.color}, transparent)` }"
        />
        <div class="relative flex flex-col gap-6 lg:flex-row lg:items-center lg:justify-between">
          <div class="flex items-center gap-5">
            <TeamBadge :team="team" size="xl" />
            <div>
              <h1 class="text-2xl font-bold text-ink-900 sm:text-3xl">{{ team.name }}</h1>
              <p class="mt-1 flex flex-wrap items-center gap-x-3 gap-y-1 text-sm text-ink-500">
                <span class="inline-flex items-center gap-1"
                  ><SvgoPin class="text-sm" /> {{ team.city }}</span
                >
                <span class="inline-flex items-center gap-1"
                  ><SvgoUsers class="text-sm" />
                  {{ $t("teams.squadSize", { n: members.length, max: MAX_SQUAD_SIZE }) }}</span
                >
                <span>{{ $t("teams.founded", { time: relative(team.createdAt) }) }}</span>
              </p>
            </div>
          </div>

          <div class="flex flex-wrap gap-2">
            <template v-if="isOwner">
              <UiButton :to="canPlay ? '/matches/new' : undefined" :disabled="!canPlay"
                ><SvgoSwords class="text-base" /> {{ $t("teams.challenge") }}</UiButton
              >
              <UiButton variant="secondary" @click="editOpen = true"
                ><SvgoEdit class="text-base" /> {{ $t("common.edit") }}</UiButton
              >
              <UiButton
                variant="ghost"
                class="!text-rose-600 hover:!bg-rose-50"
                @click="deleteOpen = true"
                ><SvgoTrash class="text-base"
              /></UiButton>
            </template>
            <UiButton v-else-if="isMember" variant="secondary" @click="leave">{{
              $t("teams.leave")
            }}</UiButton>
            <template v-else-if="hasRequested">
              <span class="chip h-11 bg-amber-50 px-4 text-sm text-amber-700"
                ><SvgoClock class="text-base" /> {{ $t("teams.requested") }}</span
              >
              <UiButton variant="ghost" @click="cancelRequest">{{
                $t("teams.cancelRequest")
              }}</UiButton>
            </template>
            <UiButton v-else-if="myOtherTeam" variant="secondary" disabled>{{
              $t("teams.inAnotherTeam")
            }}</UiButton>
            <UiButton v-else-if="isFull" variant="secondary" disabled>{{
              $t("teams.full")
            }}</UiButton>
            <UiButton v-else @click="requestToJoin"
              ><SvgoSend class="text-base" /> {{ $t("teams.requestJoin") }}</UiButton
            >
          </div>
        </div>
        <p
          v-if="isOwner && !canPlay"
          class="relative mt-5 rounded-xl bg-amber-50 px-4 py-2.5 text-sm text-amber-700"
        >
          {{ $t("teams.needPlayers", { n: PLAYERS_PER_MATCH, have: members.length }) }}
        </p>
        <p
          v-else-if="myOtherTeam"
          class="relative mt-5 rounded-xl bg-ink-50 px-4 py-2.5 text-sm text-ink-600"
        >
          {{ $t("teams.oneTeamNote") }}
          <NuxtLink
            :to="`/teams/${myOtherTeam.id}`"
            class="font-semibold text-brand-600 hover:text-brand-700"
            >{{ myOtherTeam.name }}</NuxtLink
          >
        </p>
        <p
          v-else-if="hasRequested"
          class="relative mt-5 rounded-xl bg-amber-50/70 px-4 py-2.5 text-sm text-amber-700"
        >
          {{ $t("teams.requestedNote") }}
        </p>
      </section>

      <UiTabs v-model="tab" :tabs="tabs" class="mb-6" />

      <!-- Squad -->
      <div v-if="tab === 'squad'" class="card divide-y divide-ink-100">
        <div v-for="m in members" :key="m.userId" class="flex items-center gap-3 px-5 py-3.5">
          <UiAvatar :name="m.displayName" />
          <div class="min-w-0 flex-1">
            <p class="flex items-center gap-2 truncate font-medium text-ink-800">
              {{ m.displayName }}
              <span v-if="m.userId === auth.userId" class="chip bg-ink-100 text-ink-600">{{
                $t("common.you")
              }}</span>
            </p>
            <p class="text-xs text-ink-400">
              {{ $t("teams.joinedAgo", { time: relative(m.joinedAt) }) }}
            </p>
          </div>
          <span v-if="m.role === 'Captain'" class="chip bg-amber-50 text-amber-700"
            ><SvgoCrown class="text-xs" /> {{ $t("roles.Captain") }}</span
          >
          <span v-else class="chip bg-ink-50 text-ink-500">{{ $t("roles.Player") }}</span>
          <button
            v-if="isOwner && m.userId !== auth.userId"
            class="rounded-lg p-2 text-ink-300 transition hover:bg-rose-50 hover:text-rose-500"
            :title="$t('teams.kick')"
            @click="kickTarget = { userId: m.userId, name: m.displayName }"
          >
            <SvgoX class="text-base" />
          </button>
        </div>
      </div>

      <!-- Join requests (owner only) -->
      <div v-else-if="tab === 'requests'">
        <div v-if="requests.length" class="card divide-y divide-ink-100">
          <JoinRequestRow v-for="r in requests" :key="r.userId" :request="r" />
        </div>
        <UiEmptyState
          v-else
          :title="$t('teams.requestsEmpty')"
          :text="$t('teams.requestsEmptyText')"
        >
          <template #icon><SvgoInbox class="text-2xl" /></template>
        </UiEmptyState>
        <p v-if="isFull" class="mt-4 rounded-xl bg-amber-50 px-4 py-2.5 text-sm text-amber-700">
          {{ $t("teams.fullNote", { max: MAX_SQUAD_SIZE }) }}
        </p>
      </div>

      <!-- Upcoming -->
      <div v-else-if="tab === 'upcoming'">
        <div v-if="upcoming.length" class="grid grid-cols-1 gap-3 md:grid-cols-2">
          <MatchCard v-for="m in upcoming" :key="m.id" :match="m" />
        </div>
        <UiEmptyState v-else :title="$t('teams.noUpcoming')" />
      </div>

      <!-- Results -->
      <div v-else>
        <div v-if="past.length" class="space-y-3">
          <div v-for="m in past" :key="m.id" class="flex items-center gap-3">
            <span
              class="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl text-sm font-bold"
              :class="
                {
                  W: 'bg-brand-100 text-brand-700',
                  L: 'bg-rose-100 text-rose-600',
                  D: 'bg-ink-100 text-ink-600',
                }[resultFor(m)]
              "
              >{{ $t(`teams.result${resultFor(m)}`) }}</span
            >
            <MatchCard :match="m" class="flex-1" />
          </div>
        </div>
        <UiEmptyState v-else :title="$t('teams.noResults')" />
      </div>

      <!-- Modals -->
      <UiModal v-model="editOpen" :title="$t('teams.editTitle')">
        <TeamForm :initial="team" :submit-label="$t('common.save')" @submit="save" />
      </UiModal>

      <UiModal
        v-model="deleteOpen"
        :title="$t('teams.deleteTitle')"
        :description="$t('teams.deleteText', { name: team.name })"
        size="sm"
      >
        <template #footer>
          <UiButton variant="secondary" @click="deleteOpen = false">{{
            $t("common.cancel")
          }}</UiButton>
          <UiButton variant="danger" @click="remove">{{ $t("common.delete") }}</UiButton>
        </template>
      </UiModal>

      <UiModal
        v-model="kickOpen"
        :title="$t('teams.kickTitle')"
        :description="$t('teams.kickText', { name: kickTarget?.name })"
        size="sm"
      >
        <template #footer>
          <UiButton variant="secondary" @click="kickTarget = null">{{
            $t("common.cancel")
          }}</UiButton>
          <UiButton variant="danger" @click="kick">{{ $t("teams.kick") }}</UiButton>
        </template>
      </UiModal>
    </template>

    <UiEmptyState v-else :title="$t('errors.notFound')">
      <UiButton to="/teams" size="sm">{{ $t("nav.teams") }}</UiButton>
    </UiEmptyState>
  </div>
</template>
