<template>
  <v-form class="pa-3" @submit.prevent :validate-on="'input'" @submit="save">
    <v-text-field v-model="song.title" :rules="rules" label="Название" required rounded-sm variant="outlined"></v-text-field>
    <v-combobox v-model="song.tags" :items="tags" rounded-sm label="Теги" multiple variant="outlined" chips/>
    <v-number-input v-model="song.number" label="Номер" rounded-sm control-variant="stacked" variant="outlined" >
      <template v-slot:increment></template>
      <template v-slot:decrement></template>
    </v-number-input>
    <v-select v-model="song.noteId" label="Нота" rounded-sm variant="outlined" :items="notes" :item-title="'detailedName'"
              :item-value="'id'"></v-select>
    <v-textarea v-model="song.text" label="Текст" :rules="rules" required auto-grow rounded-sm variant="outlined"></v-textarea>
    <v-btn class="mt-2"  type="submit" block>Сохранить</v-btn>
  </v-form>

</template>

<script setup lang="ts">
import {useStore} from "vuex";
import type {Song} from "@/client";
import {PropType, ref} from "vue";

const store = useStore()
const tags = store.getters["tags"]
const props = defineProps({
  song: {
    type: Object as PropType<Song>,
    required: true,
  },
});
const notes = store.state.notes
const emit = defineEmits<{
  (e: "save")
}>()

const song = props.song!;
const rules = [value => {
  if (value)
    return true
  return "Пустое значение"
}]

const save = async (promise) => {
  const validation = await promise
  if(!validation.valid){
    return
  }
  emit("save", song)
}

</script>


<style scoped>

</style>