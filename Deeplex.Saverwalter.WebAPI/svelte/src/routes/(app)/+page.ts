import type { PageLoad } from "./stack/$types";

export const load: PageLoad = async ({ fetch }) => {
    return {
        fetch: fetch,
    };
};
