import { walter_post } from "$WalterServices/requests";
import { goto } from "$app/navigation";
import type { PageLoad } from "./$types";
import Cookies from "js-cookie";

export const load: PageLoad = async () => {
    const access_token = Cookies.get('access_token');
    if (access_token) {
        const response = await walter_post(
            '/api/login',
            { token: access_token },
            undefined
        );
        if (response.succeeded) {
            goto('/');
        }
    }
}