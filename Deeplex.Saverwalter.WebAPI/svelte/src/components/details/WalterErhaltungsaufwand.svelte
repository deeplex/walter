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
    Erhaltungsaufwand einer Wohnung: je Rechnung Betrag, bezahlt und offen. „+ Rechnung“
    erfasst eine Rechnung (noch nicht bezahlt), „+ Zahlung“ bezahlt eine erfasste Rechnung;
    aufgeklappt die Zahlungen mit „Rückgängig machen“. Darunter der Jahresüberblick:
    Mieteinnahmen, Erhaltungsaufwand und nicht verteilte Betriebskosten je Jahr.
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
    import WalterDataTable from '../elements/WalterDataTable.svelte';
    import { Add } from 'carbon-icons-svelte';
    import WalterZahlungsuebersicht from '../lists/WalterZahlungsuebersicht.svelte';
    import WalterZahlungErfassen from './WalterZahlungErfassen.svelte';
    import WalterErhaltungsaufwendung from './WalterErhaltungsaufwendung.svelte';
    import {
        erhaltungsaufwandZeilen,
        validateErhaltungsaufwendung,
        WalterErhaltungsaufwendungEntry,
        type WalterErhaltungsaufwand,
        type WalterErhaltungsaufwandRechnung,
        type WalterSelectionEntry,
        type WalterZahlungsZeile
    } from '$walter/lib';
    import { walter_fetch, walter_post } from '$walter/services/requests';
    import { convertDateCanadian } from '$walter/services/utils';
    import { changeTracker } from '$walter/store';
    import { get } from 'svelte/store';

    export let wohnung: WalterSelectionEntry;
    export let fetchImpl: typeof fetch;
    /** Rechnungen und Zahlungen erfassen, Zahlungen rückgängig machen. */
    export let bearbeitbar = false;

    let daten: WalterErhaltungsaufwand | undefined;
    let fehler: string | null = null;

    export async function refresh() {
        fehler = null;
        const response = await walter_fetch(
            fetchImpl,
            `/api/wohnungen/${wohnung.id}/erhaltungsaufwand`
        );
        if (response.ok)
            daten = (await response.json()) as WalterErhaltungsaufwand;
        else fehler = await response.text();
    }

    $: (wohnung.id, refresh());

    $: zeilen = daten ? erhaltungsaufwandZeilen(daten) : [];
    $: offen = daten
        ? daten.rechnungen.reduce((s, r) => s + Math.max(0, r.offen), 0)
        : 0;

    // ── + Zahlung ────────────────────────────────────────────────────────────
    let zahlungOffen = false;
    let zahlungFuer: WalterErhaltungsaufwandRechnung | undefined;
    function zahlungErfassen(e: CustomEvent<WalterZahlungsZeile>) {
        zahlungFuer = daten?.rechnungen.find(
            (r) => r.buchungssatzId === e.detail.erfassenFuer
        );
        zahlungOffen = !!zahlungFuer;
    }

    // ── + Rechnung ───────────────────────────────────────────────────────────
    let rechnungOffen = false;
    let rechnung: Partial<WalterErhaltungsaufwendungEntry> = {};
    let rechnungFehler: string | null = null;
    let busy = false;
    // Die Formularfelder melden Änderungen an den Seiten-Änderungszähler; beim
    // Schließen wird er wiederhergestellt, damit die Seite nicht als geändert gilt.
    let tracker = 0;
    function rechnungErfassen() {
        tracker = get(changeTracker);
        rechnung = {
            wohnung: { ...wohnung },
            datum: convertDateCanadian(new Date())
        };
        rechnungFehler = null;
        rechnungOffen = true;
    }
    let warOffen = false;
    $: if (rechnungOffen && !warOffen) warOffen = true;
    else if (!rechnungOffen && warOffen) {
        warOffen = false;
        changeTracker.set(tracker);
    }

    async function rechnungSpeichern() {
        if (!validateErhaltungsaufwendung(rechnung) || busy) return;
        busy = true;
        rechnungFehler = null;
        try {
            const response = await walter_post(
                WalterErhaltungsaufwendungEntry.ApiURL,
                rechnung
            );
            if (response.ok) {
                rechnungOffen = false;
                await refresh();
            } else {
                rechnungFehler = await response.text();
            }
        } finally {
            busy = false;
        }
    }

    // ── Jahresüberblick ──────────────────────────────────────────────────────
    const jahresKoepfe = [
        { key: 'wert', value: 'Jahr' },
        { key: 'mieteinnahmen', value: 'Mieteinnahmen' },
        { key: 'erhaltungsaufwand', value: 'Erhaltungsaufwand' },
        {
            key: 'betriebskostenEigenanteil',
            value: 'Betriebskosten (nicht verteilt)'
        }
    ].map((h) => ({ ...h, sort: false as const }));
    // Beträge vorformatiert; die Tabelle sortiert nicht.
    $: jahresZeilen = (daten?.jahre ?? []).map((j) => ({
        id: j.wert,
        wert: `${j.wert}`,
        mieteinnahmen: formatEuro(j.mieteinnahmen),
        erhaltungsaufwand: formatEuro(j.erhaltungsaufwand),
        betriebskostenEigenanteil: formatEuro(j.betriebskostenEigenanteil)
    }));
</script>

<WalterDataTableContainer
    layout="accordion"
    accordionTitle="Erhaltungsaufwand"
    count={zeilen.length}
    id="erhaltungsaufwand"
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
            title="Erhaltungsaufwand nicht verfügbar"
            subtitle={fehler}
            hideCloseButton
        />
    {:else if !daten}
        <InlineLoading description="Lade Erhaltungsaufwand…" />
    {:else}
        <WalterZahlungsuebersicht
            rows={zeilen}
            leerText="Noch keine Rechnung."
            periodeTitel="Rechnung"
            faelligTitel="Betrag"
            {bearbeitbar}
            {fetchImpl}
            on:zahlung={zahlungErfassen}
            on:geaendert={refresh}
        />
        {#if jahresZeilen.length > 0}
            <h5 style="margin-top: 1.5rem">Jahresüberblick</h5>
            <WalterDataTable
                size="short"
                search={false}
                headers={jahresKoepfe}
                rows={jahresZeilen}
            />
        {/if}
    {/if}
</WalterDataTableContainer>

{#if bearbeitbar && zahlungFuer}
    <WalterZahlungErfassen
        {fetchImpl}
        ziel={{
            art: 'erhaltungsaufwand',
            buchungssatzId: zahlungFuer.buchungssatzId,
            wohnungId: Number(wohnung.id)
        }}
        titel={zahlungFuer.bezeichnung}
        vorschlag={zahlungFuer.offen}
        vermieterBankkontoId={daten?.vermieterBankkontoId ?? undefined}
        bind:open={zahlungOffen}
        on:gebucht={refresh}
    />
{/if}

{#if bearbeitbar}
    <Modal
        bind:open={rechnungOffen}
        modalHeading="Rechnung für Erhaltungsaufwand erfassen"
        primaryButtonText="Speichern"
        secondaryButtonText="Abbrechen"
        primaryButtonDisabled={!validateErhaltungsaufwendung(rechnung) || busy}
        hasScrollingContent
        on:click:button--secondary={() => (rechnungOffen = false)}
        on:submit={rechnungSpeichern}
    >
        {#if rechnungOffen}
            <WalterErhaltungsaufwendung {fetchImpl} bind:entry={rechnung} />
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
