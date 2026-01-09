import Vue from 'vue'
import Vuex from 'vuex'
import {ActionContext} from "vuex";
import VuexPersistence from 'vuex-persist'
import {
    SongService,
    HistoryService,
    SongOpeningStats,
    OpenAPI,
    Note,
    AuthService,
    Song,
    AttachmentsService, ServiceDto, SongLastHistoryDto
} from "@/client";
import fuzzysort from 'fuzzysort'
import {HistoryMode, Settings, SongModel} from './models'
import deepmerge from "deepmerge"
import * as signalR from '@microsoft/signalr';
import {HubConnection} from "@microsoft/signalr";

export interface State {
    tags: string[],
    songs: SongModel[],
    notes: Note[],
    favourites: number[],
    songLastHistory: SongLastHistoryDto[],
    searchText: string,
    showBar: boolean,
    selectedSong?: SongModel,
    settings: Settings,
    token?: string,
    userName?: string,
    isAdmin: boolean,
    adminEnabled: boolean,
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
        songs: Array.of<SongModel>(),
        searchText: "",
        showBar: false,
        notes: Array.of<Note>(),
        selectedSong: undefined,
        favourites: [],
        settings: {
            darkTheme: false,
            playNotes: true,
            showHistory: false,
            fontSize: 16,
            historyMode: "both",
            historyHideDays: 0
        },
        connection: null!,
        isAdmin: false,
        adminEnabled: false,
        songLastHistory: []
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
        setHistoryMode(state, value: HistoryMode) {
            state.settings.historyMode = value
        },
        setHistoryHideDays(state, value: number) {
            state.settings.historyHideDays = Math.max(0, value)
        },
        setSongLastHistory(state, value: SongLastHistoryDto[]) {
            state.songLastHistory = value ?? []
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
        },
        setToken(state, token: string) {
            state.token = token
        },
        setUserName(state, userName: string) {
            state.isAdmin = !!userName
            state.userName = userName
        },
        enableAdmin(state) {
            state.adminEnabled = true
        },
        addSong(state, song: SongModel) {
            state.songs.push(song)
        }
    },
    getters: {
        songs: (state: State) => (tag?: string) => {
            if (tag != null) {
                return state.songs
                    .filter(x => x.tags.indexOf(tag) != -1)
                    .sort((a, b) => {
                        return a.title.toLowerCase().localeCompare(b.title.toLowerCase())
                    })
            }
            const text = state.searchText.trim().toLowerCase()
            if (!text) {
                return state.songs.sort((a, b) => {
                    return a.title.toLowerCase().localeCompare(b.title.toLowerCase())
                })
            }

            // contains only digits
            if (/^-?\d+$/.test(text)) {
                return state.songs.filter(x => x.number != null && x.number.toString().startsWith(text))
            }
            let search = state.songs.flatMap(song => Array.from(new Set(song.text.split("\n").filter(x => x != ""))).map(x => ({
                obj: song,
                value: x.toLowerCase(),
                isTitle: false
            })).concat([{obj: song, value: song.title.toLowerCase(), isTitle: true}]))
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
                .map(x => x.song)
                .sort((a, b) => {
                    return a.title.toLowerCase().localeCompare(b.title.toLowerCase())
                })
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
            const sorted = notes.sort((a, b) => a.id - b.id)
            actionContext.commit("setNotes", sorted)
        },
        async loadSongLastHistory(actionContext) {
            const history = await HistoryService.getSongsLastHistory()
            actionContext.commit("setSongLastHistory", history)
            return history
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
        },
        async getSongAttachments(actionContext: ActionContext<State, State>, id: number){
            return AttachmentsService.getAttachments(id);
        },
        async removeSongAttachment(actionContext: ActionContext<State, State>, id: number){
            return AttachmentsService.deleteAttachment(id)
        },
        async login(actionContext: ActionContext<State, State>, user: object) {
            const res = await AuthService.getTgAdminToken(user)
            actionContext.commit("setToken", res)
            actionContext.dispatch("checkLogin")
        },
        async checkLogin(actionContext: ActionContext<State, State>) {
            if (!actionContext.state.token) {
                actionContext.commit("setUserName", null)
                return
            }
            OpenAPI.TOKEN = actionContext.state.token
            try {
                const userName = await AuthService.getUser()
                actionContext.commit("setUserName", userName)
            } catch (e) {
                actionContext.commit("setUserName", null)
            }
        },
        async updateSong(actionContext: ActionContext<State, State>, updatedSong: Song) {
            const song = await SongService.updateSong(updatedSong.id, updatedSong)
            const model = song as SongModel
            const existingSong = actionContext.state.songs.find(x => x.id == model.id)!
            existingSong.title = model.title
            existingSong.tags = model.tags
            existingSong.text = model.text
            existingSong.tags = model.tags
            existingSong.number = model.number
            existingSong.noteId = model.noteId
            existingSong.note = model.note
        },
        async addSong(actionContext: ActionContext<State, State>, newSong: Song) {
            const model = await SongService.addSong(newSong) as SongModel
            actionContext.commit("addSong", model)
            return model.id
        }
    },
    modules: {},
    plugins: [vuexLocal.plugin]
})
