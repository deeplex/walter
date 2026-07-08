// @ts-nocheck
import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig } from 'vitest/config';

export default defineConfig({
    plugins: [sveltekit()],
    test: {
        environment: 'jsdom',
        include: ['src/**/*.{test,spec}.ts'],
        coverage: {
            reporter: ['text-summary', 'html'],
            reportsDirectory: './coverage',
            provider: 'v8'
        }
    }
});
