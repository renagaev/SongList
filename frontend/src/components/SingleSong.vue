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
    <p v-if="showHistory && lastSingedText" v-html="lastSingedText">
    </p>
    <v-divider style="margin-bottom: 10px"></v-divider>
    <div v-text="song.text" class="words" :style="fontStyle"></div>
  </v-container>

</template>

<script lang="ts">
import Vue from 'vue'
import Component from 'vue-class-component'
import {Prop} from 'vue-property-decorator';
import Piano from "@/services/piano";
import {daysAgo} from "@/services/DateHelper"
import {mdiMusicNote, mdiStar, mdiStarOutline} from "@mdi/js"

Component.registerHooks([
  'beforeRouteEnter',
  'beforeRouteUpdate',
  'beforeRouteLeave'
]);
@Component
export default class SingleSong extends Vue {
  @Prop()
  id!: number
  noteIcon = mdiMusicNote
  favouriteIcon = mdiStar
  unFavouriteIcon = mdiStarOutline
  history: Date[] = []

  get isFavourite() {
    return this.$store.state.favourites.includes(this.id)
  }

  get lastSingedText() {
    const morning = this.history.find(x => x.getHours() < 16);
    const evening = this.history.find(x => x.getHours() > 16);

    const dates = []
    if (morning) {
      dates.push(`утром ${morning.toLocaleDateString()} (${daysAgo(morning)} дней)`)
    }
    if (evening) {
      dates.push(`вечером ${evening.toLocaleDateString()} (${daysAgo(evening)} дней)`)
    }
    if (dates.length > 0) {
      return "Пели " + dates.join(",<br/>")
    }
    return null
  }

  toggleFavourite() {
    this.$store.commit("toggleFavourite", this.id)
  }

  get fontStyle() {
    return `font-size: ${this.$store.state.settings.fontSize}px`
  }

  get showHistory() {
    return this.$store.state.settings.showHistory
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

  async created() {
    this.$store.commit("selectSong", this.id)
    if (this.showHistory) {
      this.history = await this.$store.dispatch("getSongHistory", this.id)
    }
  }

  async mounted() {
    await this.$store.dispatch("songOpened", this.id)
  }

  async beforeRouteLeave(to: any, from: any, next: any) {
    this.$store.dispatch("songClosed", this.id)
    await next()
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