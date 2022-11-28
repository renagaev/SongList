import {Song} from "@/client";
import Fuzzysort from "fuzzysort"

export type SongModel = Song & { prepared: Fuzzysort.Prepared }
export type Settings = {
    darkTheme: boolean
    playNotes: boolean
}