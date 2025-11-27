<template>
  <div>
    <v-list nav>
      <template v-for="(item, idx) in tabs" :key="idx">
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
import {ref, onMounted, computed} from "vue";
import {mdiViewList, mdiTagMultiple, mdiCogs, mdiDownload, mdiStar, mdiHistory} from "@mdi/js";
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

const showInstall = ref(false)

const tabs = computed(() => {
  const res = [
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
    }
  ]

  if (store.state.settings.showHistory) {
    res.push({
      title: "История",
      icon: mdiHistory,
      action: () => router.push({name: "History"})
    })
  }
  res.push({
    title: "Настройки",
    icon: mdiCogs,
    action: () => router.push({name: "Settings"}),
  })

  if (showInstall.value) {
    res.push({
      action: async () => {
        await install.trigger();
        if (!install.isAvailable()) {
          showInstall.value = false
        }
      },
      icon: mdiDownload,
      title: "Установить",
    })
  }
  
  return res
})

// Добавление кнопки установки
const addInstallButton = async () => {
  await install.waitPrompt();
  showInstall.value = true
};

// Lifecycle hook
onMounted(() => {
  addInstallButton();
});
</script>

<style scoped>
</style>
