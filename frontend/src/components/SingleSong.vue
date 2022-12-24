<template>
  <v-container>
    <v-chip-group>
      <v-chip color="primary" :ripple="false" small
              v-for="tag in song.tags" :key="tag" v-text="tag"
              @click="goToTag(tag)"
      />
    </v-chip-group>
    <v-btn class="mb-3"
           v-if="showNote"
           @click="playNote"
           rounded>
      <v-icon>{{ noteIcon }}</v-icon>
      {{ song.note }}
    </v-btn>
    <div v-text="song.text" class="words" :style="fontStyle"></div>
  </v-container>

</template>

<script lang="ts">
import Vue from 'vue'
import Component from 'vue-class-component'
import {Prop} from 'vue-property-decorator';
import Piano from "@/piano/piano";
import {mdiMusicNote} from "@mdi/js"

@Component
export default class SingleSong extends Vue {
  @Prop()
  id!: number
  noteIcon = mdiMusicNote

  get fontStyle() {
    return `font-size: ${this.$store.state.settings.fontSize}px`
  }

  get song() {
    return this.$store.state.selectedSong
  }

  get showNote() {
    return this.$store.state.settings.playNotes && Piano.canPlay(this.song.note)
  }

  playNote() {
    Piano.play(this.song.note)
  }

  created(): void {
    this.$store.commit("selectSong", this.id)
  }

  goToTag(tag: string) {
    this.$router.push({name: "Home", query: {tag: tag}})
  }

}
</script>

<style scoped>
.words {
  white-space: pre-line;
}
</style>