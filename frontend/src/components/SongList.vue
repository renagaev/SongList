<template>
  <recycle-scroller
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
import {SongModel} from "@/store/models";
import {Prop} from 'vue-property-decorator';

@Component({name: "SongList"})
export default class SongList extends Vue {

  $refs!: {
    scroll: HTMLFormElement
  }

  @Prop()
  tag?: string
  scrollTop = 0
  lastTag?: string
  
  activated() {
    if (this.lastTag == this.tag) {
      this.$refs.scroll.scrollTop = this.scrollTop
    }
    document.documentElement.style.overflow = "hidden"
  }

  deactivated() {
    this.scrollTop = this.$refs.scroll.scrollTop
    this.lastTag = this.tag
    this.$store.commit("setShowSearch", false)
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
  height: calc(100vh - 64px);
}

.item {
  height: 64px;
}
</style>