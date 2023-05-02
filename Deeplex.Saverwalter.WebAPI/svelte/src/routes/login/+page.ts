import { goto } from '$app/navigation';
import type { PageLoad } from './$types';
import { browser } from '$app/environment';

export const load: PageLoad = async ({ fetch }) => {
  if (!browser) {
    return {
      fetch
    };
  }

  const accessToken = (await import('$WalterServices/auth')).getAccessToken();
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
  if (response.ok) {
    await goto('/');
  }

  return {
    fetch
  };
};