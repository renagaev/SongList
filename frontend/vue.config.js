module.exports = {
    transpileDependencies: [
        'vuetify'
    ],
    pwa: {
        name: "Сборник песен",
        themeColor: "#808080",
        productionSourceMap: false,
        workboxPluginMode: 'GenerateSW',
        workboxOptions: {
            skipWaiting: true
        }
    }
}
