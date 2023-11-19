import adapter from '@sveltejs/adapter-static';
import { vitePreprocess } from '@sveltejs/kit/vite';
import { resolve } from 'path';

/** @type {import('@sveltejs/kit').Config} */
const config = {
    // Consult https://kit.svelte.dev/docs/integrations#preprocessors
    // for more information about preprocessors
    preprocess: vitePreprocess(),
    ssr: {
        noExternal:
            process.env.NODE_ENV === 'production' ? ['@carbon/charts'] : []
    },
    kit: {
        adapter: adapter({
            fallback: 'index.html',
            pages: '../wwwroot',
            assets: '../wwwroot'
        }),
        alias: {
            $walter: resolve('./src')
        }
    }
};

export default config;
