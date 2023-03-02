import adapter from '@sveltejs/adapter-auto';
import { vitePreprocess } from '@sveltejs/kit/vite';
import { resolve } from "path";

/** @type {import('@sveltejs/kit').Config} */
const config = {
	// Consult https://kit.svelte.dev/docs/integrations#preprocessors
	// for more information about preprocessors
	preprocess: vitePreprocess(),

	kit: {
		adapter: adapter(),
		alias: {
			$WalterComponents: resolve("./src/components/index"),
			$WalterStore: resolve("./src/store"),
			$WalterServices: resolve("./src/services"),
			$WalterTypes: resolve("./src/types/index"),
			$WalterRoutes: resolve("./src/routes"),
		}
	}
};

export default config;
