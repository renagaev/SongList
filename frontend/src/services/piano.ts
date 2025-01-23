import * as Tone from "tone";

const availableNotes = [
    'A3', 'A4', 'A5', 'C3', 'C4', 'C5', 'D#3', 'D#4', 'D#5', "F#3", 'F#4', 'F#5'
];

export default class Piano {
    private static sampler: Tone.Sampler;

    public static async init() {
        const samples = {
            'A3': (await import('../assets/notes/A3.mp3')).default,
            'A4': (await import('../assets/notes/A4.mp3')).default,
            'A5': (await import('../assets/notes/A5.mp3')).default,
            'C3': (await import('../assets/notes/C3.mp3')).default,
            'C4': (await import('../assets/notes/C4.mp3')).default,
            'C5': (await import('../assets/notes/C5.mp3')).default,
            'D#3': (await import('../assets/notes/Ds3.mp3')).default,
            'D#4': (await import('../assets/notes/Ds4.mp3')).default,
            'D#5': (await import('../assets/notes/Ds5.mp3')).default,
            'F#3': (await import('../assets/notes/Fs3.mp3')).default,
            'F#4': (await import('../assets/notes/Fs4.mp3')).default,
            'F#5': (await import('../assets/notes/Fs5.mp3')).default
        }
        Piano.sampler = new Tone.Sampler(samples);
        Piano.sampler.toDestination()
    }

    private static nearestNoteCache = new Map<string, string>();

    private static findNearestAvailableNote(note: string): string {
        if (Piano.nearestNoteCache.has(note)) {
            return Piano.nearestNoteCache.get(note)!;
        }

        const midi = Tone.Frequency(note).toMidi();
        const nearestNote = availableNotes.reduce((prev, curr) => {
            const prevDiff = Math.abs(midi - Tone.Frequency(prev).toMidi());
            const currDiff = Math.abs(midi - Tone.Frequency(curr).toMidi());
            return currDiff < prevDiff ? curr : prev;
        });

        Piano.nearestNoteCache.set(note, nearestNote);
        return nearestNote;
    }

    public static async play(note?: any) {
        await Piano.sampler.context.resume()
        if (availableNotes.includes(note)) {
            Piano.sampler.triggerAttackRelease(note, '1n');
        } else {
            // Если нота отсутствует, используем ближайшую доступную
            const nearestNote = Piano.findNearestAvailableNote(note);
            const pitchShift = Tone.Frequency(note).toMidi() - Tone.Frequency(nearestNote).toMidi();
            const pitchShiftEffect = new Tone.PitchShift(pitchShift).toDestination();
            Piano.sampler.connect(pitchShiftEffect);
            Piano.sampler.triggerAttackRelease(nearestNote!, '1n', Tone.now());
            Piano.sampler.disconnect(pitchShiftEffect);
        }
    }
}

document.addEventListener("click", () => Piano.init(), {once: true})