<template>
  <song-list :songs="songs">
  </song-list>
</template>

<script lang="ts">
import Vue from 'vue'
import Component from 'vue-class-component'
import {SongModel} from "@/store/models";
import SongList from "@/components/SongList.vue";

@Component({
  name: "FavouritesList",
  components: {SongList}
})
export default class FavouritesList extends Vue {
  get songs(): SongModel[] {
    return this.$store.getters["favourites"]
  }
}
</script>

<style scoped>
</style>