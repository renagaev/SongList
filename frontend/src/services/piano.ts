import {Sampler} from "tone";

const notes = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"]


export default class Piano {
    private static piano: Tone.Sampler;

    public static async init() {
        Piano.piano = new Sampler({
            "C4": (await import("../assets/notes/C4.mp3")).default,
            "C#4": (await import("../assets/notes/Cs4.mp3")).default,
            "D4": (await import("../assets/notes/D4.mp3")).default,
            "D#4": (await import("../assets/notes/Ds4.mp3")).default,
            "E#4": (await import("../assets/notes/E4.mp3")).default,
            "F4": (await import("../assets/notes/F4.mp3")).default,
            "F#4": (await import("../assets/notes/F4.mp3")).default,
            "G4": (await import("../assets/notes/G4.mp3")).default,
            "G#4": (await import("../assets/notes/Gs4.mp3")).default,
            "A4": (await import ("../assets/notes/A4.mp3")).default,
            "A#4": (await import("../assets/notes/As4.mp3")).default,
            "B4": (await import("../assets/notes/B4.mp3")).default
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
        return Piano.convert(noteRaw) != null
    }

    public static async play(note?: string | null) {
        await Piano.piano.context.resume()
        const converted = Piano.convert(note)
        if (converted) {
            Piano.piano.triggerAttackRelease(converted, 1)
        }
    }
}

document.addEventListener("click", () => Piano.init(), {once: true})