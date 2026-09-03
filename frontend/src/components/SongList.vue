<template>
  <recycle-scroller
      class="scroller"
      :items="songs"
      :item-size="72"
      key-field="id"
      v-slot="{ item }"
      ref="scroll"
      @scroll="handleScroll"
  >
    <div class="item-wrapper">
      <v-list-item
          two-line
          @click="open(item.id)"
          class="item-entry"
          :class="getClass(item)"
      >
        <div class="item-content">
          <div class="title-row">
            <v-list-item-title>{{ getTitle(item) }}</v-list-item-title>
            <span
                v-if="item.number != null"
                class="song-number"
            >| {{ item.number }}</span>
          </div>
          <v-list-item-subtitle>{{ getSubtitle(item) }}</v-list-item-subtitle>
        </div>
      </v-list-item>
    </div>
  </recycle-scroller>
</template>

<script setup lang="ts">
import {ref, computed, nextTick, onActivated, onDeactivated, onUnmounted, useTemplateRef} from "vue";
import type {PropType} from "vue";
import {useRouter} from "vue-router";
import {SongModel} from "@/store/models";
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
const scroll = useTemplateRef("scroll")
const theme = useTheme();
// Router
const router = useRouter();

// Methods
const getTitle = (song: SongModel): string => {
  return song.title;
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

const handleScroll = (e) => {
  scrollTop.value = e.target.scrollTop
}

// Lifecycle hooks
onActivated(() => {
  if (!scroll.value || scrollTop.value <= 0) {
    return;
  }
  // RecycleScroller recalculates its real height on the next animation frame
  // after becoming visible again (see handleVisibilityChange in vue-virtual-scroller).
  // Restoring the scroll position before that happens gets clamped back to 0.
  nextTick(() => {
    requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        scroll.value?.scrollToPosition(scrollTop.value);
      });
    });
  });
});
</script>

<style scoped>

.v-list-item-title {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.v-list-item-subtitle {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  overflow-wrap: break-word;
}

.item-content {
  width: 100%;
  height: 100%;
}

.title-row {
  display: flex;
  align-items: baseline;
  min-width: 0;
}

.title-row .v-list-item-title {
  min-width: 0;
}

.song-number {
  flex-shrink: 0;
  margin-left: 4px;
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
