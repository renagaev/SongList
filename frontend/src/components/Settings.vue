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

</style>