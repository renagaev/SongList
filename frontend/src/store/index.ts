import Vue from 'vue'
import Vuex from 'vuex'
import VuexPersistence from 'vuex-persist'
import {SongService} from "@/client";
import Fuzzysort from 'fuzzysort'
import {Settings, SongModel} from './models'
import deepmerge from "deepmerge"

export interface State {
    tags: string[],
    songs: SongModel[],
    searchText: string,
    showBar: boolean,
    showSearch: boolean,
    selectedSong?: SongModel,
    settings: Settings
}
// https://github.com/championswimmer/vuex-persist/issues/17#issuecomment-350825480
const requestIdleCallback = window.requestIdleCallback || (cb => {
    const start = Date.now()
    return setTimeout(() => {
        const data = {
            didTimeout: false,
            timeRemaining () {
                return Math.max(0, 50 - (Date.now() - start))
            }
        }
        cb(data)
    }, 1)
})

const vuexLocal = new VuexPersistence<State>({
    filter: (mutation) => mutation.type != "setSongs",
    reducer: (state) => {
        const s = {...state} as any
        delete s.songs
        return s
    },
    saveState: (key, state, storage) => {
        requestIdleCallback(() => {
            let data:any = JSON.stringify(state)
            if (storage && storage._config && storage._config.name === 'localforage') {
                data = deepmerge({}, state)
            }
            storage?.setItem(key, data)
        })
    }
})

Vue.use(Vuex)


export default new Vuex.Store<State>({
    state: {
        tags: ["Простые", "Сложные"],
        songs: [],
        searchText: "",
        showBar: false,
        showSearch: false,
        selectedSong: undefined,
        settings: {
            darkTheme: false,
            playNotes: true,
            fontSize: 16
        }
    },
    mutations: {
        setShowBar(state, value: boolean) {
            state.showBar = value
        },
        setSongs(state, songs: SongModel[]) {
            state.songs = songs
        },
        setSearchText(state, text: string) {
            state.searchText = text
        },
        setShowSearch(state, value: boolean) {
            state.showSearch = value
        },
        selectSong(state, id: number) {
            state.selectedSong = state.songs.find(x => x.id == id)
        },
        setDarkTheme(state, value: boolean) {
            state.settings.darkTheme = value
        },
        setFontSize(state, value: number) {
            state.settings.fontSize = value
        },
        setPlayNotes(state, value: boolean){
            state.settings.playNotes = value
        }
    },
    getters: {
        songs: (state) => (tag?: string) => {
            if (tag != null) {
                return state.songs.filter(x => x.tags.indexOf(tag) != -1)
            }
            const text = state.searchText.trim().toLowerCase()
            if (!text)
                return state.songs

            // contains only digits
            if (/^-?\d+$/.test(text)) {
                return state.songs.filter(x => x.number != null && x.number.toString().startsWith(text))
            }

            const dumbResults = state.songs.filter(x => x.title.toLowerCase().includes(text) || x.text.toLowerCase().includes(text))
            if (dumbResults.length != 0) {
                return dumbResults
            }

            return Fuzzysort.go(state.searchText, state.songs, {
                key: "prepared",
                limit: 5
            }).map(x => x.obj)

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

            const json = localStorage.getItem("songs")
            if (json != null) {
                const songs = JSON.parse(json)
                actionContext.commit("setSongs", songs.map(Object.freeze))
            }
            if (actionContext.state.songs.length == 0 || 1 == 1) {
                try {
                    const res = await SongService.getAllSongs();
                    const songs = res.map(x => x as SongModel)
                    songs.forEach(song => {
                        song.prepared = Fuzzysort.prepare(song.title + " " + song.text)
                    })
                    actionContext.commit("setSongs", songs.map(Object.freeze))
                    localStorage.setItem("songs", JSON.stringify(actionContext.state.songs))
                } catch (e) {
                    console.log("failed to load songs. seems we are offline")
                    console.log(e)
                }
            }
        }
    },
    modules: {},
    plugins: [vuexLocal.plugin]
})
