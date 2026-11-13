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
    Mit `garageVertragId` statt `vertragId` das Garagenmietkonto: nur die Garagenmiete
    je Monat.
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
    import WalterZahlungsliste from '../lists/WalterZahlungsliste.svelte';
    import WalterZahlungErfassen from './WalterZahlungErfassen.svelte';
    import {
        mietkontoZeilen,
        zahlungsEintrag,
        type WalterMietkonto,
        type WalterZahlungsZeile,
        type WalterZahlungsZiel
    } from '$walter/lib';
    import { walter_fetch } from '$walter/services/requests';

    export let vertragId: number | undefined = undefined;
    export let garageVertragId: number | undefined = undefined;
    export let fetchImpl: typeof fetch;
    export let jahr: number | undefined = undefined;
    export let title = 'Mietkonto';
    /** Zahlungen erfassen und rückgängig machen. */
    export let bearbeitbar = false;

    let erfassenOffen = false;
    let erfassenZeile: WalterZahlungsZeile | undefined;
    function zahlungErfassen(e: CustomEvent<WalterZahlungsZeile>) {
        erfassenZeile = e.detail;
        erfassenOffen = true;
    }

    let konto: WalterMietkonto | undefined;
    let fehler: string | null = null;

    const eur = (v: number) =>
        v.toLocaleString('de-DE', { style: 'currency', currency: 'EUR' });

    $: url = vertragId
        ? `/api/vertraege/${vertragId}/mietkonto`
        : garageVertragId
          ? `/api/garage-vertraege/${garageVertragId}/mietkonto`
          : undefined;

    export async function refresh() {
        if (!url) return;
        fehler = null;
        const response = await walter_fetch(fetchImpl, url);
        if (response.ok) konto = (await response.json()) as WalterMietkonto;
        else fehler = await response.text();
    }

    $: (url, refresh());

    function zielFuer(monat: string): WalterZahlungsZiel | undefined {
        if (vertragId) return { art: 'miete', vertragId, monat };
        if (garageVertragId) return { art: 'garage', garageVertragId, monat };
        return undefined;
    }
    $: erfassenZiel = erfassenZeile?.erfassenFuer
        ? zielFuer(erfassenZeile.erfassenFuer)
        : undefined;

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

<Tile id="mietkonto" style="margin-bottom: 1rem">
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

    <slot />

    {#if jahr === undefined && vertragId}
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
        <WalterZahlungsuebersicht
            rows={zeilen}
            {bearbeitbar}
            {fetchImpl}
            on:zahlung={zahlungErfassen}
            on:geaendert={refresh}
        />

        <WalterZahlungsliste
            titel="Guthaben (keinem Monat zugeordnet)"
            eintraege={nichtZugeordnet.map(zahlungsEintrag)}
            {bearbeitbar}
            {fetchImpl}
            on:erledigt={refresh}
        />
        <WalterZahlungsliste
            titel="Rückgängig gemachte Zahlungen"
            eintraege={rueckgaengig.map(zahlungsEintrag)}
            rueckgaengigGemacht
        />
    {/if}
</Tile>

{#if bearbeitbar && erfassenZeile && erfassenZiel}
    <WalterZahlungErfassen
        {fetchImpl}
        ziel={erfassenZiel}
        titel={erfassenZeile.periode}
        vorschlag={Math.max(0, erfassenZeile.offen)}
        bind:open={erfassenOffen}
        on:gebucht={refresh}
    />
{/if}
