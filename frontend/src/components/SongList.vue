<template>
  <recycle-scroller
      class="scroller"
      :items="songs"
      :item-size="64"
      key-field="id"
      v-slot="{ item }"
      ref="scroll"
  >
    <div class="item">
      <v-list-item
          two-line
          @click="open(item.id)"
          :class="'item ' + getClass(item)"
      >
        <v-list-item-title>{{getTitle(item)}}</v-list-item-title>
        <v-list-item-subtitle>{{item.text}}</v-list-item-subtitle>
      </v-list-item>
      <v-divider />
    </div>
  </recycle-scroller>
</template>

<script setup lang="ts">
import { ref, computed, onActivated, onDeactivated } from "vue";
import { useRouter } from "vue-router";
import { SongModel } from "@/store/models";
import {useTheme} from 'vuetify'

// Props
defineProps({
  scrollKey: {
    type: String,
    required: false,
  },
  songs: {
    type: Array as () => SongModel[],
    required: true,
  },
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

// Methods
const getTitle = (song: SongModel): string => {
  return song.number ? `${song.title} | ${song.number}` : song.title;
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
  if (lastScrollKey.value === scrollKey && scrollRef.value) {
    scrollRef.value.scrollTop = scrollTop.value;
  }
});

onDeactivated(() => {
  if (scrollRef.value) {
    scrollTop.value = scrollRef.value.scrollTop;
    lastScrollKey.value = scrollKey;
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

.item {
  height: 64px;
}

.opened-dark {
  background-color: #424242;
}

.opened {
  background-color: #BDBDBD;
}
</style>
