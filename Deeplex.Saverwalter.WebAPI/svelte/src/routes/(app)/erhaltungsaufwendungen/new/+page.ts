import type { PageLoad } from './$types';

export const load: PageLoad = async ({ fetch }) => {
    return {
        fetchImpl: fetch,
        apiURL: `/api/erhaltungsaufwendungen`,
        title: 'Neue Erhaltungsaufwendung'
    };
};
