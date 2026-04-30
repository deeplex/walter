import { defineConfig } from '@playwright/test';

const baseURL = process.env.PLAYWRIGHT_BASE_URL ?? 'http://localhost:5254';

export default defineConfig({
    testDir: './tests/e2e',
    timeout: process.env.CI ? 60_000 : 30_000,
    expect: {
        timeout: process.env.CI ? 15_000 : 7_500
    },
    fullyParallel: process.env.CI ? false : true,
    workers: process.env.CI ? 1 : undefined,
    retries: process.env.CI ? 1 : 0,
    reporter: [['list'], ['html', { open: 'never' }]],
    use: {
        baseURL,
        trace: 'on-first-retry',
        screenshot: 'only-on-failure'
    }
});
