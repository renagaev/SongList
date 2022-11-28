<template>
  <recycle-scroller
      class="scroller"
      :items="songs"
      :item-size="64"
      key-field="id"
      v-slot="{item}"
  >
    <div class="item">
      <v-list-item two-line @click="open(item.id)" class="item">
        <v-list-item-content>
          <v-list-item-title v-text="getTitle(item)"/>
          <v-list-item-subtitle v-text="item.text"/>
        </v-list-item-content>
      </v-list-item>
      <v-divider/>
    </div>
  </recycle-scroller>
</template>

<script lang="ts">
import Vue from 'vue'
import Component from 'vue-class-component'
import {SongModel} from "@/store/SongModel";
import {Prop} from 'vue-property-decorator';

@Component
export default class SongList extends Vue {

  @Prop()
  tag?: string

  activated() {
    this.$store.commit("setMainTitle", "Сборник песен")
    document.documentElement.style.overflow = "hidden"
  }

  deactivated() {
    document.documentElement.style.overflow = "auto"
  }

  getTitle(song: SongModel): string {
    if (song.number) {
      return `${song.title} | ${song.number}`
    }
    return song.title
  }


  get songs(): SongModel[] {
    return this.$store.getters["songs"](this.tag)
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