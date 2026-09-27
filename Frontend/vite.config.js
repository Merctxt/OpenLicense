import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { VitePWA } from 'vite-plugin-pwa'

export default defineConfig({
  plugins: [react(), VitePWA({
    registerType: 'autoUpdate',
    injectRegister: 'auto',
    manifest: false,
    workbox: {
      globPatterns: ['**/*.{js,css,html,svg,png,ico,txt,woff2}'],
    },
  })],
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:7224',
        changeOrigin: true,
      },
    },
  },
})
