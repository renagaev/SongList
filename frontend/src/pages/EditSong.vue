<template>
  <SongForm :song="song" @save="save"></SongForm>
</template>

<script setup lang="ts">
import {useStore} from "vuex";
import {useRouter} from "vue-router";
import SongForm from "@/components/SongForm.vue";
import {ref, toRaw} from "vue";

const store = useStore()
const router = useRouter()
const props = defineProps({
  id: {
    type: Number,
    required: true,
  },
});

const originalSong = store.getters.song(props.id)
const song = ref(structuredClone(toRaw(originalSong)))
const save = async () => {
  await store.dispatch("updateSong", song.value)
  await router.back();
}


</script>


<style scoped>

</style>