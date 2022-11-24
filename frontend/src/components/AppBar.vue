<template>
  <div st>
    <v-app-bar-nav-icon @click="showBar=!showBar"></v-app-bar-nav-icon>

    <v-toolbar-title v-if="!isSearch" v-text="title"></v-toolbar-title>

    <v-spacer></v-spacer>
    <v-btn icon @click="isSearch=!isSearch">
      <v-icon>mdi-magnify</v-icon>
    </v-btn>
  </div>
</template>

<script lang="ts">
import Vue from 'vue'
import Component from 'vue-class-component'

@Component
export default class AppBar extends Vue {
  isSearch=false
  showBar=false
  get title() {
    return this.$store.state.title
  }
  
}
</script>

<style scoped>

</style>