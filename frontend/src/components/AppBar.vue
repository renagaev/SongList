<template>
  <v-app-bar app clipped-left>

    <v-app-bar-nav-icon @click="toggleBar">
      <v-icon>{{ menuIcon }}</v-icon>
    </v-app-bar-nav-icon>

    <v-toolbar-title v-if="!isSearch" v-text="title"></v-toolbar-title>
    <div v-else>
      <v-text-field solo dense hide-details flat filled light
                    placeholder="Введите текст"
                    class="ml-5"
                    v-model="searchText"
                    :clear-icon="clearIcon"
                    autofocus
                    clearable
                    :dark="dark"
      />
    </div>

    <v-spacer></v-spacer>
    <v-btn icon @click="isSearch=!isSearch">
      <v-icon>{{ searchIcon }}</v-icon>
    </v-btn>
  </v-app-bar>
</template>

<script lang="ts">
import Vue from 'vue'
import Component from 'vue-class-component'
import {mdiMagnify, mdiMenu, mdiClose} from "@mdi/js"
import {Watch} from 'vue-property-decorator'
import {SongModel} from "@/store/models";

@Component
export default class AppBar extends Vue {
  showBar = false
  searchIcon = mdiMagnify
  clearIcon = mdiClose
  menuIcon = mdiMenu
  title = "Все"
  isSearch = false
  
  mounted(){
    this.$store.commit("setSearchText", "")
    this.updateState()
  }

  get dark() {
    return this.$vuetify.theme.dark
  }

  toggleBar() {
    this.$store.commit("setShowBar", !this.$store.state.showBar)
  }

  get searchText() {
    return this.$store.state.searchText
  }

  set searchText(value: string) {
    value ??= ""
    if (this.$route.name != "Home") {
      this.$router.push({name: "Home"})
    }
    this.$store.commit("setSearchText", value)
  }
  
  @Watch("$route", {deep: true})
  updateState() {
    const routeName = this.$route.name
    if (routeName == "Home") {
      const tag = (this.$route.query.tag as string)
      if (tag) {
        this.searchText = ""
        this.title = tag
      } else if (this.searchText) {
        this.isSearch = true
      } else {
        this.title = "Все"
      }
      return
    }
    if (routeName == "SingleSong") {
      const songId = Number.parseInt(this.$route.params.id)
      const song = this.$store.state.songs.find((x: SongModel) => x.id == songId)
      this.title = (song.number ? song.number + '. ' : '') + song.title
      this.isSearch = false
      return
    } 
    this.title = this.$route.meta?.title ?? "Unknown"
    this.isSearch = false
  }
}
</script>

<style scoped>

</style>