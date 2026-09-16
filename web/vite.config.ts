import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      // Forwards to the Api host (default Kestrel "http" profile port — see
      // backend/src/Api/Properties/launchSettings.json). Proxying avoids needing CORS
      // middleware on the backend during development.
      '/api': {
        target: 'http://localhost:5032',
        changeOrigin: true,
      },
      '/uploads': {
        target: 'http://localhost:5032',
        changeOrigin: true,
      },
    },
  },
})
