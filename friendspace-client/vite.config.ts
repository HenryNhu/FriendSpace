import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { Agent } from 'node:https';
import { env } from 'node:process';

if (!env.APPDATA) {
  throw new Error('Không tìm thấy thư mục APPDATA.');
}

const certificateDirectory = join(env.APPDATA, 'ASP.NET', 'https');

const certificate = readFileSync(
  join(certificateDirectory, 'friendspace-client.pem'),
);

const privateKey = readFileSync(
  join(certificateDirectory, 'friendspace-client.key'),
);

export default defineConfig({
  plugins: [react()],
  server: {
    host: 'localhost',
    port: 5173,
    strictPort: true,
    https: {
      cert: certificate,
      key: privateKey,
    },
    proxy: {
      '/api': {
        target: 'https://localhost:7192',
        changeOrigin: true,
        secure: true,
        agent: new Agent({ ca: certificate }),
      },
    },
  },
});