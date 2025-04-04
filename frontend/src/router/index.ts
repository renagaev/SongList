import {createRouter, RouteRecordRaw, createWebHashHistory} from 'vue-router'
import SingleSong from "@/components/SingleSong.vue";
import TagList from "@/components/TagList.vue";
import Settings from "@/components/Settings.vue";
import MainList from "@/components/MainList.vue";
import FavouritesList from "@/components/FavouritesList.vue";
import EnableAdmin from "@/components/EnableAdmin.vue";
import EditSong from "@/components/EditSong.vue";

const routes: Array<RouteRecordRaw> = [
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
        props: (route) => ({id: Number.parseInt(route.params.id as string)}),
    },
    {
        path: '/song/:id/edit',
        name: "EditSong",
        component: EditSong,
        props: (route) => ({id: Number.parseInt(route.params.id as string)}),
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
    },
    {
        path: '/enable-admin',
        name: "EnableAdmin",
        component: EnableAdmin
    }
]

const router = createRouter({
    history: createWebHashHistory(import.meta.env.VITE_API_BASE),
    routes,
    scrollBehavior(to, from, savedPosition) {
        if (savedPosition) {
            return savedPosition
        } else {
            return { top: 0 }
        }
    }
})

export default router
