<template>
  <song-list :songs="songs"></song-list>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useStore } from "vuex";
import SongList from "@/components/SongList.vue";
import { SongModel } from "@/store/models";

// Store
const store = useStore();

// Computed properties
const songs = computed<SongModel[]>(() => store.getters["favourites"]);
</script>

<style scoped>
</style>
