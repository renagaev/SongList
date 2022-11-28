<template>
  <v-list dense>
    <v-list-item-group>
      <template v-for="tag in tags">
        <v-list-item  :key="tag" @click="setTag(tag)">
          <v-list-item-title v-text="tag"></v-list-item-title>
        </v-list-item>
        <v-divider :key="tag+'divider'"></v-divider>
      </template>

    </v-list-item-group>
  </v-list>
</template>

<script lang="ts">
import Vue from 'vue'
import Component from 'vue-class-component'

@Component
export default class TagList extends Vue {

  created(){
    this.$store.commit("setMainTitle", "Категории")
  }
  setTag(tag: string) {
    this.$store.commit("setTag", tag)
    this.$router.push("/")
  }

  get tags() {
    return this.$store.getters["tags"]
  }
}
</script>

<style scoped>

</style>