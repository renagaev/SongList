<template>
  <div>
    <v-expansion-panels>
      <v-expansion-panel value="history-settings" :title="'Фильтр'" class="pa-0">
        <template #text>
          <div class="history-card">
            <div>
              <div class="text-subtitle mb-2">Cлужения</div>
              <v-btn-toggle
                  v-model="historyMode"
                  mandatory
                  color="primary"
                  class="toggle-row"
                  rounded="pill"
                  group
              >
                <v-btn
                    v-for="option in modeOptions"
                    :key="option.value"
                    :value="option.value"
                    rounded="sm"
                    variant="tonal"
                    size="large"
                    class="toggle-btn"
                    :title="option.label"
                >
                  
                  <v-icon size="22">{{ option.icon }}</v-icon>{{option.label}}
                </v-btn>
              </v-btn-toggle>
            </div>

            <div>
              <div class="text-subtitle mb-2">Скрывать ранее</div>
              <v-text-field
                  v-model.number="hideDays"
                  type="number"
                  min="0"
                  variant="solo-filled"
                  density="comfortable"
                  hide-details="auto"
                  class="mt-1"
                  suffix="дней"
                  :prepend-inner-icon="mdiFilterOutline"
              ></v-text-field>
            </div>
          </div>
        </template>
      </v-expansion-panel>
    </v-expansion-panels>
    <v-progress-linear
        v-if="isLoading"
        indeterminate
        color="primary"
        class="mb-4"
    />

    <song-list
        v-if="!isLoading && historySongs.length"
        :songs="historySongs"
        :scroll-key="'history'"
        :subtitle="historySubtitle"
    />
  </div>
</template>

<script setup lang="ts">
import {computed, onMounted, ref} from "vue";
import {useStore} from "vuex";
import SongList from "@/components/SongList.vue";
import {daysAgo, formatDays} from "@/services/DateHelper";
import {HistoryMode, SongModel} from "@/store/models";
import {mdiClockTimeFourOutline, mdiWeatherNight, mdiWeatherSunny, mdiFilterOutline} from "@mdi/js"

const store = useStore();

const isLoading = ref(false);
const loadError = ref<string | null>(null);

const songs = computed(() => store.state.songs);
const rawHistory = computed(() => store.state.songLastHistory ?? []);

const historyMode = computed<HistoryMode>({
  get: () => store.state.settings.historyMode ?? "both",
  set: (value) => store.commit("setHistoryMode", value),
});

const hideDays = computed<number>({
  get: () => store.state.settings.historyHideDays ?? 0,
  set: (value: number) => {
    const normalized = Number.isFinite(value) ? Math.max(0, value) : 0;
    store.commit("setHistoryHideDays", normalized);
  },
});

const modeOptions = [
  {value: "both", label: "Все", icon: mdiClockTimeFourOutline},
  {value: "morning", label: "Утро", icon: mdiWeatherSunny},
  {value: "evening", label: "Вечер", icon: mdiWeatherNight},
];

const historyMap = computed(() => {
  const map = new Map<number, { lastMorning?: Date; lastEvening?: Date }>();
  rawHistory.value.forEach((item) => {
    if (item?.songId == null) {
      return;
    }
    map.set(item.songId, {
      lastMorning: item.lastMorning ? new Date(item.lastMorning) : undefined,
      lastEvening: item.lastEvening ? new Date(item.lastEvening) : undefined,
    });
  });
  return map;
});

type HistoryEntry = {
  song: SongModel;
  lastDate?: Date;
  lastKind?: "morning" | "evening";
  days?: number;
};

const historyData = computed<{
  songs: SongModel[];
  meta: Map<number, HistoryEntry>;
}>(() => {
  const mode = historyMode.value;
  const minDays = hideDays.value;

  const entries: HistoryEntry[] = songs.value.map((song) => {
    const songHistory = historyMap.value.get(song.id);
    const lastMorning = songHistory?.lastMorning;
    const lastEvening = songHistory?.lastEvening;

    let lastDate: Date | undefined;
    let lastKind: "morning" | "evening" | undefined;

    if (mode === "morning") {
      if (lastMorning) {
        lastDate = lastMorning;
        lastKind = "morning";
      }
    } else if (mode === "evening") {
      if (lastEvening) {
        lastDate = lastEvening;
        lastKind = "evening";
      }
    } else {
      if (lastMorning && lastEvening) {
        if (lastMorning > lastEvening) {
          lastDate = lastMorning;
          lastKind = "morning";
        } else {
          lastDate = lastEvening;
          lastKind = "evening";
        }
      } else if (lastMorning) {
        lastDate = lastMorning;
        lastKind = "morning";
      } else if (lastEvening) {
        lastDate = lastEvening;
        lastKind = "evening";
      }
    }

    const days = lastDate ? daysAgo(lastDate) : undefined;
    return {song, lastDate, lastKind, days};
  });

  const filtered = entries.filter((entry) => {
    if (!entry.lastDate) {
      return true;
    }
    return entry.days! >= minDays;
  });

  filtered.sort((a, b) => {
    if (a.lastDate && b.lastDate) {
      return b.lastDate.getTime() - a.lastDate.getTime();
    }
    if (a.lastDate) {
      return -1;
    }
    if (b.lastDate) {
      return 1;
    }
    return a.song.title.toLowerCase().localeCompare(b.song.title.toLowerCase());
  });

  const meta = new Map<number, HistoryEntry>();
  filtered.forEach((entry) => meta.set(entry.song.id, entry));

  return {
    songs: filtered.map((entry) => entry.song),
    meta,
  };
});

const historySongs = computed<SongModel[]>(() => historyData.value.songs);
const subtitleMap = computed(() => historyData.value.meta);

const historySubtitle = (song: SongModel): string => {
  const usage = subtitleMap.value.get(song.id);
  if (!usage?.lastDate) {
    return "История не найдена";
  }
  const days = usage.days ?? daysAgo(usage.lastDate);
  const dateStr = usage.lastDate.toLocaleDateString();
  const part = usage.lastKind === "morning" ? "утром " : usage.lastKind === "evening" ? "вечером " : "";
  return `${formatDays(days)}, ${part}${dateStr}`;
};

onMounted(async () => {
  isLoading.value = true;
  loadError.value = null;
  try {
    await store.dispatch("loadSongLastHistory");
  } catch (e) {
    console.error(e);
    loadError.value = "Не удалось загрузить историю песен";
  } finally {
    isLoading.value = false;
  }
});
</script>

<style scoped>
.history-card {
  display: flex;
  flex-direction: column;
  gap: 18px;
  padding-top: 0;
}

.toggle-row {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 8px;
}

.toggle-btn {
  width: 100%;
  justify-content: center;
  min-height: 20px;
}
</style>
