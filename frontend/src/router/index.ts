import Vue from 'vue'
import VueRouter, {RouteConfig} from 'vue-router'
import SongList from "@/components/SongList.vue";
import SingleSong from "@/components/SingleSong.vue";

Vue.use(VueRouter)

const routes: Array<RouteConfig> = [
    {
        path: '/',
        name: 'Home',
        component: SongList
    },
    {
        path: '/song/:id',
        name: 'SingleSong',
        props: true,
        component: SingleSong
    }
]

const router = new VueRouter({
    mode: 'history',
    base: process.env.BASE_URL,
    routes,
    scrollBehavior(to, from, savedPosition) {
        if (savedPosition) {
            return savedPosition
        } else {
            return { x: 0, y:0 }
        }
    },
})

export default router
