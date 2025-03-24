import { defineConfig } from 'vite'

export default defineConfig({
    base: '',
    build: {
        target: "es2020",
        outDir: "../../Build/Web",
        rollupOptions: {
            output: {
                entryFileNames: "assets/[name].js",
                chunkFileNames: "assets/[name].js",
                assetFileNames: "assets/[name].[ext]"
            }
        }
    },
    server: {
        port: 9000
    }
})