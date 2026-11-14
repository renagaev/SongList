// https://github.com/championswimmer/vuex-persist/issues/17#issuecomment-350825480
// Safari не поддерживает requestIdleCallback, поэтому везде используем этот полифил.
export const requestIdleCallback: typeof window.requestIdleCallback = window.requestIdleCallback || (cb => {
    const start = Date.now()
    return setTimeout(() => {
        const data = {
            didTimeout: false,
            timeRemaining() {
                return Math.max(0, 50 - (Date.now() - start))
            }
        }
        cb(data)
    }, 1) as unknown as number
})
