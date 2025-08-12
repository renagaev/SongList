<template>
  <v-app-bar app clipped-left>
    <v-app-bar-nav-icon @click="toggleBar">
      <v-icon>{{ menuIcon }}</v-icon>
    </v-app-bar-nav-icon>

    <v-toolbar-title v-if="!isSearch">{{title}}</v-toolbar-title>
    <v-text-field v-else
        variant="solo-filled"
        density="compact"
        hide-details
        flat
        light
        placeholder="Введите текст"
        class="ml-5"
        style="padding-right: 10px"
        v-model="searchText"
        autofocus
        clearable
    />
    <v-btn icon @click="isSearch = !isSearch">
      <v-icon>{{ searchIcon }}</v-icon>
    </v-btn>
  </v-app-bar>
</template>

<script setup lang="ts">
import {ref, computed, watch, onMounted} from 'vue';
import {mdiMagnify, mdiMenu} from "@mdi/js";
import {useRoute, useRouter} from 'vue-router';
import {SongModel} from "@/store/models";
import { useStore } from "vuex";

// Ссылки и переменные
const route = useRoute();
const router = useRouter();
const searchIcon = ref(mdiMagnify);
const menuIcon = ref(mdiMenu);
const title = ref("Все");
const isSearch = ref(false);
const store = useStore()

// Геттеры и сеттеры
const searchText = computed({
  get: (): string => store.state.searchText,
  set: (value: string) => {
    value ||= "";
    if (route.name !== "Home") {
      router.push({name: "Home"});
    }
    store.commit("setSearchText", value);
  },
});

// Методы
const toggleBar = () => {
  store.commit("setShowBar", !store.state.showBar);
};

const updateState = () => {
  const routeName = route.name;

  if (routeName === "Home") {
    const tag = route.query.tag as string;
    if (tag) {
      searchText.value = "";
      title.value = tag;
    } else if (searchText.value) {
      isSearch.value = true;
    } else {
      title.value = "Все";
    }
    return;
  }

  if (routeName === "SingleSong" || routeName === "EditSong") {
    const idParam = route.params.id;
    const songId = Array.isArray(idParam) ? Number.parseInt(idParam[0]) : Number.parseInt(idParam);
    const song = store.state.songs.find((x: SongModel) => x.id === songId);
    title.value = (song?.number ? `${song.number}. ` : "") + song?.title;
    isSearch.value = false;
    return;
  }

  title.value = (route.meta.title as string) ?? "Unknown";
  isSearch.value = false;
};

// Lifecycle hooks
onMounted(() => {
  store.commit("setSearchText", "");
  updateState();
});

// Наблюдатели
watch(
    () => route,
    () => {
      updateState();
    },
    {deep: true}
);
</script>

<style scoped>
</style>
