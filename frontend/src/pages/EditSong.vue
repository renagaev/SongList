<template>
  <SongForm :song="song" :loading="isSaving" @save="save"></SongForm>
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
const isSaving = ref(false);
const save = async () => {
  if (isSaving.value) {
    return;
  }
  isSaving.value = true;
  try {
    await store.dispatch("updateSong", song.value)
    await router.back();
  } finally {
    isSaving.value = false;
  }
}


</script>


<style scoped>

</style>
