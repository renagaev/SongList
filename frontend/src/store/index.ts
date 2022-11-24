import Vue from 'vue'
import Vuex from 'vuex'
import VuexPersistence from 'vuex-persist'
import {SongService} from "@/client";
import Fuzzysort from 'fuzzysort'
import {SongModel} from './SongModel'

const vuexLocal = new VuexPersistence({
    storage: window.localStorage
})

export interface State {
    tags: string[],
    title: string,
    selectedTag: string | null,
    songs: SongModel[],
    searchText: string
}

Vue.use(Vuex)


export default new Vuex.Store<State>({
    state: {
        tags: ["Простые", "Сложные"],
        title: "Сборник песен",
        selectedTag: null,
        songs: [],
        searchText: ""
    },
    mutations: {

        setMainTitle(state, title: string){
            state.title = title
        },
        setTag(state, tag: string) {
            state.selectedTag = tag
            state.searchText = ""
        },
        setSongs(state, songs: SongModel[]) {
            state.songs = songs
        },
        setSearchText(state, text: string) {
            state.searchText = text
            state.selectedTag = null
        }
    },
    getters: {
        songs(state) {
            if (state.selectedTag != null) {
                return state.songs.filter(x => x.tags.indexOf(state.selectedTag!) != -1)
            }
            if (state.searchText) {
                const res = Fuzzysort.go(state.searchText, state.songs, {
                    key: "prepared",
                    limit: 5
                }).map(x=> x.obj)
                return res
            } else {
                return state.songs
            }
        },
        song: (state) => (id: number) => {
            return state.songs.find(x => x.id == id)
        },
        tags(state) {
            return new Array(...new Set(state.songs.map(x => x.tags).flat())).sort()
        }
    },
    actions: {
        async loadSongs(actionContext) {
            if (actionContext.state.songs.length == 0 || 1 == 1) {
                const res = await SongService.getAllSongs();
                const songs = res.map(x => x as SongModel)
                songs.forEach(song => {
                    song.prepared = Fuzzysort.prepare(song.text)
                })
                actionContext.commit("setSongs", songs.map(Object.freeze))
            }
        }
    },
    modules: {},
    plugins: [vuexLocal.plugin]
})
