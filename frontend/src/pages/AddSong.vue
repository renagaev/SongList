<template>
  <SongForm :song="song" @save="save"></SongForm>
</template>

<script setup lang="ts">
import {useStore} from "vuex";
import {useRouter} from "vue-router";
import SongForm from "@/components/SongForm.vue";
import {Song} from "@/client";
import {ref} from "vue";

const store = useStore()
const router = useRouter()
const song: Song = ref({
  number: null,
  title: "",
  text: null,
  noteId: null
})

const save = async () => {
  const id = await store.dispatch("addSong", song.value)
  await router.push({name: "SingleSong", params: {id: id}})
}


</script>


<style scoped>

</style>