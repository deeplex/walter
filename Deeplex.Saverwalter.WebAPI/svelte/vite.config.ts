import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig } from 'vite';

const apiTarget = process.env.VITE_API_PROXY_TARGET ?? 'http://localhost:5254';

export default defineConfig({
    plugins: [sveltekit()],
    server: {
        proxy: {
            '/api': apiTarget
        }
    }
});
