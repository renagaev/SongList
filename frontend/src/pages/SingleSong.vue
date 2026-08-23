<template>
  <v-container  @touchstart="handleTouchStart" @touchmove="handleTouchMove" @touchend="handleTouchEnd">
    <v-chip-group column>
      <v-chip
          v-for="tag in tags"
          density="compact"
          :key="tag"
          @click="goToTag(tag)">{{ tag }}
      </v-chip>
    </v-chip-group>
    <div>
      <v-btn
          class="mb-3 mr-2"
          v-if="showNote"
          @click="playNote"
          rounded
      >
        <v-icon>{{ noteIcon }}</v-icon>
        {{ song.note }}
      </v-btn>
      <v-btn
          class="mb-3 mr-2"
          rounded
          @click="toggleFavourite"
      >
        <v-icon :color="isFavourite ? 'amber' : ''">
          {{ isFavourite ? favouriteIcon : unFavouriteIcon }}
        </v-icon>
      </v-btn>

      <v-btn
          class="mb-3 mr-2"
          rounded
          @click="share"
      >
        <v-icon>
          {{ shareIcon }}
        </v-icon>
      </v-btn>

      <attachments :id="id"/>

      <v-btn
          class="mb-3 mr-2"
          rounded
          @click="goToEdit"
          v-if="isAdmin">
        <v-icon>{{ editIcon }}</v-icon>
      </v-btn>

      <v-btn
          class="mb-3 mr-2"
          rounded
          @click="openDeleteDialog"
          v-if="isAdmin">
        <v-icon>{{ deleteIcon }}</v-icon>
      </v-btn>
    </div>
    <v-dialog v-model="deleteDialog" max-width="420">
      <v-card class="pa-2">
        <v-card-title class="px-4 pt-4 pb-2">Удаление песни</v-card-title>
        <v-card-text class="px-4 pb-2">Удалить песню «{{ song.title }}»?</v-card-text>
        <v-card-actions class="justify-end ga-2 px-4 pb-4">
          <v-btn variant="text" @click="deleteDialog = false">Отмена</v-btn>
          <v-btn color="red" @click="confirmDelete">Удалить</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
    <p v-if="showHistory && lastSingedText" v-html="lastSingedText"></p>
    <v-divider style="margin: 10px 0"></v-divider>
    <div ref="textContainer" class="words" :style="fontStyle" v-html="formattedText"></div>
  </v-container>
</template>

<script setup lang="ts">
import {ref, computed, onMounted, onBeforeMount, onUnmounted, onActivated} from "vue";
import {onBeforeRouteLeave, useRoute, useRouter} from "vue-router";
import {mdiMusicNote, mdiStar, mdiStarOutline, mdiPencil, mdiShareVariant, mdiDelete} from "@mdi/js";
import Piano from "@/services/piano";
import {daysAgo, formatDays} from "@/services/DateHelper";
import {useStore} from "vuex";
import Attachments from "@/components/Attachments.vue";
defineOptions({
  name: 'SingleSong'
});
const store = useStore()

// Pinch-to-zoom state
const initialPinchDistance = ref<number | null>(null);
const initialFontSize = ref(0);
const textContainer = ref<HTMLElement | null>(null);

const getPinchDistance = (e: TouchEvent): number => {
  if (e.touches.length < 2) return 0;
  const dx = e.touches[0].clientX - e.touches[1].clientX;
  const dy = e.touches[0].clientY - e.touches[1].clientY;
  return Math.sqrt(dx * dx + dy * dy);
};

const handleTouchStart = (e: TouchEvent) => {
  if (e.touches.length === 2) {
    initialPinchDistance.value = getPinchDistance(e);
    initialFontSize.value = store.state.settings.fontSize;
  }
};

const handleTouchMove = (e: TouchEvent) => {
  if (e.touches.length === 2 && initialPinchDistance.value !== null) {
    e.preventDefault();
    const currentDistance = getPinchDistance(e);
    const scale = currentDistance / initialPinchDistance.value;
    const newFontSize = Math.max(10, Math.min(48, Math.round(initialFontSize.value * scale)));
    store.commit("setFontSize", newFontSize);
  }
};

const handleTouchEnd = () => {
  initialPinchDistance.value = null;
};

onUnmounted(() => {
  initialPinchDistance.value = null;
});
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
const editIcon = mdiPencil
const shareIcon = mdiShareVariant
const deleteIcon = mdiDelete


// Refs and route
const route = useRoute();
const router = useRouter();
const history = ref<Date[]>([]);
const deleteDialog = ref(false);

const isFavourite = computed(() => store.state.favourites.includes(props.id));
const isAdmin = computed(() => store.state.isAdmin);

const lastSingedText = computed(() => {
  const morning = history.value.find((x) => x.getHours() < 16);
  const evening = history.value.find((x) => x.getHours() > 16);

  const dates = [];
  if (morning) {
    dates.push(
        `утром ${formatDays(daysAgo(morning))}, ${morning.toLocaleDateString()}`
    );
  }
  if (evening) {
    dates.push(
        `вечером ${formatDays(daysAgo(evening))}, ${evening.toLocaleDateString()}`
    );
  }
  return dates.length > 0 ? "Пели " + dates.join(",<br/>") : null;
});

const fontStyle = computed(() => `font-size: ${store.state.settings.fontSize}px`);

const formattedText = computed(() => {
  if (!song.value?.text) return '';
  // Заменяем пробел перед тире на неразрывный пробел
  return song.value.text
    .replace(/ - /g, ' \u2014 ')
    .replace(/ — /g, ' \u2014 ')
    .replace(/ – /g, ' \u2013 ')
    .replace(/([^ \u00A0]) \u2014/g, '$1\u00A0\u2014')
    .replace(/([^ \u00A0]) \u2013/g, '$1\u00A0\u2013')
    .replace(/([^ \u00A0]) - /g, '$1\u00A0- ');
});

const showHistory = computed(() => store.state.settings.showHistory);

const song = computed(() => store.state.songs.find(x => x.id == props.id));
const tags = computed(() => song.value.tags.sort((a, b) => {
  return a.toLowerCase().localeCompare(b.toLowerCase())
}));
const showNote = computed(() =>
    store.state.settings.playNotes && song.value.noteId != null
);
const note = computed(() => store.state.notes.find((x) => x.id === song.value.noteId));

// Methods
const toggleFavourite = () => {
  store.commit("toggleFavourite", props.id);
};

const playNote = () => {
  Piano.play(note.value.name);
};

const goToTag = (tag: string) => {
  router.push({name: "Home", query: {tag}});
};
const goToEdit = () => {
  router.push({name: "EditSong", params: {id: props.id}})
}

const openDeleteDialog = () => {
  deleteDialog.value = true
}

const confirmDelete = async () => {
  await store.dispatch("deleteSong", props.id)
  deleteDialog.value = false
  router.back()

}

const share = () => {
  navigator.share({
    title: song.value.title,
    url: window.location.href,
  });
}

onActivated(async () => {
  if (showHistory.value) {
    history.value = await store.dispatch("getSongHistory", props.id);
  }
})
onBeforeMount(async () => {
  if (showHistory.value) {
    history.value = await store.dispatch("getSongHistory", props.id);
  }
})
// Lifecycle hooks
onMounted(async () => {
  await store.dispatch("songOpened", props.id);
});

onBeforeRouteLeave((to, from, next) => {
  store.dispatch("songClosed", props.id);
  next();
});
</script>

<style scoped>
.words {
  white-space: pre-wrap;
  overflow-wrap: normal;
  word-break: keep-all;
}
</style>
