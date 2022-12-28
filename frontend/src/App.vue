<template>
  <v-app>

    <v-navigation-drawer v-model="showBar" clipped app>
      <navigation-bar></navigation-bar>
    </v-navigation-drawer>
    <app-bar></app-bar>

    <v-main>
      <keep-alive include="MainList">
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

@Component({
  components: {
    NavigationBar,
    AppBar
  },
})
export default class App extends Vue {
  isSearch = false

  get showBar() {
    return this.$store.state.showBar
  }

  set showBar(value) {
    this.$store.commit("setShowBar", value)
  }

  async created() {
    this.$vuetify.theme.dark = this.$store.state.settings.darkTheme
    await this.$store.dispatch("loadSongs")
  }
}
</script>
