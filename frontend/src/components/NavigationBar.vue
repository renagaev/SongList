<template>
  <div>
    <v-list nav>
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
import {mdiViewList, mdiTagMultiple, mdiCogs} from '@mdi/js'

@Component
export default class NavigationBar extends Vue {
  items = [
    {
      title: "Все",
      icon: mdiViewList,
      action: () => this.$router.push({name: "Home"})
    },
    {
      title: "Категории",
      icon: mdiTagMultiple,
      action: () => this.$router.push({name: "Tags"})
    },
    {
      title: "Настройки",
      icon: mdiCogs,
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