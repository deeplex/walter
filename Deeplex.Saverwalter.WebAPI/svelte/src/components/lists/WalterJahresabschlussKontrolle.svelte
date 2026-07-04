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

<script lang="ts">
    import {
        Accordion,
        AccordionItem,
        Content,
        DataTable,
        DataTableSkeleton,
        Dropdown,
        Tag,
        Tile
    } from 'carbon-components-svelte';
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
    import { onMount } from 'svelte';
    import {
        getJahresUebersicht,
        getJahresabschluss,
        type WalterJahresUebersicht,
        type WalterJahresabschluss,
        type WalterKontoJahres
    } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';
    import { convertEuro } from '$walter/services/utils';

    export let fetchImpl: typeof fetch;

    let uebersicht: WalterJahresUebersicht[] = [];
    let uebersichtLoaded = false;
    let selectedJahr: number | undefined;
    let abschluss: WalterJahresabschluss | undefined;
    let geladenesJahr: number | undefined;

    onMount(async () => {
        uebersicht = await getJahresUebersicht(fetchImpl);
        uebersichtLoaded = true;
        // Jüngstes Jahr mit offenen Punkten vorauswählen, sonst das neueste.
        selectedJahr = (
            uebersicht.find((u) => !u.abgeschlossen) ?? uebersicht[0]
        )?.jahr;
    });

    $: if (selectedJahr !== undefined && selectedJahr !== geladenesJahr) {
        geladenesJahr = selectedJahr;
        abschluss = undefined;
        getJahresabschluss(selectedJahr, fetchImpl).then((a) => {
            // Antwort verwerfen, wenn inzwischen ein anderes Jahr gewählt wurde.
            if (a.jahr === selectedJahr) abschluss = a;
        });
    }

    $: dropdownItems = uebersicht.map((u) => ({
        id: u.jahr,
        text: u.abgeschlossen
            ? `${u.jahr} — abgeschlossen`
            : `${u.jahr} — ${u.kontenOffen + u.abrechnungenOffen} offen`
    }));

    type KontoGruppe = {
        funktion: string;
        ausgleichbar: boolean;
        konten: WalterKontoJahres[];
        offeneKonten: number;
        offenerBetrag: number;
    };

    function gruppiere(konten: WalterKontoJahres[]): KontoGruppe[] {
        const map = new Map<string, WalterKontoJahres[]>();
        for (const konto of konten) {
            const key = konto.funktion ?? 'Ohne Funktion';
            const gruppe = map.get(key);
            if (gruppe) {
                gruppe.push(konto);
            } else {
                map.set(key, [konto]);
            }
        }
        // Konten kommen vom Backend bereits sortiert: ausgleichbare
        // Funktionen zuerst — die Map erhält diese Reihenfolge.
        return [...map.entries()].map(([funktion, gruppe]) => ({
            funktion,
            ausgleichbar: gruppe.some((k) => k.ausgleichbar),
            konten: gruppe,
            offeneKonten: gruppe.filter((k) => !k.ausgeglichen).length,
            offenerBetrag: gruppe.reduce(
                (s, k) => s + (k.ausgeglichen ? 0 : k.endsaldo),
                0
            )
        }));
    }

    $: gruppen = gruppiere(abschluss?.konten ?? []);

    const kontoHeaders = [
        { key: 'kontonummer', value: 'Konto' },
        { key: 'bezeichnung', value: 'Bezeichnung' },
        { key: 'verknuepfung', value: 'Verknüpfung' },
        { key: 'saldovortrag', value: 'Saldovortrag' },
        { key: 'sollJahr', value: 'Soll (Jahr)' },
        { key: 'habenJahr', value: 'Haben (Jahr)' },
        { key: 'endsaldo', value: 'Endsaldo' },
        { key: 'offenePosten', value: 'Offene Posten' },
        { key: 'status', value: 'Status' }
    ];

    function kontoRows(konten: WalterKontoJahres[]): DataTableRow[] {
        return konten.map((k) => ({
            id: k.kontoId,
            kontonummer: k.kontonummer,
            bezeichnung: k.bezeichnung,
            verknuepfung: k.verknuepfungText ?? '---',
            saldovortrag: convertEuro(k.saldovortrag),
            sollJahr: convertEuro(k.sollJahr),
            habenJahr: convertEuro(k.habenJahr),
            endsaldo: k.endsaldo,
            offenePosten:
                k.offenePostenAnzahl > 0
                    ? `${k.offenePostenAnzahl} (${convertEuro(k.offenePostenBetrag)})`
                    : '—',
            ausgleichbar: k.ausgleichbar,
            ausgeglichen: k.ausgeglichen
        }));
    }

    const abrechnungHeaders = [
        { key: 'bezeichnung', value: 'Vertrag' },
        { key: 'status', value: 'Status' }
    ];

    $: abrechnungRows = (abschluss?.abrechnungen ?? []).map((a) => ({
        id: a.vertragId,
        bezeichnung: a.bezeichnung,
        resultatVorhanden: a.resultatVorhanden,
        abgesendet: a.abgesendet,
        ausgeglichen: a.ausgeglichen
    }));

    const zeigeKonto = (e: CustomEvent<DataTableRow>) =>
        navigation.buchungskonto(e.detail.id as number);
    const zeigeVertrag = (e: CustomEvent<DataTableRow>) =>
        navigation.vertrag(e.detail.id as number);
</script>

<Content>
    {#if !uebersichtLoaded}
        <DataTableSkeleton showHeader={false} showToolbar={false} />
    {:else if uebersicht.length === 0}
        <Tile>Noch keine Buchungen vorhanden.</Tile>
    {:else}
        <Dropdown
            titleText="Abrechnungszeitraum"
            items={dropdownItems}
            selectedId={selectedJahr}
            on:select={(e) => (selectedJahr = e.detail.selectedId)}
            style="max-width: 20rem; margin-bottom: 1rem;"
        />

        {#if abschluss === undefined}
            <DataTableSkeleton showHeader={false} showToolbar={false} />
        {:else}
            <Tile style="margin-bottom: 1rem;">
                <h4 style="display: inline; margin-right: 1rem;">
                    Abrechnungszeitraum 01.01.{abschluss.jahr} – 31.12.{abschluss.jahr}
                </h4>
                {#if abschluss.jahrAbgeschlossen}
                    <Tag type="green">Jahr abgeschlossen</Tag>
                {:else}
                    {#if abschluss.kontenOffen > 0}
                        <Tag type="red">
                            {abschluss.kontenOffen}
                            {abschluss.kontenOffen === 1 ? 'Konto' : 'Konten'} offen
                        </Tag>
                    {/if}
                    {#if abschluss.abrechnungenFertig < abschluss.abrechnungenGesamt}
                        <Tag type="red">
                            {abschluss.abrechnungenGesamt -
                                abschluss.abrechnungenFertig}
                            {abschluss.abrechnungenGesamt -
                                abschluss.abrechnungenFertig ===
                            1
                                ? 'Abrechnung'
                                : 'Abrechnungen'} ausstehend
                        </Tag>
                    {/if}
                {/if}
            </Tile>

            <Accordion>
                <AccordionItem
                    open={abschluss.abrechnungenFertig <
                        abschluss.abrechnungenGesamt}
                >
                    <svelte:fragment slot="title">
                        Betriebskostenabrechnungen ({abschluss.abrechnungenFertig}/{abschluss.abrechnungenGesamt}
                        abgesendet)
                        {#if abschluss.abrechnungenFertig < abschluss.abrechnungenGesamt}
                            <Tag type="red" style="margin-left: 0.5rem;">
                                {abschluss.abrechnungenGesamt -
                                    abschluss.abrechnungenFertig} ausstehend
                            </Tag>
                        {:else if abschluss.abrechnungenGesamt > 0}
                            <Tag type="green" style="margin-left: 0.5rem;"
                                >fertig</Tag
                            >
                        {/if}
                    </svelte:fragment>
                    <Tile style="overflow: auto">
                        {#if abrechnungRows.length === 0}
                            <p>Keine aktiven Verträge in diesem Zeitraum.</p>
                        {:else}
                            <DataTable
                                size="short"
                                headers={abrechnungHeaders}
                                rows={abrechnungRows}
                                on:click:row={zeigeVertrag}
                                style="cursor: pointer;"
                            >
                                <svelte:fragment slot="cell" let:row let:cell>
                                    {#if cell.key === 'status'}
                                        {#if row.abgesendet}
                                            <Tag size="sm" type="green"
                                                >Abgesendet</Tag
                                            >
                                            {#if row.ausgeglichen}
                                                <Tag size="sm" type="green"
                                                    >Ausgeglichen</Tag
                                                >
                                            {:else}
                                                <Tag size="sm" type="red"
                                                    >Saldo offen</Tag
                                                >
                                            {/if}
                                        {:else if row.resultatVorhanden}
                                            <Tag size="sm" type="gray"
                                                >Erstellt</Tag
                                            >
                                        {:else}
                                            <Tag size="sm" type="red">Fehlt</Tag
                                            >
                                        {/if}
                                    {:else}
                                        {cell.value}
                                    {/if}
                                </svelte:fragment>
                            </DataTable>
                        {/if}
                    </Tile>
                </AccordionItem>

                {#each gruppen as gruppe (gruppe.funktion)}
                    <AccordionItem open={gruppe.offeneKonten > 0}>
                        <svelte:fragment slot="title">
                            {gruppe.funktion} ({gruppe.konten.length})
                            {#if gruppe.offeneKonten > 0}
                                <Tag type="red" style="margin-left: 0.5rem;">
                                    {gruppe.offeneKonten} offen ({convertEuro(
                                        gruppe.offenerBetrag
                                    )})
                                </Tag>
                            {:else if gruppe.ausgleichbar}
                                <Tag type="green" style="margin-left: 0.5rem;">
                                    ausgeglichen
                                </Tag>
                            {/if}
                        </svelte:fragment>
                        <Tile style="overflow: auto">
                            <DataTable
                                size="short"
                                headers={kontoHeaders}
                                rows={kontoRows(gruppe.konten)}
                                on:click:row={zeigeKonto}
                                style="cursor: pointer;"
                            >
                                <svelte:fragment slot="cell" let:row let:cell>
                                    {#if cell.key === 'status'}
                                        {#if !row.ausgleichbar}
                                            <Tag size="sm" type="cool-gray"
                                                >Summenkonto</Tag
                                            >
                                        {:else if row.ausgeglichen}
                                            <Tag size="sm" type="green"
                                                >Ausgeglichen</Tag
                                            >
                                        {:else}
                                            <Tag size="sm" type="red">Offen</Tag
                                            >
                                        {/if}
                                    {:else if cell.key === 'endsaldo'}
                                        <span
                                            style={row.ausgleichbar &&
                                            Math.abs(cell.value) > 0.005
                                                ? 'color: var(--cds-support-error)'
                                                : ''}
                                        >
                                            {convertEuro(cell.value)}
                                        </span>
                                    {:else}
                                        {cell.value}
                                    {/if}
                                </svelte:fragment>
                            </DataTable>
                        </Tile>
                    </AccordionItem>
                {/each}
            </Accordion>
        {/if}
    {/if}
</Content>
