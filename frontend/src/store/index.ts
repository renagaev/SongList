import Vue from 'vue'
import Vuex from 'vuex'
import VuexPersistence from 'vuex-persist'
import {SongService} from "@/client";
import Fuzzysort from 'fuzzysort'
import {Settings, SongModel} from './models'
import Piano from "@/piano/piano";

const vuexLocal = new VuexPersistence({
    storage: window.localStorage
})

export interface State {
    tags: string[],
    songs: SongModel[],
    searchText: string,
    showBar: boolean,
    showSearch: boolean,
    selectedSong?: SongModel,
    settings: Settings
}

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
            playNotes: true
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
        }
    },
    getters: {
        songs: (state) => (tag?: string) => {
            if (tag != null) {
                return state.songs.filter(x => x.tags.indexOf(tag) != -1)
            }
            const text = state.searchText.trim()
            if (!text)
                return state.songs

            // contains only digits
            if (/^-?\d+$/.test(text)) {
                return state.songs.filter(x => x.number != null && x.number.toString().startsWith(text))
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
