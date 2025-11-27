import {createRouter, RouteRecordRaw, createWebHashHistory} from 'vue-router'
import SingleSong from "@/pages/SingleSong.vue";
import TagList from "@/pages/TagList.vue";
import Settings from "@/pages/Settings.vue";
import MainList from "@/pages/MainList.vue";
import FavouritesList from "@/pages/FavouritesList.vue";
import EnableAdmin from "@/pages/EnableAdmin.vue";
import EditSong from "@/pages/EditSong.vue";
import AddSong from "@/pages/AddSong.vue";
import History from "@/pages/History.vue";

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
        path: '/song/add',
        name: "AddSong",
        component: AddSong,
        meta:{
            title: "Добавить песню"
        }
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
        path: '/history',
        name: "History",
        component: History,
        meta: {
            title: "История"
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
            return {top: 0}
        }
    }
})

export default router
