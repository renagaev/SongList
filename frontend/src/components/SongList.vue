<template>
  <recycle-scroller
      class="scroller"
      :items="songs"
      :item-size="72"
      key-field="id"
      v-slot="{ item }"
  >
    <div class="item-wrapper">
      <v-list-item
          two-line
          @click="open(item.id)"
          class="item-entry"
          :class="getClass(item)"
      >
        <v-list-item-title>{{getTitle(item)}}</v-list-item-title>
        <v-list-item-subtitle>{{getSubtitle(item)}}</v-list-item-subtitle>
      </v-list-item>
    </div>
  </recycle-scroller>
</template>

<script setup lang="ts">
import { ref, computed, onActivated, onDeactivated } from "vue";
import type {PropType} from "vue";
import { useRouter } from "vue-router";
import { SongModel } from "@/store/models";
import {useTheme} from 'vuetify'

// Props
const props = defineProps({
  songs: {
    type: Array as () => SongModel[],
    required: true,
  },
  subtitle: {
    type: Function as PropType<(song: SongModel) => string>,
    required: false,
  },
  scrollKey: {
    type: String,
    required: false,
  }
});

// Refs
const scrollTop = ref(0);
const lastScrollKey = ref<string | undefined>(undefined);
const scroll = ref<HTMLElement | null>(null);
const theme = useTheme();
// Router
const router = useRouter();

// Computed
const scrollRef = computed(() => scroll.value);
const computedScrollKey = computed(() => props.scrollKey ?? props.songs?.map(x=> x.id).join(""))

// Methods
const getTitle = (song: SongModel): string => {
  return song.number ? `${song.title} | ${song.number}` : song.title;
};

const getSubtitle = (song: SongModel): string => {
  if (props.subtitle) {
    return props.subtitle(song);
  }
  return song.text;
};

const getClass = (song: SongModel): string => {
  if (song.opened) {
    return theme.isDark ? "opened-dark" : "opened";
  }
  return "";
};

const open = (id: number) => {
  router.push(`/song/${id}`);
};

// Lifecycle hooks
onActivated(() => {
  if (scroll.value && scrollTop.value > 0) {
    // Ждём следующего тика + небольшая задержка для recycle-scroller
    setTimeout(() => {
      if (scroll.value) {
        scroll.value.scrollTop = scrollTop.value;
        console.log("scroll restored to", scrollTop.value);
      }
    }, 50);
  }
});

onDeactivated(() => {
  if (scroll.value) {
    scrollTop.value = scroll.value.scrollTop;
  }
});
</script>

<style scoped>

.v-list-item-subtitle {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  overflow-wrap: break-word;
}
.scroller {
  height: calc(100vh - 64px);
}

.item-wrapper {
  height: 72px;
}

.item-entry {
  height: 100%;
  box-sizing: border-box;
  border-bottom: 1px solid rgba(var(--v-theme-on-surface), 0.12);
}

.opened-dark {
  background-color: #424242;
}

.opened {
  background-color: #BDBDBD;
}
</style>
