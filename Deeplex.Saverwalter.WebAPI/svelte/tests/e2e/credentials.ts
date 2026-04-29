export const devPassword = process.env.WALTER_E2E_PASSWORD ?? 'postgres';

const userRole = {
    Guest: 0,
    User: 1,
    Admin: 2,
    Owner: 3
} as const;

export type DevUser = {
    username: string;
    expectedRole: (typeof userRole)[keyof typeof userRole];
};

export const devUsers: DevUser[] = [
    { username: 'admin.dev', expectedRole: userRole.Admin },
    { username: 'owner.dev', expectedRole: userRole.Owner },
    { username: 'manager.dev', expectedRole: userRole.User },
    { username: 'viewer.dev', expectedRole: userRole.User },
    { username: 'limited.dev', expectedRole: userRole.Guest }
];
