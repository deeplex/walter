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
    Mietkonto eines Vertrags: Kaltmiete und Garagen je Monat, NK-Vorauszahlungen je
    Jahr. Mit `jahr` nur dieses Jahr (Abrechnungslauf), dann ohne Miet-Korrektur-Hinweis.
-->
<script lang="ts">
    import {
        InlineLoading,
        InlineNotification,
        Tag,
        Tile
    } from 'carbon-components-svelte';
    import WalterZahlungsuebersicht from '../lists/WalterZahlungsuebersicht.svelte';
    import WalterMietKorrektur from './WalterMietKorrektur.svelte';
    import {
        mietkontoZeilen,
        zahlungsEintrag,
        type WalterMietkonto
    } from '$walter/lib';
    import { walter_fetch } from '$walter/services/requests';

    export let vertragId: number;
    export let fetchImpl: typeof fetch;
    export let jahr: number | undefined = undefined;
    export let title = 'Mietkonto';

    let konto: WalterMietkonto | undefined;
    let fehler: string | null = null;

    const eur = (v: number) =>
        v.toLocaleString('de-DE', { style: 'currency', currency: 'EUR' });
    const datumText = (d: string) => new Date(d).toLocaleDateString('de-DE');

    export async function refresh() {
        if (!vertragId) return;
        fehler = null;
        const response = await walter_fetch(
            fetchImpl,
            `/api/vertraege/${vertragId}/mietkonto`
        );
        if (response.ok) konto = (await response.json()) as WalterMietkonto;
        else fehler = await response.text();
    }

    $: (vertragId, refresh());

    $: zeilen = konto ? mietkontoZeilen(konto, jahr) : [];
    $: offen = zeilen.reduce((s, z) => s + z.offen, 0);
    $: imJahr = <T extends { datum: string }>(liste: T[]) =>
        jahr === undefined
            ? liste
            : liste.filter((z) => new Date(z.datum).getFullYear() === jahr);
    $: nichtZugeordnet = konto ? imJahr(konto.nichtZugeordnet) : [];
    $: guthaben = nichtZugeordnet.reduce((s, z) => s + z.betrag, 0);
    $: gesamt = offen - guthaben;
    $: rueckgaengig = konto ? imJahr(konto.rueckgaengigGemacht) : [];
</script>

<Tile style="margin-bottom: 1rem">
    <h4 style="margin-bottom: 0.75rem">
        {title}
        {#if konto}
            {#if gesamt > 0.005}
                <Tag type="red">{eur(gesamt)} offen</Tag>
            {:else if gesamt < -0.005}
                <Tag type="blue">{eur(-gesamt)} Guthaben</Tag>
            {:else}
                <Tag type="green">Alles bezahlt</Tag>
            {/if}
        {/if}
    </h4>

    {#if jahr === undefined}
        <WalterMietKorrektur {vertragId} {fetchImpl} on:geaendert={refresh} />
    {/if}

    {#if fehler}
        <InlineNotification
            kind="error"
            title="Mietkonto nicht verfügbar"
            subtitle={fehler}
            hideCloseButton
        />
    {:else if !konto}
        <InlineLoading description="Lade Mietkonto…" />
    {:else}
        <WalterZahlungsuebersicht rows={zeilen} />

        {#if nichtZugeordnet.length > 0}
            <h5 style="margin-top: 1rem">Guthaben (keinem Monat zugeordnet)</h5>
            <ul class="liste">
                {#each nichtZugeordnet.map(zahlungsEintrag) as z (z.id)}
                    <li>
                        <span>{datumText(z.datum)}</span>
                        <span>{eur(z.betrag)}</span>
                        <span class="text">{z.text}</span>
                        <a href={z.href}>Details →</a>
                    </li>
                {/each}
            </ul>
        {/if}

        {#if rueckgaengig.length > 0}
            <h5 style="margin-top: 1rem">Rückgängig gemachte Zahlungen</h5>
            <ul class="liste">
                {#each rueckgaengig.map(zahlungsEintrag) as z (z.id)}
                    <li>
                        <span>{datumText(z.datum)}</span>
                        <span>{eur(z.betrag)}</span>
                        <span class="text">{z.text}</span>
                        <Tag size="sm" type="gray">Rückgängig gemacht</Tag>
                        <a href={z.href}>Details →</a>
                    </li>
                {/each}
            </ul>
        {/if}
    {/if}
</Tile>

<style>
    .liste {
        list-style: none;
        margin: 0.5rem 0 0 0;
        padding: 0;
    }
    .liste li {
        display: flex;
        gap: 1rem;
        flex-wrap: wrap;
        align-items: center;
        padding: 0.25rem 0;
    }
    .text {
        flex: 1;
    }
</style>
