import { expect, type APIRequestContext } from '@playwright/test';
import { authHeader } from './auth';

/**
 * A billing group as returned by `GET /api/abrechnungslauf/gruppen`. The
 * Betriebskostenabrechnung print/preview endpoints require the submitted
 * `wohnungIds` to match a complete group exactly.
 */
export type AbrechnungsGruppe = {
    groupKey: string;
    wohnungIds: number[];
    bezeichnung: string;
};

/** Years (newest first) to probe for renderable abrechnung data in the dev seed. */
export const CANDIDATE_YEARS = [2024, 2023, 2022, 2021, 2020, 2019];

export async function getGruppen(
    api: APIRequestContext,
    token: string
): Promise<AbrechnungsGruppe[]> {
    const response = await api.get('/api/abrechnungslauf/gruppen', {
        headers: authHeader(token)
    });
    expect(
        response.ok(),
        'GET /api/abrechnungslauf/gruppen should succeed'
    ).toBeTruthy();
    return (await response.json()) as AbrechnungsGruppe[];
}

export type PrintableAbrechnung = {
    gruppe: AbrechnungsGruppe;
    jahr: number;
    body: Buffer;
};

/**
 * Walks the billing groups and candidate years until one produces a printable
 * Betriebskostenabrechnung. Returns the group, year and the rendered bytes
 * (a single PDF for one-contract groups, otherwise a ZIP of PDFs).
 */
export async function findPrintableAbrechnung(
    api: APIRequestContext,
    token: string
): Promise<PrintableAbrechnung | undefined> {
    const gruppen = await getGruppen(api, token);
    for (const jahr of CANDIDATE_YEARS) {
        for (const gruppe of gruppen) {
            const response = await api.post('/api/abrechnungslauf/print/pdf', {
                headers: authHeader(token),
                data: { wohnungIds: gruppe.wohnungIds, jahr }
            });
            if (response.ok()) {
                return { gruppe, jahr, body: await response.body() };
            }
        }
    }
    return undefined;
}

/** A rendered abrechnung is either a raw PDF or a ZIP archive of PDFs. */
export function isPdfOrZip(body: Buffer): boolean {
    const head = body.subarray(0, 5).toString('latin1');
    return head === '%PDF-' || head.startsWith('PK');
}
