import {Song} from "@/client";
import Fuzzysort from "fuzzysort"

export type SongModel = Song & { opened: number }
export type Settings = {
    darkTheme: boolean
    playNotes: boolean
    showHistory: boolean
    fontSize: number
}