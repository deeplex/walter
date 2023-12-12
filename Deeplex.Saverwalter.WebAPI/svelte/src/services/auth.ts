import { browser } from '$app/environment';
import type { WalterToastContent } from '$walter/lib';
import { addToast } from '$walter/store';
import { writable, type Writable } from 'svelte/store';

export enum UserRole {
    User = 0,
    Owner = 1,
    Admin = 2
}

type SignInRequest = {
    username: string;
    password: string;
};
export type SignInResponse = {
    userId: string;
    token: string;
    role: UserRole;
};

export type AuthState = {
    userId: string;
    token: string;
    role: UserRole;
};

export let authState: Writable<AuthState | null> | undefined = undefined;
let _accessToken: string | undefined = undefined;
export function getAuthState(): Writable<AuthState | null> {
    if (!browser) {
        throw Error("Don't call me during ssr");
    }
    if (authState === undefined) {
        authState = writable(tryLoadAuthState());
        authState.subscribe((state) => {
            if (state != null) {
                _accessToken = state.token;
                storeAuthState(state);
            }
        });
    }
    return authState;
}
export function getAccessToken(): string | undefined {
    if (browser && authState == null) {
        getAuthState();
    }
    return _accessToken;
}

export function walter_sign_out(toast?: WalterToastContent) {
    getAuthState().set(null);
    _accessToken = undefined;
    localStorage.removeItem('auth-state');
    toast && addToast(toast, true, 'Abmelden erfolgreich');
}

export async function walter_sign_in(
    fetchImpl: typeof fetch,
    username: string,
    password: string,
    toast?: WalterToastContent
): Promise<AuthState | null> {
    const response = await fetchImpl('/api/user/sign-in', {
        method: 'POST',
        body: JSON.stringify({ username, password } as SignInRequest),
        headers: { 'Content-Type': 'application/json' }
    });
    if (!(response.status === 200)) {
        toast && addToast(toast, false);
        return null;
    }
    const parsedAuthState: AuthState =
        (await response.json()) as SignInResponse;
    getAuthState().set(parsedAuthState);
    toast && addToast(toast, true, parsedAuthState.userId);
    return parsedAuthState;
}

function tryLoadAuthState(): AuthState | null {
    const serializedAuthState = localStorage.getItem('auth-state');
    if (serializedAuthState == null) {
        return null;
    }
    let authState: unknown = undefined;
    try {
        authState = JSON.parse(serializedAuthState);
    } catch (exc) {
        console.error(
            'Failed to deserialize the auth-state from localStorage',
            exc
        );
        return null;
    }
    if (
        typeof authState !== 'object' ||
        authState == null ||
        !('userId' in authState) ||
        typeof authState.userId !== 'string' ||
        !('token' in authState) ||
        typeof authState.token !== 'string' ||
        !('role' in authState) ||
        typeof authState.role !== 'number'
    ) {
        return null;
    }
    if (!isTokenStillValid(authState.token)) {
        console.error('removing auth-state');
        localStorage.removeItem('auth-state');
        return null;
    }
    return {
        userId: authState.userId,
        token: authState.token,
        role: authState.role
    };
}
function isTokenStillValid(token: string): boolean {
    try {
        const decodedToken = Uint8Array.from(atob(token), (c) =>
            c.charCodeAt(0)
        );
        const expiryDateIso8601Basic = new TextDecoder('utf-8', {
            fatal: true
        }).decode(decodedToken.subarray(32, 32 + 16));
        // I don't know who "designed" the js Date API and left out the basic
        // ISO 8601 format, but I do sincerely hate him.
        const basicPart = (start: number, length: number) =>
            expiryDateIso8601Basic.substring(start, start + length);
        // prettier-ignore
        const expiryDateIso8601Extended
            = `${basicPart(0, 4)}-${basicPart(4, 2)}-${basicPart(6, 5)}:${basicPart(11, 2)}:${basicPart(13, 3)}`;

        const expiry = new Date(expiryDateIso8601Extended);
        // _obviously_ a default constructed a Date object is initialized from
        // the current clock value.
        const now = new Date();
        // console.log(expiryDateIso8601Basic, expiryDateIso8601Extended, expiry, now);
        return now < expiry;
    } catch (_) {
        console.error('vali fail', _);
        return false;
    }
}
function storeAuthState(state: AuthState) {
    localStorage.setItem('auth-state', JSON.stringify(state));
}
