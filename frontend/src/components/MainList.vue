<template>
  <song-list :songs="songs" :scroll-key="tag">

  </song-list>
</template>

<script lang="ts">
import Vue from 'vue'
import Component from 'vue-class-component'
import {SongModel} from "@/store/models";
import {Prop} from 'vue-property-decorator';
import SongList from "@/components/SongList.vue";

@Component({
  name: "MainList",
  components: {SongList}
})
export default class MainList extends Vue {

  @Prop()
  tag?: string

  get songs(): SongModel[] {
    return this.$store.getters["songs"](this.tag)
  }
}
</script>

<style scoped>
</style>