import {Song} from "@/client";
import Fuzzysort from "fuzzysort"

export type SongModel = Song & { opened: number }
export type HistoryMode = 'morning' | 'evening' | 'both'
export type Settings = {
    darkTheme: boolean
    playNotes: boolean
    showHistory: boolean
    fontSize: number
    historyMode: HistoryMode
    historyHideDays: number
}
