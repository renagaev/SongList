<template>
  <v-dialog close-on-back @after-leave="clearForm">
    <template v-slot:activator="{ props: activatorProps }">
      <v-btn
          v-bind="activatorProps"
          class="mb-3 mr-2"
          rounded
      >
        <v-icon>{{ mdiPaperclip }}</v-icon>
        {{ attachmentsCount > 0 ? attachmentsCount : "" }}
      </v-btn>
    </template>
    <template v-slot:default="{ isActive }">
      <v-card class="pa-2">
        <v-card-title>Файлы</v-card-title>

        <v-card-text class="pa-1">
          <div v-if="attachmentsCount != 0">
            <attachments-list :items="attachments" @update="loadAttachments()"/>
          </div>
          <div v-else>
            <p class="pa-2">
              Тут пока ничего нет. Если ты хочешь сюда что-то добавить: ноты, фонограмму, партии - скажи Ренату
            </p>

          </div>
          <div v-if="isAdmin">
            <v-divider></v-divider>
            <v-form class="pa-2" @submit="uploadAttachment" @submit.prevent>
              <v-alert v-if="error" type="error" density="compact" class="mb-3">{{ error }}</v-alert>
              <v-file-input :key="formKey"
                            :label="'Файл'"
                            @change="setFile"
                            :prepend-icon="null"
                            variant="outlined"
              ></v-file-input>
              <v-text-field :label="'Название'" placeholder="Введи название" variant="outlined" v-model="title"/>
              <v-btn type="submit" block :loading="loading">Добавить</v-btn>
            </v-form>
          </div>
        </v-card-text>
      </v-card>
    </template>
  </v-dialog>

</template>

<script setup lang="ts">

import {useStore} from "vuex";
import {mdiPaperclip} from "@mdi/js";
import {AttachmentsService} from "@/client";
import {computed, ref} from "vue";
import AttachmentsList from "@/components/AttachmentsList.vue";

const store = useStore()
// Props
const props = defineProps({
  id: {
    type: Number,
    required: true,
  },
});

const song = computed(() => store.state.songs.find(x => x.id == props.id));
let attachments = computed(() => song.value?.attachments ?? [])
let attachmentsCount = computed(() => attachments.value.length)

async function loadAttachments() {
  await store.dispatch("refreshSongAttachments", props.id)
}

const isAdmin = computed(() => store.state.isAdmin);

let file: Blob | null = null
const title = ref("")
const loading = ref(false)
const error = ref<string | null>(null)
// Смена key пересоздаёт v-file-input и сбрасывает выбранный файл.
const formKey = ref(0)

function setFile(e: Event) {
  const target = e.target as HTMLInputElement;
  file = target.files?.[0] ?? null;
}
function clearForm() {
  file = null
  title.value = ""
  error.value = null
  formKey.value++
  loading.value = false
}

async function uploadAttachment() {
  if (loading.value) {
    return
  }
  if (file == null || !title.value.trim()) {
    error.value = "Выбери файл и введи название"
    return
  }
  loading.value = true
  error.value = null
  try {
    await AttachmentsService.uploadAttachment(props.id, {file: file, displayName: title.value.trim()})
    await loadAttachments()
    clearForm()
  } catch (e) {
    console.error(e)
    error.value = "Не удалось загрузить файл"
  } finally {
    loading.value = false
  }
}
</script>


<style scoped>

</style>