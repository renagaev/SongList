import Vue from 'vue'
import Vuex, {ActionContext} from 'vuex'
import VuexPersistence from 'vuex-persist'
import {SongService, HistoryService, SongOpeningStats, OpenAPI, Note} from "@/client";
import fuzzysort from 'fuzzysort'
import {Settings, SongModel} from './models'
import deepmerge from "deepmerge"
import * as signalR from '@microsoft/signalr';
import {HubConnection} from "@microsoft/signalr";

export interface State {
    tags: string[],
    songs: SongModel[],
    notes: Note[],
    favourites: number[],
    searchText: string,
    showBar: boolean,
    selectedSong?: SongModel,
    settings: Settings,
    connection: HubConnection
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
    filter: (mutation) => mutation.type != "setSongs" && mutation.type != "setSearchText" && mutation.type != 'updateOpenedCounter',
    reducer: (state) => {
        const s = {...state} as any
        delete s.songs
        delete s.connection
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

export default new Vuex.Store<State>({
    state: {
        tags: ["Простые", "Сложные"],
        songs: [],
        searchText: "",
        showBar: false,
        notes: [],
        selectedSong: undefined,
        favourites: [],
        settings: {
            darkTheme: false,
            playNotes: true,
            showHistory: false,
            fontSize: 16
        },
        connection: null!
    },
    mutations: {
        setShowBar(state, value: boolean) {
            state.showBar = value
        },
        setSongs(state, songs: SongModel[]) {
            state.songs = songs
        },
        setNotes(state, notes: Note[]) {
            state.notes = notes
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
        setShowHistory(state, value: boolean) {
            state.settings.showHistory = value
        },
        toggleFavourite(state, id: number) {
            const index = state.favourites.indexOf(id)
            if (index == -1) {
                state.favourites.unshift(id)
            } else {
                state.favourites.splice(index, 1)
            }
        },
        updateOpenedCounter(state: State, value: { id: number, value: number }) {
            const song = state.songs.find(x => x.id == value.id)!.opened = value.value
        }
    },
    getters: {
        songs: (state: State) => (tag?: string) => {
            if (tag != null) {
                return state.songs.filter(x => x.tags.indexOf(tag) != -1)
            }
            const text = state.searchText.trim().toLowerCase()
            if (!text) {
                return state.songs.sort((a, b) => {
                    return (b.opened - a.opened) || (a.title.toLowerCase().localeCompare(b.title.toLowerCase()))
                })
            }

            // contains only digits
            if (/^-?\d+$/.test(text)) {
                return state.songs.filter(x => x.number != null && x.number.toString().startsWith(text))
            }
            let search = state.songs.flatMap(song => Array.from(new Set(song.text.split("\n").filter(x=> x != ""))).map(x => ({obj: song, value: x.toLowerCase(), isTitle: false})).concat([{obj: song, value: song.title.toLowerCase(), isTitle: true}]))
            const searchRes = fuzzysort.go<SongModel>(state.searchText, search, {
                key: "value",
                limit: 50,
                scoreFn: res => res.score * (res.obj.isTitle ? 3 : 1),
            })
            return Array.from(new Set(searchRes.map(x => x.obj.obj)))
        },
        favourites: (state) => {
            return state.songs
                .map(x => ({idx: state.favourites.indexOf(x.id), song: x}))
                .filter(x => x.idx != -1)
                .sort((a, b) => a.idx - b.idx)
                .map(x => x.song)
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
                const songs: SongModel[] = JSON.parse(json)
                songs.forEach(x => x.opened = 0)
                actionContext.commit("setSongs", songs)
            }
            try {
                const res = await SongService.getAllSongs();
                const songs = res.map(x => x as SongModel)
                songs.forEach(song => {
                    song.opened = 0
                })
                actionContext.commit("setSongs", songs)
                localStorage.setItem("songs", JSON.stringify(actionContext.state.songs))
            } catch (e) {
                console.log("failed to load songs. seems we are offline")
                console.log(e)
            }
        },
        async loadNotes(actionContext) {
            const notes = await SongService.getNotes();
            actionContext.commit("setNotes", notes)
        },
        async initializeNowOpened(actionContext: ActionContext<State, State>) {
            actionContext.state.connection = new signalR.HubConnectionBuilder()
                .withUrl(`${OpenAPI.BASE}/songsHub`, {
                    skipNegotiation: true,
                    transport: signalR.HttpTransportType.WebSockets
                })
                .withAutomaticReconnect()
                .build()
            const connection = actionContext.state.connection
            connection.onreconnected(async connectionId => {
                if (actionContext.state.selectedSong) {
                    await actionContext.state.connection.invoke("openSong", actionContext.state.selectedSong.id)
                }
            })

            connection.on("updateCounter", (songId, value) => {
                actionContext.commit("updateOpenedCounter", {id: songId, value})
            })

            SongService.getOpenedSongs().then(stats => stats.forEach(song => {
                const id = song.id
                const value = song.count
                actionContext.commit("updateOpenedCounter", {id, value})
            }))

            await connection.start()
                .catch(err => console.error('SignalR Connection Error: ', err));

        },
        async songOpened(actionContext: ActionContext<State, State>, id: number) {
            while (actionContext.state.connection?.state != 'Connected') {
                await new Promise(r => setTimeout(r, 100));
            }
            await actionContext.state.connection.invoke("openSong", id)
        },
        async songClosed(actionContext: ActionContext<State, State>, id: number) {
            actionContext.commit("updateOpenedCounter", {
                id,
                value: Math.max(actionContext.getters.song(id).opened - 1, 0)
            })
            while (actionContext.state.connection.state != 'Connected') {
                await new Promise(r => setTimeout(r, 100));
            }
            await actionContext.state.connection.invoke("closeSong", id)
        },
        async getSongHistory(actionContext: ActionContext<State, State>, id: number) {
            const res = await HistoryService.getSongHistory(id)
            return res.map(d => new Date(d))
        }
    },
    modules: {},
    plugins: [vuexLocal.plugin]
})
