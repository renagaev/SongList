﻿<template>
  <v-list dense>
    <template v-for="tag in tags">
      <v-list-item :key="tag" @click="setTag(tag)">
        <v-list-item-title v-text="tag"></v-list-item-title>
      </v-list-item>
      <v-divider :key="tag+'divider'"></v-divider>
    </template>
  </v-list>
</template>

<script lang="ts">
import Vue from 'vue'
import Component from 'vue-class-component'

@Component
export default class TagList extends Vue {

  setTag(tag: string) {
    this.$router.push({name: "Home", query: {tag: tag}})
  }

  get tags() {
    return this.$store.getters["tags"]
  }
}
</script>

<style scoped>
.v-list.theme--dark{
  background-color: rgb(18, 18, 18);
}
</style>