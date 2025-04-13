<template>
  <div></div>
</template>
<script setup lang="ts">
import {useStore} from "vuex";
import {useRouter} from 'vue-router'

const router = useRouter();
const store = useStore();

store.commit("enableAdmin")
router.push("/settings")

</script>


<style scoped>

</style>