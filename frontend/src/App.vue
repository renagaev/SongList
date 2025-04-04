<template>
  <v-app>
    <PWAPrompt
        :times-to-show="3"
        copyTitle="Добавить на главный экран"
        copyDescription="Этот сайт может быть приложением. Добавь его на главный экран, чтобы просматривать его в оффлайн в полноэкранном режиме"
        copyShareStep="1. Нажми на кнопку 'Поделиться'"
        copyAddToHomeScreenStep="2. Нажми 'Добавить на главный экран'"
        copy-close-prompt="Отмена"
    />
    <v-navigation-drawer v-model="showBar">
      <navigation-bar></navigation-bar>
    </v-navigation-drawer>
    <app-bar></app-bar>
    <v-main>
      <router-view v-slot="{ Component }">
        <keep-alive include="MainList">
          <component :is="Component" />
        </keep-alive>
      </router-view>
    </v-main>
  </v-app>
</template>

<script setup lang="ts">
import {ref, computed, onMounted} from "vue";
import {useStore} from "vuex";
import {useTheme} from "vuetify";
import NavigationBar from "@/components/NavigationBar.vue";
import AppBar from "@/components/AppBar.vue";
import {PWAPrompt} from "vue-ios-pwa-prompt";

// Store
const store = useStore();
const theme = useTheme();

// Reactive properties
const isSearch = ref(false);

// Computed properties
const showBar = computed({
  get: () => store.state.showBar,
  set: (value: boolean) => {
    store.commit("setShowBar", value);
  },
});

// Methods
const initializeApp = () => {
  store.dispatch("loadSongs");
  store.dispatch("initializeNowOpened");
  store.dispatch("loadNotes")
  store.dispatch("checkLogin")
  theme.global.name.value = store.state.settings.darkTheme ? "dark" : "light";
};

// Lifecycle hooks
onMounted(() => {
  initializeApp();
});
</script>
