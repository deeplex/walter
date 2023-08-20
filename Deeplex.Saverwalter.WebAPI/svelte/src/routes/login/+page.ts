import type { PageLoad } from './$types';
import { browser } from '$app/environment';
import { walter_goto } from '$walter/services/utils';

export const load: PageLoad = async ({ fetch }) => {
    if (!browser) {
        return {
            fetch
        };
    }

    const accessToken = (
        await import('$walter/services/auth')
    ).getAccessToken();
    if (accessToken == null) {
        return {
            fetch
        };
    }
    const response = await fetch('/api/account/refresh-token', {
        method: 'POST',
        headers: {
            Authorization: `X-WalterToken ${accessToken}`
        }
    });
    if (response.status === 200) {
        await walter_goto('/');
    }

    return {
        fetch
    };
};
