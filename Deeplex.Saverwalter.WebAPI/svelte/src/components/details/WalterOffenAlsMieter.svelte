<!-- Copyright (C) 2023-2026  Kai Lawrence -->
<!--
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
-->

<!--
    Was ein Kontakt als Mieter offen hat, über alle seine Verträge: je Vertrag die offene
    Miete (Mietkonto) und offene Nachzahlungen aus versendeten Nebenkostenabrechnungen,
    darunter die Summe. Die Zeile führt zum Mietkonto bzw. zu den Abrechnungen.
-->
<script lang="ts">
    import {
        InlineLoading,
        StructuredList,
        StructuredListBody,
        StructuredListCell,
        StructuredListHead,
        StructuredListRow,
        Tag,
        Tile
    } from 'carbon-components-svelte';
    import type {
        WalterMietkonto,
        WalterNebenkostenabrechnungen,
        WalterSelectionEntry
    } from '$walter/lib';
    import { walter_fetch } from '$walter/services/requests';

    /** Verträge, in denen der Kontakt Mieter ist (Text für die Zeile). */
    export let vertraege: WalterSelectionEntry[];
    export let fetchImpl: typeof fetch;

    type Zeile = { vertrag: WalterSelectionEntry; miete: number; nk: number };
    let zeilen: Zeile[] | undefined;

    async function json<T>(url: string): Promise<T | undefined> {
        const r = await walter_fetch(fetchImpl, url);
        return r.ok ? ((await r.json()) as T) : undefined;
    }

    async function lade(liste: WalterSelectionEntry[]) {
        zeilen = await Promise.all(
            liste.map(async (vertrag) => {
                const [konto, abrechnungen] = await Promise.all([
                    json<WalterMietkonto>(
                        `/api/vertraege/${vertrag.id}/mietkonto`
                    ),
                    json<WalterNebenkostenabrechnungen>(
                        `/api/vertraege/${vertrag.id}/abrechnungen`
                    )
                ]);
                const nk = (abrechnungen?.abrechnungen ?? [])
                    .filter((a) => a.status === 'Abgesendet' && a.saldo > 0)
                    .reduce((s, a) => s + a.offen, 0);
                return {
                    vertrag,
                    miete: Math.max(0, konto?.offen ?? 0),
                    nk
                };
            })
        );
    }

    $: lade(vertraege);

    $: summe = (zeilen ?? []).reduce((s, z) => s + z.miete + z.nk, 0);

    const eur = (v: number) =>
        v.toLocaleString('de-DE', { style: 'currency', currency: 'EUR' });
    const betragOderStrich = (v: number) => (v > 0.005 ? eur(v) : '—');
</script>

{#if vertraege.length > 0}
    <Tile id="offen-als-mieter" style="margin-bottom: 1rem">
        <h4 style="margin-bottom: 0.75rem">
            Offen als Mieter
            {#if zeilen}
                {#if summe > 0.005}
                    <Tag type="red">{eur(summe)} offen</Tag>
                {:else}
                    <Tag type="green">Nichts offen</Tag>
                {/if}
            {/if}
        </h4>
        {#if !zeilen}
            <InlineLoading description="Lade Mietkonten…" />
        {:else}
            <StructuredList condensed>
                <StructuredListHead>
                    <StructuredListRow head>
                        <StructuredListCell head>Vertrag</StructuredListCell>
                        <StructuredListCell head>Miete offen</StructuredListCell
                        >
                        <StructuredListCell head
                            >Nachzahlung offen</StructuredListCell
                        >
                    </StructuredListRow>
                </StructuredListHead>
                <StructuredListBody>
                    {#each zeilen as z (z.vertrag.id)}
                        <StructuredListRow>
                            <StructuredListCell>
                                <a href={`/vertraege/${z.vertrag.id}#mietkonto`}
                                    >{z.vertrag.text}</a
                                >
                            </StructuredListCell>
                            <StructuredListCell
                                >{betragOderStrich(z.miete)}</StructuredListCell
                            >
                            <StructuredListCell>
                                {#if z.nk > 0.005}
                                    <a
                                        href={`/vertraege/${z.vertrag.id}#abrechnungen`}
                                        >{eur(z.nk)}</a
                                    >
                                {:else}
                                    —
                                {/if}
                            </StructuredListCell>
                        </StructuredListRow>
                    {/each}
                </StructuredListBody>
            </StructuredList>
        {/if}
    </Tile>
{/if}
