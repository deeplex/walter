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
    Betriebskostenrechnungen einer Umlage: je Rechnung Betrag, bezahlt, offen und wie viel
    auf die Mieter verteilt ist. „+ Rechnung“ erfasst eine Rechnung, „+ Zahlung“ bezahlt
    sie; aufgeklappt die Verteilung und die Zahlungen mit „Rückgängig machen“. Der Link der
    Zeile führt zum Buchungssatz der Rechnung. Ein aufklappbarer Abschnitt „Rechnungen
    (Anzahl)“ mit dem offenen Betrag als Tag; Sprungziel `#rechnungen`.
-->
<script lang="ts">
    import { formatEuro } from '$walter/services/format';
    import {
        Button,
        InlineLoading,
        InlineNotification,
        Modal,
        Tag
    } from 'carbon-components-svelte';
    import WalterDataTableContainer from '../elements/WalterDataTableContainer.svelte';
    import { Add } from 'carbon-icons-svelte';
    import WalterZahlungsuebersicht from '../lists/WalterZahlungsuebersicht.svelte';
    import WalterZahlungsliste from '../lists/WalterZahlungsliste.svelte';
    import WalterZahlungErfassen from './WalterZahlungErfassen.svelte';
    import WalterBetriebskostenrechnung from './WalterBetriebskostenrechnung.svelte';
    import {
        rechnungsZahlungsEintrag,
        umlageRechnungZeilen,
        validateBetriebskostenrechnung,
        WalterBetriebskostenrechnungEntry,
        type WalterUmlageRechnung,
        type WalterUmlageRechnungen,
        type WalterZahlungsZeile
    } from '$walter/lib';
    import { walter_fetch, walter_post } from '$walter/services/requests';
    import { changeTracker } from '$walter/store';
    import { get } from 'svelte/store';
    import { createEventDispatcher } from 'svelte';

    export let umlageId: number;
    /** Bezeichnung der Umlage für Überschriften, z. B. der Betriebskostentyp. */
    export let bezeichnung: string;
    /** Vorbelegung für „+ Rechnung“ (Typ, Umlage, Betrag und Jahr der letzten Rechnung). */
    export let neueRechnung: Partial<WalterBetriebskostenrechnungEntry>;
    export let fetchImpl: typeof fetch;
    /** Rechnungen und Zahlungen erfassen, Zahlungen rückgängig machen. */
    export let bearbeitbar = false;

    /** `geaendert`: eine Rechnung wurde erfasst. */
    const dispatch = createEventDispatcher<{ geaendert: void }>();

    let daten: WalterUmlageRechnungen | undefined;
    let fehler: string | null = null;

    export async function refresh() {
        fehler = null;
        const response = await walter_fetch(
            fetchImpl,
            `/api/umlagen/${umlageId}/rechnungen`
        );
        if (response.ok)
            daten = (await response.json()) as WalterUmlageRechnungen;
        else fehler = await response.text();
    }

    $: (umlageId, refresh());

    $: zeilen = daten ? umlageRechnungZeilen(daten) : [];
    $: offen = daten
        ? daten.rechnungen.reduce((s, r) => s + Math.max(0, r.offen), 0)
        : 0;

    // ── + Zahlung ────────────────────────────────────────────────────────────
    let zahlungOffen = false;
    let zahlungFuer: WalterUmlageRechnung | undefined;
    function zahlungErfassen(e: CustomEvent<WalterZahlungsZeile>) {
        zahlungFuer = daten?.rechnungen.find(
            (r) => r.buchungssatzId === e.detail.erfassenFuer
        );
        zahlungOffen = !!zahlungFuer;
    }

    // ── + Rechnung ───────────────────────────────────────────────────────────
    let rechnungOffen = false;
    let rechnung: Partial<WalterBetriebskostenrechnungEntry> = {};
    let rechnungFehler: string | null = null;
    let busy = false;
    // Die Formularfelder melden Änderungen an den Seiten-Änderungszähler; beim
    // Schließen wird er wiederhergestellt, damit die Seite nicht als geändert gilt.
    let tracker = 0;
    let warOffen = false;
    function rechnungErfassen() {
        tracker = get(changeTracker);
        rechnung = { ...neueRechnung };
        rechnungFehler = null;
        rechnungOffen = true;
    }
    $: if (rechnungOffen && !warOffen) warOffen = true;
    else if (!rechnungOffen && warOffen) {
        warOffen = false;
        changeTracker.set(tracker);
    }

    async function rechnungSpeichern() {
        if (!validateBetriebskostenrechnung(rechnung) || busy) return;
        busy = true;
        rechnungFehler = null;
        try {
            const response = await walter_post(
                WalterBetriebskostenrechnungEntry.ApiURL,
                rechnung
            );
            if (response.ok) {
                rechnungOffen = false;
                await refresh();
                dispatch('geaendert');
            } else {
                rechnungFehler = await response.text();
            }
        } finally {
            busy = false;
        }
    }
</script>

<WalterDataTableContainer
    layout="accordion"
    accordionTitle="Rechnungen"
    count={zeilen.length}
    id="rechnungen"
>
    <svelte:fragment slot="titel">
        {#if daten && offen > 0.005}
            <Tag size="sm" type="red">{formatEuro(offen)} offen</Tag>
        {/if}
    </svelte:fragment>
    <svelte:fragment slot="aktionen">
        {#if bearbeitbar}
            <Button
                kind="ghost"
                size="small"
                icon={Add}
                on:click={rechnungErfassen}
            >
                Rechnung
            </Button>
        {/if}
    </svelte:fragment>

    {#if fehler}
        <InlineNotification
            kind="error"
            title="Rechnungen nicht verfügbar"
            subtitle={fehler}
            hideCloseButton
        />
    {:else if !daten}
        <InlineLoading description="Lade Rechnungen…" />
    {:else}
        <WalterZahlungsuebersicht
            rows={zeilen}
            leerText="Noch keine Rechnung."
            periodeTitel="Rechnung"
            faelligTitel="Betrag"
            zusatzTitel="Auf Mieter verteilt"
            {bearbeitbar}
            {fetchImpl}
            on:zahlung={zahlungErfassen}
            on:geaendert={refresh}
        />
        <WalterZahlungsliste
            titel="Rückgängig gemachte Zahlungen"
            eintraege={daten.rueckgaengigGemacht.map(rechnungsZahlungsEintrag)}
            rueckgaengigGemacht
        />
    {/if}
</WalterDataTableContainer>

{#if bearbeitbar && zahlungFuer}
    <WalterZahlungErfassen
        {fetchImpl}
        ziel={{ art: 'bkRechnung', buchungssatzId: zahlungFuer.buchungssatzId }}
        titel={`${bezeichnung} ${zahlungFuer.jahr}`}
        vorschlag={zahlungFuer.offen}
        bind:open={zahlungOffen}
        on:gebucht={refresh}
    />
{/if}

{#if bearbeitbar}
    <Modal
        bind:open={rechnungOffen}
        modalHeading={`Rechnung erfassen – ${bezeichnung}`}
        primaryButtonText="Speichern"
        secondaryButtonText="Abbrechen"
        primaryButtonDisabled={!validateBetriebskostenrechnung(rechnung) ||
            busy}
        hasScrollingContent
        on:click:button--secondary={() => (rechnungOffen = false)}
        on:submit={rechnungSpeichern}
    >
        {#if rechnungOffen}
            <WalterBetriebskostenrechnung {fetchImpl} bind:entry={rechnung} />
            {#if rechnungFehler}
                <InlineNotification
                    kind="error"
                    title="Speichern nicht möglich:"
                    subtitle={rechnungFehler}
                    hideCloseButton
                    lowContrast
                />
            {/if}
        {/if}
    </Modal>
{/if}
