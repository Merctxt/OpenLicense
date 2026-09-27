import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { VitePWA } from 'vite-plugin-pwa'

export default defineConfig({
  plugins: [react(), VitePWA({
    registerType: 'autoUpdate',
    includeAssets: ['favicon.svg', 'robots.txt', 'CNAME'],
    manifest: {
      name: 'OpenLicense',
      short_name: 'OpenLicense',
      description: 'OpenLicense Application',
      theme_color: '#ffffff',
      background_color: '#ffffff',
      display: 'standalone',
      icons: [
        {
          src: '/favicon.svg',
          sizes: '192x192',
          type: 'image/svg+xml',
        },
        {
          src: '/favicon.svg',
          sizes: '512x512',
          type: 'image/svg+xml',
        },
      ],
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
