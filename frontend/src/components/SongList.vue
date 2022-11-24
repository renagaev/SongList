<template>
  <RecycleScroller
      class="scroller"
      :items="songs"
      :item-size="64"
      key-field="id"
      v-slot="{item}"
      ref="scroll"
  >
    <div class="item">
      <v-list-item two-line @click="open(item.id)" class="item">
        <v-list-item-content>
          <v-list-item-title v-text="item.title"/>
          <v-list-item-subtitle v-text="item.text"/>
        </v-list-item-content>
      </v-list-item>
      <v-divider/>
    </div>
  </RecycleScroller>

  <!--  <div style="height: 100%" ref="container">-->
  <!--    <v-virtual-scroll-->
  <!--        :items="songs"-->
  <!--        item-height="64"-->
  <!--        :height="height"-->
  <!--        ref="scroll"-->
  <!--    >-->
  <!--      <template v-slot:default="{item}">-->
  <!--        <v-list-item two-line @click="open(item.id)" :key="item.id">-->
  <!--          <v-list-item-content>-->
  <!--            <v-list-item-title v-text="item.title"/>-->
  <!--            <v-list-item-subtitle v-text="item.text"/>-->
  <!--          </v-list-item-content>-->
  <!--        </v-list-item>-->
  <!--        <v-divider :key="item.id+'divider'"/>-->
  <!--      </template>-->
  <!--    </v-virtual-scroll>-->
  <!--  </div>-->


  <!--    <v-list dense flat>-->
  <!--      <template v-for="item in songs">-->
  <!--        <v-list-item two-line @click="open(item.id)" :key="item.id">-->
  <!--          <v-list-item-content>-->
  <!--            <v-list-item-title v-text="item.title"/>-->
  <!--            <v-list-item-subtitle v-text="item.text"/>-->
  <!--          </v-list-item-content>-->
  <!--        </v-list-item>-->
  <!--        <v-divider :key="item.id+'divider'"/>-->
  <!--      </template>-->
  <!--      <v-list-item-group>-->

  <!--      </v-list-item-group>-->
  <!--    </v-list>-->
</template>

<script lang="ts">
import Vue from 'vue'
import Component from 'vue-class-component'
import {SongModel} from "@/store/SongModel";
import {RecycleScroller} from "vue-virtual-scroller";

@Component
export default class SongList extends Vue {

  $refs!: {
    container: HTMLFormElement
    scroll: HTMLFormElement
  }

  activated() {
    this.$store.commit("setMainTitle", "Сборник песен")
    document.documentElement.style.overflow = "hidden"
  }

  deactivated() {
    document.documentElement.style.overflow = "auto"
  }


  get songs(): SongModel[] {
    return this.$store.getters["songs"]
  }

  open(id: number) {
    this.$router.push("song/" + id.toString())
  }
}
</script>

<style scoped>
.scroller {
  height: calc(100vh - 48px);
}

.item {
  height: 64px;
}
</style>