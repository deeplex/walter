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
    Zahlungen mit „Rückgängig machen“. Der Link der Zeile führt zur Abrechnung. Ein
    aufklappbarer Abschnitt „Nebenkostenabrechnungen (Anzahl)“ mit dem Stand als Tags;
    Sprungziel `#abrechnungen`.
-->
<script lang="ts">
    import { formatEuro } from '$walter/services/format';
    import {
        InlineLoading,
        InlineNotification,
        Tag
    } from 'carbon-components-svelte';
    import WalterDataTableContainer from '../elements/WalterDataTableContainer.svelte';
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

    let erfassenOffen = false;
    let erfassen: WalterNebenkostenabrechnung | undefined;
    function zahlungErfassen(e: CustomEvent<WalterZahlungsZeile>) {
        erfassen = daten?.abrechnungen.find(
            (a) => a.abrechnungsresultatId === e.detail.erfassenFuer
        );
        erfassenOffen = !!erfassen;
    }
</script>

<WalterDataTableContainer
    layout="accordion"
    accordionTitle="Nebenkostenabrechnungen"
    count={zeilen.length}
    id="abrechnungen"
>
    <svelte:fragment slot="titel">
        {#if daten}
            {#if nachzahlungOffen > 0.005}
                <Tag size="sm" type="red"
                    >{formatEuro(nachzahlungOffen)} Nachzahlung offen</Tag
                >
            {/if}
            {#if erstattungOffen > 0.005}
                <Tag size="sm" type="blue"
                    >{formatEuro(erstattungOffen)} Erstattung offen</Tag
                >
            {/if}
            {#if nachzahlungOffen <= 0.005 && erstattungOffen <= 0.005 && zeilen.length > 0}
                <Tag size="sm" type="green">Alles ausgeglichen</Tag>
            {/if}
        {/if}
    </svelte:fragment>

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
            items={daten.rueckgaengigGemacht.map(abrechnungsZahlungsEintrag)}
            rueckgaengigGemacht
        />
    {/if}
</WalterDataTableContainer>

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
