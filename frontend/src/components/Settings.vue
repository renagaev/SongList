<template>
  <v-list>
    <v-list-item
        title="Темная тема">
      <template v-slot:prepend>
        <v-list-item-action start>
          <v-checkbox-btn v-model="darkTheme" :true-icon="checkBoxOn" :false-icon="checkBoxOff"></v-checkbox-btn>
        </v-list-item-action>
      </template>
    </v-list-item>

    <v-divider/>

    <v-list-item
        title="Показывать ноты">
      <template v-slot:prepend>
        <v-list-item-action start>
          <v-checkbox-btn :model-value="playNotes" :true-icon="checkBoxOn" :false-icon="checkBoxOff"></v-checkbox-btn>
        </v-list-item-action>
      </template>

    </v-list-item>
    <v-divider/>

    <v-list-item
        title="Показывать историю песен">
      <template v-slot:prepend>
        <v-list-item-action start>
          <v-checkbox-btn v-model="showHistory" :true-icon="checkBoxOn" :false-icon="checkBoxOff"></v-checkbox-btn>
        </v-list-item-action>
      </template>
    </v-list-item>

    <v-divider/>

    <v-list-item>
      <v-list-item-title class="pl-0">Размер текста песен: {{ fontSize }}</v-list-item-title>
      <v-container>
        <v-slider min="12" max="50" step="1" v-model="fontSize"></v-slider>
      </v-container>
      <div :style="fontStyle">Пример текста</div>
    </v-list-item>
    <v-divider/>
  </v-list>
</template>

<script setup lang="ts">
import {ref, computed, onMounted, onUnmounted} from "vue";
import {mdiCheckboxBlankOutline, mdiCheckboxMarked} from "@mdi/js";
import {useStore} from "vuex";
import {useTheme} from "vuetify";

// Icons
const checkBoxOff = mdiCheckboxBlankOutline;
const checkBoxOn = mdiCheckboxMarked;

// Store
const store = useStore();
const theme = useTheme();

// Reactive variables
const fontSize = ref(0);

// Computed properties
const fontStyle = computed(() => `font-size: ${fontSize.value}px`);

const settings = computed(() => store.state.settings);

const playNotes = computed({
  get: () => settings.value.playNotes,
  set: (value: boolean) => {
    store.commit("setPlayNotes", value);
  },
});

const showHistory = computed({
  get: () => settings.value.showHistory,
  set: (value: boolean) => {
    store.commit("setShowHistory", value);
  },
});

const darkTheme = computed({
  get: () => settings.value.darkTheme,
  set: (value: boolean) => {
    store.commit("setDarkTheme", value);
    theme.global.name.value = store.state.settings.darkTheme ? "dark" : "light";
  },
});

// Methods
const saveFontSize = () => {
  store.commit("setFontSize", fontSize.value);
};

// Lifecycle hooks
onMounted(() => {
  fontSize.value = store.state.settings.fontSize;
});

onUnmounted(() => {
  saveFontSize();
});
</script>

<style scoped>
.v-list.theme--dark {
  background-color: rgb(18, 18, 18);
}
</style>
