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
    Mietkonto eines Vertrags: je Monat das Soll (Kaltmiete und Garagen) gegen die
    Transaktionen, die es bezahlt haben, samt deren NK-Vorauszahlungs-Anteil; aufgeklappt
    jede Transaktion mit ihrer Zuordnung. Auf der Vertragsseite ein aufklappbarer Abschnitt
    „Mietkonto (Monate)“ mit dem Stand als Tag, 12 Monate je Seite; offen, wenn die Adresse
    auf `#mietkonto` zeigt. Mit `jahr` nur dieses Jahr als Kachel (Abrechnungslauf), dann
    ohne Miet-Korrektur-Hinweis.
    Mit `garageVertragId` statt `vertragId` das Garagenmietkonto: nur die Garagenmiete
    je Monat.
-->
<script lang="ts">
    import { formatEuro } from '$walter/services/format';
    import {
        InlineLoading,
        InlineNotification,
        Tag
    } from 'carbon-components-svelte';
    import WalterZahlungsuebersicht from '../lists/WalterZahlungsuebersicht.svelte';
    import WalterDataTableContainer from '../elements/WalterDataTableContainer.svelte';
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

    /** Auf der Vertrags- bzw. Garagenseite ein aufklappbarer Abschnitt, im Abrechnungslauf eine Kachel. */
    $: asSection = jahr === undefined;
    $: status = !konto
        ? undefined
        : gesamt > 0.005
          ? { tag: 'red' as const, text: `${formatEuro(gesamt)} offen` }
          : gesamt < -0.005
            ? { tag: 'blue' as const, text: `${formatEuro(-gesamt)} Guthaben` }
            : { tag: 'green' as const, text: 'Alles bezahlt' };
</script>

<WalterDataTableContainer
    layout={asSection ? 'accordion' : 'inline'}
    accordionTitle={title}
    count={zeilen.length}
    id={asSection ? 'mietkonto' : undefined}
>
    <svelte:fragment slot="titel">
        {#if status}
            <Tag size="sm" type={status.tag}>{status.text}</Tag>
        {/if}
    </svelte:fragment>
    <div
        id={asSection ? undefined : 'mietkonto'}
        class:bx--tile={!asSection}
        class:kachel={!asSection}
    >
        {#if !asSection}
            <h4 style="margin-bottom: 0.75rem">
                {title}
                {#if status}
                    <Tag type={status.tag}>{status.text}</Tag>
                {/if}
            </h4>
        {/if}

        <slot />

        {#if jahr === undefined && vertragId}
            <WalterMietKorrektur
                {vertragId}
                {fetchImpl}
                on:geaendert={refresh}
            />
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
                pageSize={12}
                faelligTitel="Soll"
                mitTransaktionen
                mitNk={!!vertragId}
                {bearbeitbar}
                {fetchImpl}
                on:zahlung={zahlungErfassen}
                on:geaendert={refresh}
            />

            <WalterZahlungsliste
                titel="Guthaben (keinem Monat zugeordnet)"
                items={nichtZugeordnet.map(zahlungsEintrag)}
                {bearbeitbar}
                {fetchImpl}
                on:erledigt={refresh}
            />
            <WalterZahlungsliste
                titel="Rückgängig gemachte Zahlungen"
                items={rueckgaengig.map(zahlungsEintrag)}
                rueckgaengigGemacht
            />
        {/if}
    </div>
</WalterDataTableContainer>

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

<style>
    .kachel {
        margin-bottom: 1rem;
    }
</style>
