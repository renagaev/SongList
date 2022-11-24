<template>
  <div>
    <v-list nav dense>
      <v-list-item-group>
        <v-list-item @click="setTag(null)">
          <v-list-item-title>Все</v-list-item-title>
        </v-list-item>
        <v-list-item v-for="tag in tags" :key="tag" @click="setTag(tag)">
          <v-list-item-title v-text="tag"></v-list-item-title>
        </v-list-item>
      </v-list-item-group>
    </v-list>
  </div>
</template>

<script lang="ts">
import Vue from 'vue'
import Component from 'vue-class-component'

@Component
export default class NavigationBar extends Vue {
  get tags(): string[] {
    return this.$store.getters["tags"];
  }

  setTag(tag: string){
    this.$store.commit("setTag", tag)
    this.$router.push("/")
  }

}
</script>

<style scoped>

</style>