<template>
  <v-list>

    <v-list-item>
      <v-list-item-action>
        <v-checkbox v-model="darkTheme" :on-icon="checkBoxOn" :off-icon="checkBoxOff"></v-checkbox>
      </v-list-item-action>
      <v-list-item-content>
        <v-list-item-title>Темная тема</v-list-item-title>
      </v-list-item-content>
    </v-list-item>

    <v-divider/>

    <v-list-item>
      <v-list-item-action>
        <v-checkbox v-model="playNotes" :on-icon="checkBoxOn" :off-icon="checkBoxOff"></v-checkbox>
      </v-list-item-action>
      <v-list-item-content>
        <v-list-item-title>Показывать ноты</v-list-item-title>
      </v-list-item-content>
    </v-list-item>

    <v-list-item>
      <v-list-item-action>
        <v-checkbox v-model="showHistory" :on-icon="checkBoxOn" :off-icon="checkBoxOff"></v-checkbox>
      </v-list-item-action>
      <v-list-item-content>
        <v-list-item-title>Показывать историю песен</v-list-item-title>
      </v-list-item-content>
    </v-list-item>

    <v-divider/>

    <v-list-item>
      <v-list-item-content>
        <v-list-item-title class="pl-0">Размер текста песен: {{ fontSize }}</v-list-item-title>
        <v-container>
          <v-slider min="12" max="50" v-model="fontSize"></v-slider>
        </v-container>
        <div :style="fontStyle">Пример текста</div>
      </v-list-item-content>
    </v-list-item>
    <v-divider/>
  </v-list>

</template>

<script lang="ts">
import Vue from 'vue'
import Component from 'vue-class-component'
import {mdiCheckboxBlankOutline, mdiCheckboxMarked} from "@mdi/js"

@Component
export default class Settings extends Vue {
  checkBoxOff = mdiCheckboxBlankOutline
  checkBoxOn = mdiCheckboxMarked
  fontSize = 0

  get fontStyle() {
    return `font-size: ${this.fontSize}px`
  }

  created() {
    this.fontSize = this.$store.state.settings.fontSize
  }

  destroyed() {
    this.$store.commit("setFontSize", this.fontSize)
  }

  get playNotes() {
    return this.settings.playNotes
  }

  set playNotes(value: boolean) {
    this.$store.commit("setPlayNotes", value)
  }

  get showHistory() {
    return this.settings.showHistory
  }

  set showHistory(value: boolean) {
    this.$store.commit("setShowHistory", value)
  }

  get settings() {
    return this.$store.state.settings
  }

  get darkTheme() {
    return this.settings.darkTheme
  }

  set darkTheme(value: boolean) {
    this.$store.commit("setDarkTheme", value)
    this.$vuetify.theme.dark = this.$store.state.settings.darkTheme
  }

}
</script>

<style scoped>
.v-list.theme--dark {
  background-color: rgb(18, 18, 18);
}
</style>