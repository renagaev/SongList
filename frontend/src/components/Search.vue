<template>
  <v-text-field solo dense hide-details flat filled light
                placeholder="Введите текст"
                class="ml-5"
                @input="onEdit"

  />

</template>

<script lang="ts">
import Vue from 'vue'
import Component from 'vue-class-component'
@Component
export default class Search extends Vue {

  onEdit(text: string): void {
    this.$store.commit("setSearchText", text)
  }
}
</script>

<style scoped>

</style>