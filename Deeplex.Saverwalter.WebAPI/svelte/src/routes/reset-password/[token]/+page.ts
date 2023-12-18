import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params }) => {
    const apiURL = `/api/user/reset-password`;

    return {
        apiURL,
        token: params.token
    };
};
