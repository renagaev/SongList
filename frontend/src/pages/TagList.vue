<template>
  <v-list dense>
    <template v-for="tag in tags" :key="tag">
      <v-list-item @click="setTag(tag)">
        <v-list-item-title v-text="tag"></v-list-item-title>
      </v-list-item>
      <v-divider></v-divider>
    </template>
  </v-list>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useRouter } from "vue-router";
import { useStore } from "vuex";

// Vuex Store и Router
const store = useStore();
const router = useRouter();

// Computed
const tags = computed(() => store.getters["tags"]);

// Methods
const setTag = (tag: string) => {
  router.push({ name: "Home", query: { tag } });
};
</script>

<style scoped>
.v-list.theme--dark {
  background-color: rgb(18, 18, 18);
}
</style>
