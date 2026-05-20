import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  build: {
    outDir: '../Casan_IT15_Project/wwwroot',
    emptyOutDir: true,
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'https://localhost:7144',
        changeOrigin: true,
        secure: false,
      },
      '/hubs': {
        target: 'https://localhost:7144',
        changeOrigin: true,
        secure: false,
        ws: true,
      },
    },
  },
})
