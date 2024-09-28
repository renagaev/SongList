<template>
  <v-container>
    <v-chip-group column>
      <v-chip color="secondary" :ripple="false" small
              v-for="tag in song.tags" :key="tag" v-text="tag"
              @click="goToTag(tag)"
      />
    </v-chip-group>
    <div>
      <v-btn class="mb-3 mr-2"
             v-if="showNote"
             @click="playNote"
             color="secondary"
             rounded>
        <v-icon>{{ noteIcon }}</v-icon>
        {{ song.note }}
      </v-btn>
      <v-btn class="mb-3 mr-2" rounded @click="toggleFavourite" color="secondary">
        <v-icon :color="isFavourite ? 'amber' : ''">
          {{ isFavourite ? favouriteIcon : unFavouriteIcon }}
        </v-icon>
      </v-btn>
    </div>
    <div v-text="song.text" class="words" :style="fontStyle"></div>
  </v-container>

</template>

<script lang="ts">
import Vue from 'vue'
import Component from 'vue-class-component'
import {Prop} from 'vue-property-decorator';
import Piano from "@/services/piano";
import {mdiMusicNote, mdiStar, mdiStarOutline} from "@mdi/js"

@Component
export default class SingleSong extends Vue {
  @Prop()
  id!: number
  noteIcon = mdiMusicNote
  favouriteIcon = mdiStar
  unFavouriteIcon = mdiStarOutline

  get isFavourite() {
    return this.$store.state.favourites.includes(this.id)
  }

  toggleFavourite() {
    this.$store.commit("toggleFavourite", this.id)
  }

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

  created() {
    this.$store.commit("selectSong", this.id)
  }

  async mounted() {
    await this.$store.dispatch("songOpened", this.id)
  }

  async beforeDestroy() {
    await this.$store.dispatch("songClosed", this.id)
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