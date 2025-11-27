<template>
  <SongForm :song="song" :loading="isSaving" @save="save"></SongForm>
</template>

<script setup lang="ts">
import {useStore} from "vuex";
import {useRouter} from "vue-router";
import SongForm from "@/components/SongForm.vue";
import {Song} from "@/client";
import {ref} from "vue";

const store = useStore()
const router = useRouter()
const song: Song = ref<Song>({
  number: null,
  title: "",
  text: "",
  noteId: null,
  tags: [],
  id: 0
})
const isSaving = ref(false);

const save = async () => {
  if (isSaving.value) {
    return;
  }
  isSaving.value = true;
  try {
    const id = await store.dispatch("addSong", song.value)
    await router.push({name: "SingleSong", params: {id: id}})
  } finally {
    isSaving.value = false;
  }
}


</script>


<style scoped>

</style>
