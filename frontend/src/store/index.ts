import Vue from 'vue'
import Vuex from 'vuex'
import VuexPersistence from 'vuex-persist'
import {SongService} from "@/client";
import fuzzysort from 'fuzzysort'
import {Settings, SongModel} from './models'
import deepmerge from "deepmerge"

export interface State {
    tags: string[],
    songs: SongModel[],
    favourites: number[],
    searchText: string,
    showBar: boolean,
    selectedSong?: SongModel,
    settings: Settings
}

// https://github.com/championswimmer/vuex-persist/issues/17#issuecomment-350825480
const requestIdleCallback = window.requestIdleCallback || (cb => {
    const start = Date.now()
    return setTimeout(() => {
        const data = {
            didTimeout: false,
            timeRemaining() {
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
            let data: any = JSON.stringify(state)
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
        selectedSong: undefined,
        favourites: [],
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
        selectSong(state, id: number) {
            state.selectedSong = state.songs.find(x => x.id == id)
        },
        setDarkTheme(state, value: boolean) {
            state.settings.darkTheme = value
        },
        setFontSize(state, value: number) {
            state.settings.fontSize = value
        },
        setPlayNotes(state, value: boolean) {
            state.settings.playNotes = value
        },
        toggleFavourite(state, id: number){
            const index = state.favourites.indexOf(id)
            if(index == -1){
                state.favourites.unshift(id)
            } else{
                state.favourites.splice(index, 1)
            }
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

            const searchRes = fuzzysort.go<SongModel>(state.searchText, state.songs, {
                keys: ["preparedTitle", "prepared"],
                limit: 8,
                // make title match more important for search result
                scoreFn: res => {
                    const title = res[0]
                    const textRes = res[1]
                    
                    return Math.max(title ? title.score : -1000, textRes ? textRes.score - 100 : -1000)
                }
            })
            return searchRes.map(x => x.obj)

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
                        song.prepared = fuzzysort.prepare(song.text)
                        song.preparedTitle = fuzzysort.prepare(song.title)
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
