<template>
  <v-app-bar app clipped-left>

    <v-app-bar-nav-icon @click="toggleBar"></v-app-bar-nav-icon>

    <v-toolbar-title v-if="!isSearch" v-text="title"></v-toolbar-title>
    <div v-else>
      <v-text-field solo dense hide-details flat filled light
                    placeholder="Введите текст"
                    class="ml-5"
                    v-model="searchText"
                    autofocus
                    :dark="dark"
      />
    </div>

    <v-spacer></v-spacer>
    <v-btn icon @click="isSearch=!isSearch">
      <v-icon>mdi-magnify</v-icon>
    </v-btn>
  </v-app-bar>
</template>

<script lang="ts">
import Vue from 'vue'
import Component from 'vue-class-component'

@Component
export default class AppBar extends Vue {
  showBar = false

  get dark(){
    return this.$vuetify.theme.dark
  }
  get isSearch() {
    return this.$store.state.showSearch
  }

  set isSearch(value: boolean) {
    if(!value){
      this.$store.commit("setSearchText", "")
    }
    this.$store.commit("setShowSearch", value)
  }

  toggleBar() {
    this.$store.commit("setShowBar", !this.$store.state.showBar)
  }
  
  get searchText(){
    return this.$store.state.searchText
  }
  set searchText(value: string){
    if (this.$route.name != "Home") {
      this.$router.push({name: "Home"})
    }
    this.$store.commit("setSearchText", value)
  }


  get title() {
    const route = this.$route.name
    if (route == "Home") {
      return this.$route.query.tag ?? "Все"
    }
    if (route == "SingleSong") {
      const song = this.$store.state.selectedSong
      return (song.number ? song.number + '. ' : '') + song.title
    }
    if (route == "Tags") {
      return "Категории"
    }
    if (route == "Settings") {
      return "Настройки"
    }
    return "unknown"
  }
}
</script>

<style scoped>

</style>