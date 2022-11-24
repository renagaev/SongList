import Vue from 'vue'
import App from './App.vue'
import './registerServiceWorker'
import router from './router'
import store from './store'
import vuetify from './plugins/vuetify'
import {OpenAPI} from "@/client";
import VueVirtualScroller from 'vue-virtual-scroller'

import 'vue-virtual-scroller/dist/vue-virtual-scroller.css'
Vue.use(VueVirtualScroller)

Vue.config.productionTip = false
OpenAPI.BASE = process.env.VUE_APP_API_BASE

new Vue({
  router,
  store,
  vuetify,
  render: h => h(App)
}).$mount('#app')
