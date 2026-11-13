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
    Nebenkostenabrechnungen eines Vertrags: je Abrechnung die Nachzahlung bzw. Erstattung,
    bezahlt und offen. „+ Zahlung“ bei versendeten, offenen Abrechnungen; aufgeklappt die
    Zahlungen mit „Rückgängig machen“. Der Link der Zeile führt zur Abrechnung.
-->
<script lang="ts">
    import {
        InlineLoading,
        InlineNotification,
        Tag,
        Tile
    } from 'carbon-components-svelte';
    import WalterZahlungsuebersicht from '../lists/WalterZahlungsuebersicht.svelte';
    import WalterZahlungsliste from '../lists/WalterZahlungsliste.svelte';
    import WalterZahlungErfassen from './WalterZahlungErfassen.svelte';
    import {
        abrechnungsZahlungsEintrag,
        abrechnungsZeilen,
        type WalterNebenkostenabrechnung,
        type WalterNebenkostenabrechnungen,
        type WalterZahlungsZeile
    } from '$walter/lib';
    import { walter_fetch } from '$walter/services/requests';

    export let vertragId: number;
    export let fetchImpl: typeof fetch;
    /** Zahlungen erfassen und rückgängig machen. */
    export let bearbeitbar = false;

    let daten: WalterNebenkostenabrechnungen | undefined;
    let fehler: string | null = null;

    export async function refresh() {
        if (!vertragId) return;
        fehler = null;
        const response = await walter_fetch(
            fetchImpl,
            `/api/vertraege/${vertragId}/abrechnungen`
        );
        if (response.ok)
            daten = (await response.json()) as WalterNebenkostenabrechnungen;
        else fehler = await response.text();
    }

    $: (vertragId, refresh());

    $: zeilen = daten ? abrechnungsZeilen(daten) : [];
    const offen = (liste: WalterNebenkostenabrechnung[], erstattung: boolean) =>
        liste
            .filter(
                (a) => a.status === 'Abgesendet' && a.saldo < 0 === erstattung
            )
            .reduce((s, a) => s + a.offen, 0);
    $: nachzahlungOffen = daten ? offen(daten.abrechnungen, false) : 0;
    $: erstattungOffen = daten ? offen(daten.abrechnungen, true) : 0;

    const eur = (v: number) =>
        v.toLocaleString('de-DE', { style: 'currency', currency: 'EUR' });

    let erfassenOffen = false;
    let erfassen: WalterNebenkostenabrechnung | undefined;
    function zahlungErfassen(e: CustomEvent<WalterZahlungsZeile>) {
        erfassen = daten?.abrechnungen.find(
            (a) => a.abrechnungsresultatId === e.detail.erfassenFuer
        );
        erfassenOffen = !!erfassen;
    }
</script>

<Tile id="abrechnungen" style="margin-bottom: 1rem">
    <h4 style="margin-bottom: 0.75rem">
        Nebenkostenabrechnungen
        {#if daten}
            {#if nachzahlungOffen > 0.005}
                <Tag type="red">{eur(nachzahlungOffen)} Nachzahlung offen</Tag>
            {/if}
            {#if erstattungOffen > 0.005}
                <Tag type="blue">{eur(erstattungOffen)} Erstattung offen</Tag>
            {/if}
            {#if nachzahlungOffen <= 0.005 && erstattungOffen <= 0.005 && zeilen.length > 0}
                <Tag type="green">Alles ausgeglichen</Tag>
            {/if}
        {/if}
    </h4>

    {#if fehler}
        <InlineNotification
            kind="error"
            title="Abrechnungen nicht verfügbar"
            subtitle={fehler}
            hideCloseButton
        />
    {:else if !daten}
        <InlineLoading description="Lade Abrechnungen…" />
    {:else}
        <WalterZahlungsuebersicht
            rows={zeilen}
            leerText="Noch keine Abrechnung."
            {bearbeitbar}
            {fetchImpl}
            on:zahlung={zahlungErfassen}
            on:geaendert={refresh}
        />
        <WalterZahlungsliste
            titel="Rückgängig gemachte Zahlungen"
            eintraege={daten.rueckgaengigGemacht.map(
                abrechnungsZahlungsEintrag
            )}
            rueckgaengigGemacht
        />
    {/if}
</Tile>

{#if bearbeitbar && erfassen}
    <WalterZahlungErfassen
        {fetchImpl}
        ziel={{
            art: 'abrechnung',
            abrechnungsresultatId: erfassen.abrechnungsresultatId,
            erstattung: erfassen.saldo < 0
        }}
        titel={`${erfassen.saldo < 0 ? 'Erstattung' : 'Nachzahlung'} Abrechnung ${erfassen.jahr}`}
        vorschlag={erfassen.offen}
        vermieterBankkontoId={daten?.vermieterBankkontoId ?? undefined}
        bind:open={erfassenOffen}
        on:gebucht={refresh}
    />
{/if}
