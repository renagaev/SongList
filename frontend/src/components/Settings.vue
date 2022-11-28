<template>
  <v-list>
    <v-list-item>
      <v-list-item-action>
        <v-checkbox v-model="darkTheme"></v-checkbox>
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

@Component
export default class Settings extends Vue {
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