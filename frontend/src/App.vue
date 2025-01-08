<template>
  <v-app>
    <PWAPrompt 
        :timesToShow="3"
        copyTitle='Добавить на главный экран'
        copyBody='Этот сайт может быть приложением. Добавь его на главный экран, чтобы просматривать его в оффлайн в полноэкранном режиме'
        copyShareButtonLabel='1. Нажми на кнопку "Поделиться"'
        copyAddHomeButtonLabel='2. Нажми "Добавить на главный экран"'
        copyClosePrompt="Отмена"
    />
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
import PWAPrompt from "vue2-ios-pwa-prompt"

@Component({
  components: {
    NavigationBar,
    PWAPrompt,
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

  created() {
    this.$vuetify.theme.dark = this.$store.state.settings.darkTheme
    this.$store.dispatch("loadSongs")
    this.$store.dispatch("initializeNowOpened")
  }
}
</script>
