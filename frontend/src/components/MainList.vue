<template>
  <song-list :songs="songs" :scroll-key="tag"></song-list>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useStore } from "vuex";
import SongList from "@/components/SongList.vue";

// Props
defineProps({
  tag: {
    type: String,
    required: false,
  },
});

// Store
const store = useStore();

// Computed properties
const songs = computed(() => store.getters["songs"](tag));
</script>

<style scoped>
</style>
