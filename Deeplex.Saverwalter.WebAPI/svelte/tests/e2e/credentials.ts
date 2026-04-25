export const devPassword = process.env.WALTER_E2E_PASSWORD ?? 'postgres';

export type DevUser = {
    username: string;
    expectedRole: 'Admin' | 'Owner' | 'Manager' | 'Viewer';
};

export const devUsers: DevUser[] = [
    { username: 'admin.dev', expectedRole: 'Admin' },
    { username: 'owner.dev', expectedRole: 'Owner' },
    { username: 'manager.dev', expectedRole: 'Manager' },
    { username: 'viewer.dev', expectedRole: 'Viewer' },
    { username: 'limited.dev', expectedRole: 'Viewer' }
];

export function authHeader(token: string): Record<string, string> {
    return {
        Authorization: `X-WalterToken ${token}`
    };
}
