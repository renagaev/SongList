<template>
  <div>
    <v-list>
      <div v-for="item in items" :key="item.id">
        <v-list-item :title="item.name" class="spacer">
          <template v-slot:prepend class="spacer">
            <v-icon size="24">{{ getIcon(item.type) }}</v-icon>
          </template>
          <template v-slot:append>
            <a target="_blank" :href="getLink(item.id)">
              <v-icon size="24">
                {{ mdiEye }}
              </v-icon>
            </a>
            <v-icon v-if="isAdmin" class="ml-3" @click="remove(item.id)">
              {{ mdiTrashCan }}
            </v-icon>
          </template>
        </v-list-item>

        <v-divider/>
      </div>

    </v-list>
  </div>
</template>
<script setup lang="ts">
import {useStore} from "vuex";
import {Attachment, AttachmentType, OpenAPI} from "@/client";
import {mdiFileDocument, mdiMusicBox, mdiEye, mdiTrashCan} from "@mdi/js";

const store = useStore()
const props = defineProps<{
  items: Attachment[]
}>()

const emit = defineEmits(["update"])
let isAdmin = store.state.isAdmin
let getIcon = (type: AttachmentType) => type == AttachmentType._0 ? mdiMusicBox : mdiFileDocument
let getLink = (id: number) => `${OpenAPI.BASE}/attachments/${id}/download`

let remove = async (id: number) => {
  await store.dispatch("removeSongAttachment", id);
  emit("update")
}

</script>

<style scoped>
.spacer :deep(.v-list-item__prepend .v-list-item__spacer) {
  width: 8px !important;
}
</style>