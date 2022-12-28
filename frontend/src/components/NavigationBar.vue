<template>
  <div>
    <v-list nav shaped>
      <v-list-item-group>
        <template v-for="(item, idx) in items">
          <v-list-item :key="idx" @click="item.action()">
            <v-list-item-icon class="ml-2">
              <v-icon>{{ item.icon }}</v-icon>
            </v-list-item-icon>
            <v-list-item-title>{{ item.title }}</v-list-item-title>
            <v-divider></v-divider>
          </v-list-item>
          
        </template>
      </v-list-item-group>
    </v-list>
  </div>
</template>

<script lang="ts">
import Vue from 'vue'
import Component from 'vue-class-component'
import {mdiViewList, mdiTagMultiple, mdiCogs, mdiDownload, mdiStar} from '@mdi/js'
import install from "@/services/installPrompt"

type NavItem = {
  action(): void,
  icon: string
  title: string
}
@Component
export default class NavigationBar extends Vue {

  items: NavItem[] = [
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
      title: "Избранные",
      icon: mdiStar,
      action: () => this.$router.push({name: "Favourites"})
    },
    {
      title: "Настройки",
      icon: mdiCogs,
      action: () => this.$router.push({name: "Settings"})
    },
  ]

  installNav: NavItem = {
    action: async () => {
      await install.trigger();
      if (!install.isAvailable()) {
        this.items.splice(this.items.findIndex(x => x.title == "Установить"), 1)
      }
    },
    icon: mdiDownload,
    title: "Установить"
  }

  async addInstallButton() {
    await install.waitPrompt()
    this.items.push(this.installNav)
  }

  created() {
    this.addInstallButton()
  }

  get tags(): string[] {
    return this.$store.getters["tags"];
  }
}
</script>

<style scoped>

</style>