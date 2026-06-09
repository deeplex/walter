import { expect, test } from '@playwright/test';
import { authHeader, signInApi } from './auth';
import { getFullListAs } from './entities';
import { findPrintableAbrechnung, getGruppen } from './abrechnung';

/**
 * Regression tests for the Abrechnungslauf authorization hole: the
 * preview/print/book/gruppen endpoints used to act on arbitrary `wohnungIds`
 * with no per-wohnung authorization, letting any authenticated user read or
 * book other tenants' Betriebskostenabrechnungen (IDOR). They must now scope
 * to the caller's managed wohnungen.
 */

test.describe('Abrechnungslauf authorization', () => {
    test('a low-privilege user cannot list, preview, print or book foreign billing groups', async ({
        request
    }) => {
        const adminLogin = await signInApi(request, 'admin.dev');
        const allGroups = await getGruppen(request, adminLogin.token);
        expect(allGroups.length).toBeGreaterThan(0);

        const limitedWohnungen = new Set(
            (await getFullListAs(request, 'limited.dev', '/api/wohnungen')).map(
                (w) => w.id
            )
        );
        const foreign = allGroups.find((g) =>
            g.wohnungIds.every((id) => !limitedWohnungen.has(id))
        );
        expect(
            foreign,
            'expected a billing group that limited.dev does not manage'
        ).toBeTruthy();

        const limited = await signInApi(request, 'limited.dev');

        // The group listing is scoped: fewer than admin, never the foreign one.
        const limitedGroups = await getGruppen(request, limited.token);
        expect(limitedGroups.length).toBeLessThan(allGroups.length);
        expect(
            limitedGroups.some((g) => g.groupKey === foreign!.groupKey)
        ).toBeFalsy();

        // preview / print / book of a foreign group are all forbidden. (book is
        // denied, so no data is mutated.)
        const preview = await request.post('/api/abrechnungslauf/preview', {
            headers: authHeader(limited.token),
            data: { jahr: 2023, gruppen: [{ wohnungIds: foreign!.wohnungIds }] }
        });
        expect(preview.status()).toBe(403);

        const print = await request.post('/api/abrechnungslauf/print/pdf', {
            headers: authHeader(limited.token),
            data: { wohnungIds: foreign!.wohnungIds, jahr: 2023 }
        });
        expect(print.status()).toBe(403);

        const book = await request.post('/api/abrechnungslauf/book', {
            headers: authHeader(limited.token),
            data: { jahr: 2023, gruppen: [{ wohnungIds: foreign!.wohnungIds }] }
        });
        expect(book.status()).toBe(403);
    });

    test('an admin can list and render billing groups', async ({ request }) => {
        const admin = await signInApi(request, 'admin.dev');
        const groups = await getGruppen(request, admin.token);
        expect(groups.length).toBeGreaterThan(0);

        const printable = await findPrintableAbrechnung(request, admin.token);
        expect(
            printable,
            'admin should be able to render at least one billing group'
        ).toBeDefined();
    });
});
