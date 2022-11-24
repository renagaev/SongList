import {Song} from "@/client";
import Fuzzysort from "fuzzysort"

export type SongModel = Song & { prepared: Fuzzysort.Prepared }