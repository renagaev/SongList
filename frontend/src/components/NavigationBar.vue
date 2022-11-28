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
      action: () => this.$router.push({name: "Home"})
    },
    {
      title: "Категории",
      icon: "mdi-tag-multiple",
      action: () => this.$router.push({name: "Tags"})
    },
    {
      title: "Настройки",
      icon: "mdi-cogs",
      action: () => this.$router.push({name: "Settings"})
    }
  ]

  get tags(): string[] {
    return this.$store.getters["tags"];
  }
}
</script>

<style scoped>

</style>