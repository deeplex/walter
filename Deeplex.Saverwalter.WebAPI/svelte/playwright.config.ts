import { defineConfig } from '@playwright/test';

const uiBaseUrl = process.env.PLAYWRIGHT_BASE_URL ?? 'http://localhost:5173';

export default defineConfig({
    testDir: './tests/e2e',
    timeout: 30_000,
    expect: {
        timeout: 7_500
    },
    fullyParallel: true,
    retries: process.env.CI ? 1 : 0,
    reporter: [['list'], ['html', { open: 'never' }]],
    use: {
        baseURL: uiBaseUrl,
        trace: 'on-first-retry',
        screenshot: 'only-on-failure'
    }
});
