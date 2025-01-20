<template>
  <div>
    <v-list nav>
      <template v-for="(item, idx) in navItems" :key="idx">
        <v-list-item
            rounded="shaped"
            density="comfortable"
            :title="item.title"
            :prepend-icon="item.icon"
            class="ml-2"
            @click="item.action">
        </v-list-item>
      </template>
    </v-list>
  </div>
</template>

<script setup lang="ts">
import {ref, onMounted} from "vue";
import {mdiViewList, mdiTagMultiple, mdiCogs, mdiDownload, mdiStar} from "@mdi/js";
import install from "@/services/installPrompt";
import {useRouter} from "vue-router";
import {useStore} from "vuex";

type NavItem = {
  action(): void;
  icon: string;
  title: string;
};

// Vue Router и Store
const router = useRouter();
const store = useStore();

// Элементы навигации
const navItems = ref<NavItem[]>([
  {
    title: "Все",
    icon: mdiViewList,
    action: () => router.push({name: "Home"}),
  },
  {
    title: "Категории",
    icon: mdiTagMultiple,
    action: () => router.push({name: "Tags"}),
  },
  {
    title: "Избранные",
    icon: mdiStar,
    action: () => router.push({name: "Favourites"}),
  },
  {
    title: "Настройки",
    icon: mdiCogs,
    action: () => router.push({name: "Settings"}),
  },
]);

// Кнопка установки
const installNav: NavItem = {
  action: async () => {
    await install.trigger();
    if (!install.isAvailable()) {
      const index = navItems.value.findIndex((x) => x.title === "Установить");
      if (index !== -1) {
        navItems.value.splice(index, 1);
      }
    }
  },
  icon: mdiDownload,
  title: "Установить",
};

// Добавление кнопки установки
const addInstallButton = async () => {
  await install.waitPrompt();
  navItems.value.push(installNav);
};

// Lifecycle hook
onMounted(() => {
  addInstallButton();
});
</script>

<style scoped>
</style>
