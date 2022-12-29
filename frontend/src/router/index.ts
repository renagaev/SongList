import Vue from 'vue'
import VueRouter, {RouteConfig} from 'vue-router'
import SingleSong from "@/components/SingleSong.vue";
import TagList from "@/components/TagList.vue";
import Settings from "@/components/Settings.vue";
import MainList from "@/components/MainList.vue";
import FavouritesList from "@/components/FavouritesList.vue";

Vue.use(VueRouter)

const routes: Array<RouteConfig> = [
    {
        path: '/',
        name: 'Home',
        component: MainList,
        meta: {
            title: "Все",
        },
        props: route => ({
            tag: route.query.tag
        })
    },
    {
        path: '/song/:id',
        name: 'SingleSong',
        component: SingleSong,
        props: (route) => ({id: Number.parseInt(route.params.id)}),
    },
    {
        path: '/tags',
        name: "Tags",
        component: TagList,
        meta: {
            title: "Категории"
        }
    },
    {
        path: '/settings',
        name: "Settings",
        component: Settings,
        meta: {
            title: "Настройки"
        }
    },
    {
        path: '/favourites',
        name: "Favourites",
        component: FavouritesList,
        meta: {
            title: "Избранные"
        }
    }
]

const router = new VueRouter({
    mode: 'hash',
    base: process.env.BASE_URL,
    routes,
    scrollBehavior(to, from, savedPosition) {
        if (savedPosition) {
            return savedPosition
        } else {
            return {x: 0, y: 0}
        }
    },
})

export default router
