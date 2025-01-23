import { createApp } from 'vue'
import App from './App.vue'
import './registerServiceWorker'
import router from './router'
import store from './store'
import VueRouter from 'vue-router'
import Vuex from 'vuex'
import {OpenAPI} from "@/client";
import VueVirtualScroller from 'vue-virtual-scroller'
import Piano from "@/services/piano"
import "@/services/installPrompt"
import 'vue-virtual-scroller/dist/vue-virtual-scroller.css'
import 'vuetify/styles'
import { createVuetify } from 'vuetify'
import {mdiViewList, mdiTagMultiple, mdiCogs, mdiDownload, mdiStar, mdiCheckboxBlankOutline, mdiCheckboxMarked} from "@mdi/js";
import { aliases, mdi } from 'vuetify/iconsets/mdi-svg'

document.addEventListener("click", e => navigator.wakeLock.request("screen"), {once: true})
const vuetify = createVuetify({
  icons:{
    defaultSet: 'mdi',
    sets: {
      mdi,
    },
  }
})

OpenAPI.BASE = import.meta.env.VITE_API_BASE
const app = createApp(App, {
  vuetify,
})
app.use(vuetify)
app.use(store)
app.use(VueVirtualScroller)
app.use(router)
app.mount('#app')
