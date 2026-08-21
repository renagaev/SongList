import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import store from './store'
import {OpenAPI} from "@/client";
import VueVirtualScroller from 'vue-virtual-scroller'
import "@/services/installPrompt"
import 'vue-virtual-scroller/dist/vue-virtual-scroller.css'
import 'vuetify/styles'
import { createVuetify } from 'vuetify'
import { aliases, mdi } from 'vuetify/iconsets/mdi-svg' // именно -svg

document.addEventListener("click", e => navigator.wakeLock.request("screen"), {once: true})
const vuetify = createVuetify({
  icons:{
    defaultSet: 'mdi',
    aliases,
    sets: {
      mdi,
    },
  }
})
const base =  import.meta.env.VITE_API_BASE
console.log("base url:", base !== undefined, base)
OpenAPI.BASE = base
// OpenAPI.BASE = "https://songs.russia-church.com"
const app = createApp(App, {
  vuetify,
})
app.use(vuetify)
app.use(store)
app.use(VueVirtualScroller)
app.use(router)
app.mount('#app')
