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
import { createVuetify } from 'vuetify'

try {
  navigator.wakeLock.request("screen")
} catch (e){
  console.log(e)
}

const vuetify = createVuetify()

OpenAPI.BASE = process.env.VUE_APP_API_BASE
const app = createApp(App, {
  vuetify,
})
app.use(vuetify)
app.use(store)
app.use(VueVirtualScroller)
app.use(router)
app.mount('#app')
