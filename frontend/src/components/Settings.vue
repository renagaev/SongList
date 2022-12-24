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
        <v-list-item-content>
          <v-list-item-title class="pl-0">Размер текста песен: {{fontSize}}</v-list-item-title>
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

  get fontStyle(){
    return `font-size: ${this.fontSize}px`
  }
  get fontSize(){
    return this.settings.fontSize
  }
  set fontSize(value: number){
    this.$store.commit("setFontSize", value)
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

</style>