import { expect, test, type APIRequestContext } from '@playwright/test';
import { authHeader, signInApi } from './auth';

type WalterFileEntry = {
    fileName: string;
    key: string;
    lastModified: number;
    size: number | null;
    type: string;
    blob: number[] | null;
};

type WohnungListEntry = {
    id: number;
    permissions: { read: boolean; update: boolean; remove: boolean };
};

async function uniqueFileName(prefix: string, ext = 'pdf'): Promise<string> {
    return `${prefix}-${Date.now()}-${Math.floor(Math.random() * 1000)}.${ext}`;
}

async function listFiles(
    api: APIRequestContext,
    token: string,
    url: string
): Promise<WalterFileEntry[]> {
    const response = await api.get(url, { headers: authHeader(token) });
    expect(response.ok()).toBeTruthy();
    return (await response.json()) as WalterFileEntry[];
}

async function putFile(
    api: APIRequestContext,
    token: string,
    url: string,
    body: Buffer,
    contentType: string
): Promise<number> {
    const response = await api.put(url, {
        headers: { ...authHeader(token), 'Content-Type': contentType },
        data: body
    });
    return response.status();
}

async function deleteFile(
    api: APIRequestContext,
    token: string,
    url: string
): Promise<number> {
    const response = await api.delete(url, { headers: authHeader(token) });
    return response.status();
}

const HELLO_PDF = Buffer.from(
    '%PDF-1.4\n1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj\n' +
        '2 0 obj<</Type/Pages/Kids[3 0 R]/Count 1>>endobj\n' +
        '3 0 obj<</Type/Page/Parent 2 0 R/MediaBox[0 0 200 200]>>endobj\n' +
        'xref\n0 4\n0000000000 65535 f \n0000000009 00000 n \n' +
        '0000000056 00000 n \n0000000105 00000 n \n' +
        'trailer<</Size 4/Root 1 0 R>>\nstartxref\n160\n%%EOF\n'
);

test.describe('S3-backed file storage flow', () => {
    test('owner can upload, list, download and trash a wohnung file', async ({
        request
    }) => {
        const ownerLogin = await signInApi(request, 'owner.dev');
        const wohnungenResp = await request.get('/api/wohnungen', {
            headers: authHeader(ownerLogin.token)
        });
        expect(wohnungenResp.ok()).toBeTruthy();
        const wohnungen = (await wohnungenResp.json()) as WohnungListEntry[];
        const target = wohnungen.find((w) => w.permissions.update);
        expect(target, 'owner should be able to update at least one wohnung').toBeTruthy();

        const wohnungId = target!.id;
        const filesUrl = `/api/wohnungen/${wohnungId}/files`;
        const fileName = await uniqueFileName('playwright-upload');
        const uploadUrl = `${filesUrl}/${encodeURIComponent(fileName)}`;

        const beforeFiles = await listFiles(request, ownerLogin.token, filesUrl);

        const putStatus = await putFile(
            request,
            ownerLogin.token,
            uploadUrl,
            HELLO_PDF,
            'application/pdf'
        );
        expect(putStatus).toBe(200);

        const afterFiles = await listFiles(request, ownerLogin.token, filesUrl);
        expect(afterFiles.map((f) => f.fileName)).toContain(fileName);
        expect(afterFiles.length).toBe(beforeFiles.length + 1);

        const downloadResp = await request.get(uploadUrl, {
            headers: authHeader(ownerLogin.token)
        });
        expect(downloadResp.ok()).toBeTruthy();
        const downloaded = await downloadResp.body();
        expect(downloaded.subarray(0, 5).toString('latin1')).toBe('%PDF-');

        const deleteStatus = await deleteFile(request, ownerLogin.token, uploadUrl);
        expect(deleteStatus).toBe(200);

        const finalFiles = await listFiles(request, ownerLogin.token, filesUrl);
        expect(finalFiles.map((f) => f.fileName)).not.toContain(fileName);
    });

    test('viewer cannot upload to a read-only wohnung', async ({ request }) => {
        const viewerLogin = await signInApi(request, 'viewer.dev');
        const wohnungenResp = await request.get('/api/wohnungen', {
            headers: authHeader(viewerLogin.token)
        });
        expect(wohnungenResp.ok()).toBeTruthy();
        const wohnungen = (await wohnungenResp.json()) as WohnungListEntry[];
        const readOnly = wohnungen.find(
            (w) => w.permissions.read && !w.permissions.update
        );
        expect(readOnly, 'viewer should have a readable but read-only wohnung').toBeTruthy();

        const fileName = await uniqueFileName('playwright-viewer');
        const status = await putFile(
            request,
            viewerLogin.token,
            `/api/wohnungen/${readOnly!.id}/files/${fileName}`,
            HELLO_PDF,
            'application/pdf'
        );
        expect(status).toBe(403);
    });

    test('seeded sample files are visible on at least one entity', async ({
        request
    }) => {
        const adminLogin = await signInApi(request, 'admin.dev');
        const wohnungenResp = await request.get('/api/wohnungen', {
            headers: authHeader(adminLogin.token)
        });
        expect(wohnungenResp.ok()).toBeTruthy();
        const wohnungen = (await wohnungenResp.json()) as WohnungListEntry[];

        let foundFile = false;
        // The seeder uploads to the first ~15 wohnungen by id; checking the
        // first 20 in the list is plenty for both seed configurations.
        for (const wohnung of wohnungen.slice(0, 20)) {
            const files = await listFiles(
                request,
                adminLogin.token,
                `/api/wohnungen/${wohnung.id}/files`
            );
            if (files.length > 0) {
                foundFile = true;
                break;
            }
        }

        expect(
            foundFile,
            'expected the dev seed to have uploaded sample files; ' +
                'is MinIO running and was bootstrap-dev.sh executed with S3 reachable?'
        ).toBeTruthy();
    });

    test('user stack endpoint accepts upload and listing', async ({ request }) => {
        const adminLogin = await signInApi(request, 'admin.dev');
        const fileName = await uniqueFileName('stack-test');
        const uploadUrl = `/api/user/files/${encodeURIComponent(fileName)}`;

        const putStatus = await putFile(
            request,
            adminLogin.token,
            uploadUrl,
            HELLO_PDF,
            'application/pdf'
        );
        expect(putStatus).toBe(200);

        const listResp = await request.get('/api/user/files', {
            headers: authHeader(adminLogin.token)
        });
        expect(listResp.ok()).toBeTruthy();
        const stackFiles = (await listResp.json()) as WalterFileEntry[];
        expect(stackFiles.map((f) => f.fileName)).toContain(fileName);

        const deleteStatus = await deleteFile(request, adminLogin.token, uploadUrl);
        expect(deleteStatus).toBe(200);
    });
});

test.describe('Abrechnung PDF generation and download', () => {
    test('owner can generate a Betriebskostenabrechnung PDF for a contract', async ({
        request
    }) => {
        const ownerLogin = await signInApi(request, 'owner.dev');

        const vertraegeResp = await request.get('/api/vertraege', {
            headers: authHeader(ownerLogin.token)
        });
        expect(vertraegeResp.ok()).toBeTruthy();
        const vertraege = (await vertraegeResp.json()) as Array<{
            id: number;
            permissions: { update: boolean };
            beginn?: string;
            ende?: string | null;
        }>;
        expect(vertraege.length).toBeGreaterThan(0);

        // Try contracts in order until one returns a renderable abrechnung.
        // Generated test data always contains some unfinished contracts where
        // certain years are empty, so we walk a few candidates.
        let resp: { status: number; body: Buffer; vertragId: number; jahr: number } | undefined;
        const editable = vertraege.filter((v) => v.permissions.update).slice(0, 12);
        for (const vertrag of editable) {
            const beginn = vertrag.beginn ? new Date(vertrag.beginn) : null;
            const ende = vertrag.ende ? new Date(vertrag.ende) : new Date();
            if (!beginn) continue;
            const startYear = beginn.getFullYear();
            const endYear = ende.getFullYear();
            for (let jahr = endYear; jahr >= startYear; jahr--) {
                const url = `/api/betriebskostenabrechnung/${vertrag.id}/${jahr}/pdf_document`;
                const r = await request.get(url, {
                    headers: authHeader(ownerLogin.token)
                });
                if (r.ok()) {
                    resp = {
                        status: r.status(),
                        body: await r.body(),
                        vertragId: vertrag.id,
                        jahr
                    };
                    break;
                }
            }
            if (resp) break;
        }

        expect(
            resp,
            'expected the dev seed to contain at least one contract with renderable abrechnung data'
        ).toBeDefined();
        expect(resp!.status).toBe(200);
        expect(resp!.body.length).toBeGreaterThan(100);
        expect(resp!.body.subarray(0, 5).toString('latin1')).toBe('%PDF-');
    });

    test('non-existing contract reports a clean error without 5xx', async ({
        request
    }) => {
        const ownerLogin = await signInApi(request, 'owner.dev');
        const url = `/api/betriebskostenabrechnung/99999999/2024/pdf_document`;
        const r = await request.get(url, {
            headers: authHeader(ownerLogin.token)
        });
        expect(r.status()).toBeLessThan(500);
    });
});
