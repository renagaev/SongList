<template>
  <v-app>

    <v-navigation-drawer v-model="showBar" clipped app>
      <navigation-bar></navigation-bar>
    </v-navigation-drawer>
    <v-app-bar app clipped-left dense>  

      <v-app-bar-nav-icon @click="showBar=!showBar"></v-app-bar-nav-icon>

      <v-toolbar-title v-if="!isSearch" v-text="title"></v-toolbar-title>
      <search v-else></search>

      <v-spacer></v-spacer>
      <v-btn icon @click="isSearch=!isSearch">
        <v-icon>mdi-magnify</v-icon>
      </v-btn>
      
    </v-app-bar>

    <v-main>
      <keep-alive include="SongList">
        <router-view/>
      </keep-alive>
    </v-main>
  </v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import NavigationBar from "@/components/NavigationBar.vue";
import AppBar from "@/components/AppBar.vue";
import {Component} from 'vue-property-decorator';
import Search from "@/components/Search.vue";

@Component({
  components: {
    Search,
    NavigationBar,
    AppBar
  },
})
export default class App extends Vue {
  showBar = false
  isSearch = false

  async created() {
    await this.$store.dispatch("loadSongs")
  }

  get title() {
    return this.$store.getters["title"]
  }
}
</script>
