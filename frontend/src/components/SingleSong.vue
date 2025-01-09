<template>
  <v-container>
    <v-chip-group column>
      <v-chip
          color="secondary"
          :ripple="false"
          small
          v-for="tag in song.tags"
          :key="tag"
          v-text="tag"
          @click="goToTag(tag)"
      />
    </v-chip-group>
    <div>
      <v-btn
          class="mb-3 mr-2"
          v-if="showNote"
          @click="playNote"
          color="secondary"
          rounded
      >
        <v-icon>{{ noteIcon }}</v-icon>
        {{ song.note }}
      </v-btn>
      <v-btn
          class="mb-3 mr-2"
          rounded
          @click="toggleFavourite"
          color="secondary"
      >
        <v-icon :color="isFavourite ? 'amber' : ''">
          {{ isFavourite ? favouriteIcon : unFavouriteIcon }}
        </v-icon>
      </v-btn>
    </div>
    <p v-if="showHistory && lastSingedText" v-html="lastSingedText"></p>
    <v-divider style="margin-bottom: 10px"></v-divider>
    <div v-text="song.text" class="words" :style="fontStyle"></div>
  </v-container>
</template>

<script setup lang="ts">
import {ref, computed, onMounted} from "vue";
import {onBeforeRouteLeave, useRoute, useRouter} from "vue-router";
import {mdiMusicNote, mdiStar, mdiStarOutline} from "@mdi/js";
import Piano from "@/services/piano";
import {daysAgo} from "@/services/DateHelper";
import {useStore} from "vuex";

const store = useStore()
// Props
const props = defineProps({
  id: {
    type: Number,
    required: true,
  },
});

// Icons
const noteIcon = mdiMusicNote;
const favouriteIcon = mdiStar;
const unFavouriteIcon = mdiStarOutline;

// Refs and route
const route = useRoute();
const router = useRouter();
const history = ref<Date[]>([]);

const isFavourite = computed(() => store.state.favourites.includes(route.params.id));

const lastSingedText = computed(() => {
  const morning = history.value.find((x) => x.getHours() < 16);
  const evening = history.value.find((x) => x.getHours() > 16);

  const dates = [];
  if (morning) {
    dates.push(
        `утром ${morning.toLocaleDateString()} (${daysAgo(morning)} дней)`
    );
  }
  if (evening) {
    dates.push(
        `вечером ${evening.toLocaleDateString()} (${daysAgo(evening)} дней)`
    );
  }
  return dates.length > 0 ? "Пели " + dates.join(",<br/>") : null;
});

const fontStyle = computed(() => `font-size: ${store.state.settings.fontSize}px`);

const showHistory = computed(() => store.state.settings.showHistory);

const song = computed(() => store.state.selectedSong);

const showNote = computed(() =>
    store.state.settings.playNotes && Piano.canPlay(song.value.note)
);

// Methods
const toggleFavourite = () => {
  store.commit("toggleFavourite", route.params.id);
};

const playNote = () => {
  Piano.play(song.value.note);
};

const loadHistory = async () => {
  if (showHistory.value) {
    history.value = await store.dispatch("getSongHistory", route.params.id);
  }
};

const goToTag = (tag: string) => {
  router.push({name: "Home", query: {tag}});
};

// Lifecycle hooks
onMounted(async () => {
  store.commit("selectSong", route.params.id);
  await loadHistory();
  await store.dispatch("songOpened", route.params.id);
});

onBeforeRouteLeave((to, from, next) => {
  store.dispatch("songClosed", route.params.id);
  next();
});
</script>

<style scoped>
.words {
  white-space: pre-line;
}
</style>
