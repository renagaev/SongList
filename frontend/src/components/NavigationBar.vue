<template>
  <div>
    <v-list nav dense>
      <v-list-item-group>
        <template v-for="(item, idx) in items">
          <v-list-item :key="idx" @click="item.action()">
            <v-list-item-icon>
              <v-icon>{{ item.icon }}</v-icon>
            </v-list-item-icon>
            <v-list-item-title>{{ item.title }}</v-list-item-title>
          </v-list-item>
          <v-divider :key="idx + 'divider'"></v-divider>
        </template>
      </v-list-item-group>
    </v-list>
  </div>
</template>

<script lang="ts">
import Vue from 'vue'
import Component from 'vue-class-component'

@Component
export default class NavigationBar extends Vue {
  items = [
    {
      title: "Все",
      icon: "mdi-view-list",
      action: this.showAllSongs
    },
    {
      title: "Категории",
      icon: "mdi-tag",
      action: this.goToTags
    },
  ]

  get tags(): string[] {
    return this.$store.getters["tags"];
  }

  showAllSongs() {
    this.$store.commit("setTag", null)
    if(this.$router.currentRoute.name != "Home"){
      this.$router.push({"name": "Home"})
    }
    
  }

  goToTags() {
    this.$router.push({name: "Tags"})
  }
}
</script>

<style scoped>

</style>