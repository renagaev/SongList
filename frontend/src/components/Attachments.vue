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
        <v-divider></v-divider>

        <v-card-text class="pa-1">
          <div v-if="attachmentsCount != 0">
            <attachments-list :items="attachments" @update="loadAttachments()"/>
          </div>
          <div v-else>
            <p class="pa-2">
              Тут пока ничего нет. Если ты хочешь сюда что-то добавить: ноты, фотограмму, партии - скажи Ренату
            </p>

          </div>
          <div v-if="isAdmin || true">
            <v-divider v-if="attachmentsCount == 0"></v-divider>
            <v-form class="pa-2" @submit="uploadAttachment" @submit.prevent>
              <v-file-input :label="'Файл'"
                            @change="setFile"
                            :clear-icon="mdiCloseCircle"
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
import {mdiPaperclip, mdiCloseCircle} from "@mdi/js";
import {Attachment, AttachmentsService} from "@/client";
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
console.log(mdiCloseCircle)

let attachments = ref(Array.of<Attachment>())
let attachmentsCount = computed(() => attachments.value.length)

async function loadAttachments() {
  const res = await store.dispatch("getSongAttachments", props.id)
  attachments.value = res
}

loadAttachments()

const isAdmin = computed(() => store.state.isAdmin);

let file: Blob | null = null
let title: string | null = null
let loading = ref(false)

function setFile(e: Event){
  const target= e.target as HTMLInputElement;
  file = (target.files as FileList)[0];
}
function clearForm() {
  file = null
  title = null
  loading.value = false
}

async function uploadAttachment() {
  if (file == null || !(!!title)) {
    return
  }
  loading.value = true
  await AttachmentsService.uploadAttachment(props.id, {file: file!, displayName: title!})
  await loadAttachments()
  loading.value = false
}
</script>


<style scoped>

</style>