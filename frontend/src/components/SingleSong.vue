<template>
  <v-container>
    <v-chip-group>
      <v-chip color="primary" :ripple="false" small
              v-for="tag in song.tags" :key="tag" v-text="tag"
              @click="goToTag(tag)"
      />
    </v-chip-group>
    <v-btn class="mb-3"
        v-if="canPlay"
        @click="playNote"
        rounded>
      <v-icon>mdi-music-note</v-icon>
      {{ song.note }}
    </v-btn>
    <div v-text="song.text" class="text-body-1 words"></div>
  </v-container>

</template>

<script lang="ts">
import Vue from 'vue'
import Component from 'vue-class-component'
import {Song} from "@/client";
import {Prop} from 'vue-property-decorator';
import Piano from "@/piano/piano";

@Component
export default class SingleSong extends Vue {
  @Prop()
  id!: number
  song!: Song

  constructor() {
    super();
  }

  get canPlay() {
    return Piano.canPlay(this.song.note)
  }

  playNote() {
    Piano.play(this.song.note)
  }

  created(): void {
    this.song = this.$store.getters["song"](this.id)
    const song = this.song
    this.$store.commit("setMainTitle", (song.number ? song.number + '. ' : '') + song.title)
  }

  goToTag(tag: string) {
    this.$store.commit("setTag", tag)
    this.$router.push("/")
  }

}
</script>

<style scoped>
.words {
  white-space: pre-line;
}
</style>