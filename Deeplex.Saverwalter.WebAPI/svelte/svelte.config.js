import adapter from '@sveltejs/adapter-static';
import { vitePreprocess } from '@sveltejs/kit/vite';
import { resolve } from 'path';

/** @type {import('@sveltejs/kit').Config} */
const config = {
    // Consult https://kit.svelte.dev/docs/integrations#preprocessors
    // for more information about preprocessors
    preprocess: vitePreprocess(),

    kit: {
        adapter: adapter({
            fallback: 'index.html',
            pages: '../wwwroot',
            assets: '../wwwroot'
        }),
        alias: {
            $WalterComponents: resolve('./src/components/index'),
            $WalterStore: resolve('./src/store'),
            $WalterServices: resolve('./src/services'),
            $WalterTypes: resolve('./src/types/index'),
            $WalterLib: resolve('./src/lib/index')
        }
    }
};

export default config;
