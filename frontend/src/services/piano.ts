import {Sampler} from "tone";

const notes = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"]


export default class Piano {
    private static piano: Sampler;

    public static init() {
        this.piano = new Sampler({
            "C4": require("../assets/notes/C4.mp3"),
            "C#4": require("../assets/notes/Cs4.mp3"),
            "D4": require("../assets/notes/D4.mp3"),
            "D#4": require("../assets/notes/Ds4.mp3"),
            "E#4": require("../assets/notes/E4.mp3"),
            "F4": require("../assets/notes/F4.mp3"),
            "F#4": require("../assets/notes/F4.mp3"),
            "G4": require("../assets/notes/G4.mp3"),
            "G#4": require("../assets/notes/Gs4.mp3"),
            "A4": require("../assets/notes/A4.mp3"),
            "A#4": require("../assets/notes/As4.mp3"),
            "B4": require("../assets/notes/B4.mp3")
        }).toDestination()
    }

    private static convert(noteRaw?: string | null): string | null {
        if (noteRaw == null || noteRaw.length == 0)
            return null
        const match = noteRaw.toLowerCase().match("(до|ре|ми|фа|соль|ля|си)(#|♭)?")
        if (match == null)
            return null
        const baseNote = match[1]
            .replace("до", "C")
            .replace("ре", "D")
            .replace("ми", "E")
            .replace("фа", "F")
            .replace("соль", "G")
            .replace("ля", "A")
            .replace("си", "B");
        const modifier = match[2]
        let note = baseNote
        if (modifier == "#")
            note = note + modifier
        if (modifier == "♭")
            note = notes[notes.indexOf(note) - 1]
        if (notes.includes(note))
            return note + "4"
        return null
    }

    public static canPlay(noteRaw?: string | null): boolean {
        return this.convert(noteRaw) != null
    }

    public static async play(note?: string | null) {
        await this.piano.context.resume()
        const converted = this.convert(note)
        if (converted) {
            this.piano.triggerAttackRelease(converted, 1)
        }
    }
}

document.addEventListener("click", Piano.init, {once: true})