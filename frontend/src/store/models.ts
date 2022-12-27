import {Song} from "@/client";
import Fuzzysort from "fuzzysort"

export type SongModel = Song & { prepared: Fuzzysort.Prepared, preparedTitle: Fuzzysort.Prepared }
export type Settings = {
    darkTheme: boolean
    playNotes: boolean
    fontSize: number
}