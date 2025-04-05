<template>
  <div class="pa-3">
    <v-text-field v-model="title" label="Название" required rounded-sm variant="outlined"></v-text-field>
    <v-number-input v-model="number" label="Номер" rounded-sm control-variant="stacked" variant="outlined">
      <template v-slot:increment></template>
      <template v-slot:decrement></template>
    </v-number-input>
    <v-select v-model="note" label="Нота" rounded-sm variant="outlined" :items="notes" :item-title="'name'" :item-value="'id'"></v-select>
    <v-textarea v-model="text" label="Текст" required auto-grow rounded-sm variant="outlined"></v-textarea>
    <v-btn class="mt-2" @click="save">Сохранить</v-btn>
  </div>


</template>

<script setup lang="ts">
import {useStore} from "vuex";
import {useRouter} from "vue-router";
import {VNumberInput} from "vuetify/labs/components";
import {Song} from "@/client";
import {ref} from "vue";

const store = useStore()
const router = useRouter()
const props = defineProps({
  id: {
    type: Number,
    required: true,
  },
});

const song = store.getters.song(props.id)
const title = ref(song.title)
const text = ref(song.text)
const notes = store.state.notes
const note = ref(song.noteId)
const number = ref(song.number)

const save = async () => {
  const updatedSong: Song = {
    id: song.id,
    title: title.value,
    text: text.value,
    noteId: note.value,
    tags: song.tags
  }
  await store.dispatch("updateSong", updatedSong)
  await router.back();
}


</script>


<style scoped>

</style>