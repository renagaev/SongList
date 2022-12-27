let installPrompt: any = null


const waitPromise = new Promise<void>((resolve, reject) => {
    window.addEventListener("beforeinstallprompt", (e) => {
        installPrompt = e
        resolve()
    })
})
const isAvailable = () => installPrompt != null
const waitPrompt = () => waitPromise

const trigger = async () => {
    const {outcome} = await installPrompt.prompt()
    if (outcome === "accepted") {
        installPrompt = null
    }
}
export default {waitPrompt, trigger, isAvailable}