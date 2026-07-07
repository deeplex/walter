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
        Button,
        Content,
        DataTable,
        DataTableSkeleton,
        Dropdown,
        InlineLoading,
        Modal,
        Tag,
        TextArea,
        Tile
    } from 'carbon-components-svelte';
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
    import { onMount } from 'svelte';
    import {
        getJahresUebersicht,
        getJahresabschluss,
        getJahresabschlussKontrolle,
        setzeAbrechnungsverzicht,
        hebeAbrechnungsverzichtAuf,
        type WalterJahresUebersicht,
        type WalterJahresabschluss,
        type WalterKontoJahres,
        type WalterJahresabschlussKontrolle,
        type WalterPruefStatus,
        WalterToastContent
    } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';
    import { convertEuro } from '$walter/services/utils';
    import { addToast } from '$walter/store';

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
        kontrolle = undefined; // Kontroll-Ergebnis gehört zum alten Jahr → verwerfen.
        getJahresabschluss(selectedJahr, fetchImpl).then((a) => {
            // Antwort verwerfen, wenn inzwischen ein anderes Jahr gewählt wurde.
            if (a.jahr === selectedJahr) abschluss = a;
        });
    }

    // Konsistenz-Kontrolle: rechnet jede Gruppe des Jahres einmal nach und vergleicht
    // mit dem Gebuchten. On-demand, weil das teuer sein kann.
    let kontrolle: WalterJahresabschlussKontrolle | undefined;
    let kontrolleLaeuft = false;

    const KontrolleToast = new WalterToastContent(
        undefined,
        'Konsistenz-Check fehlgeschlagen',
        () => '',
        (error: unknown) =>
            typeof error === 'string' && error.length > 0
                ? error
                : 'Bitte erneut versuchen.'
    );

    async function pruefeKonsistenz() {
        if (selectedJahr === undefined) return;
        const jahr = selectedJahr;
        kontrolleLaeuft = true;
        kontrolle = undefined;
        try {
            const result = await getJahresabschlussKontrolle(jahr, fetchImpl);
            if (jahr === selectedJahr) kontrolle = result;
        } catch (e) {
            addToast(KontrolleToast, false, String(e instanceof Error ? e.message : e));
        } finally {
            kontrolleLaeuft = false;
        }
    }

    const statusInfo: Record<
        WalterPruefStatus,
        { label: string; type: 'green' | 'red' | 'gray' | 'teal' }
    > = {
        Bestanden: { label: 'Bestanden', type: 'green' },
        NichtBestanden: { label: 'Abweichend', type: 'red' },
        Fehlt: { label: 'Fehlt', type: 'gray' },
        Verzichtet: { label: 'Verzicht ok', type: 'teal' }
    };

    // Nur die auffälligen Positionen listen (bestanden/verzicht-ok braucht keiner).
    $: probleme = (kontrolle?.positionen ?? []).filter(
        (p) => p.status !== 'Bestanden' && p.status !== 'Verzichtet'
    );

    // Nach einer Verzicht-Änderung Jahr + Übersicht neu laden.
    async function reload() {
        if (selectedJahr === undefined) return;
        const jahr = selectedJahr;
        const [a, u] = await Promise.all([
            getJahresabschluss(jahr, fetchImpl),
            getJahresUebersicht(fetchImpl)
        ]);
        if (a.jahr === selectedJahr) abschluss = a;
        uebersicht = u;
    }

    // Verzicht-Dialog
    let verzichtModalOpen = false;
    let verzichtVertragId: number | undefined;
    let verzichtBezeichnung = '';
    let verzichtGrund = '';

    function oeffneVerzichtDialog(vertragId: number, bezeichnung: string) {
        verzichtVertragId = vertragId;
        verzichtBezeichnung = bezeichnung;
        verzichtGrund = '';
        verzichtModalOpen = true;
    }

    async function bestaetigeVerzicht() {
        if (
            verzichtVertragId === undefined ||
            selectedJahr === undefined ||
            !verzichtGrund.trim()
        )
            return;
        await setzeAbrechnungsverzicht(
            verzichtVertragId,
            selectedJahr,
            verzichtGrund.trim()
        );
        verzichtModalOpen = false;
        await reload();
    }

    async function verzichtAufheben(vertragId: number) {
        if (selectedJahr === undefined) return;
        await hebeAbrechnungsverzichtAuf(vertragId, selectedJahr);
        await reload();
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
            ausgeglichen: k.ausgeglichen,
            verzichtet: k.verzichtet
        }));
    }

    const abrechnungHeaders = [
        { key: 'bezeichnung', value: 'Vertrag' },
        { key: 'status', value: 'Status' },
        { key: 'aktion', value: '' }
    ];

    $: abrechnungRows = (abschluss?.abrechnungen ?? []).map((a) => ({
        id: a.vertragId,
        bezeichnung: a.bezeichnung,
        resultatVorhanden: a.resultatVorhanden,
        abgesendet: a.abgesendet,
        ausgeglichen: a.ausgeglichen,
        verzichtet: a.verzichtet,
        verzichtGrund: a.verzichtGrund
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

            <Tile style="margin-bottom: 1rem;">
                <div
                    style="display: flex; align-items: center; gap: 1rem; flex-wrap: wrap;"
                >
                    <Button
                        size="small"
                        kind="tertiary"
                        disabled={kontrolleLaeuft}
                        on:click={pruefeKonsistenz}
                    >
                        Konsistenz prüfen
                    </Button>
                    {#if kontrolleLaeuft}
                        <InlineLoading
                            description="Rechne alle Abrechnungsgruppen nach …"
                        />
                   {/if}
                </div>

                {#if kontrolle && !kontrolleLaeuft}
                    {@const k = kontrolle}
                    <p
                        style="font-size: 0.75rem; color: var(--cds-text-secondary); margin: 1rem 0 0.5rem;"
                    >
                        {k.gesamt} Positionen geprüft
                    </p>
                    <div
                        style="display: flex; gap: 0.75rem; flex-wrap: wrap;"
                    >
                        {#each [{ label: 'Bestanden', wert: k.bestanden, type: 'green' }, { label: 'Fehlt', wert: k.fehlt, type: 'gray' }, { label: 'Verzicht ok', wert: k.verzichtet, type: 'teal' }, { label: 'Abweichend', wert: k.nichtBestanden, type: 'red' }] as kachel}
                            <div
                                style="min-width: 7rem; padding: 0.5rem 0.75rem; border-left: 4px solid {kachel.type ===
                                'green'
                                    ? 'var(--cds-support-success)'
                                    : kachel.type === 'red'
                                      ? 'var(--cds-support-error)'
                                      : kachel.type === 'teal'
                                        ? '#009d9a'
                                        : 'var(--cds-border-subtle)'}; background: var(--cds-layer);"
                            >
                                <div
                                    style="font-size: 1.5rem; font-weight: 600; line-height: 1;"
                                >
                                    {kachel.wert}
                                </div>
                                <div
                                    style="font-size: 0.75rem; color: var(--cds-text-secondary);"
                                >
                                    {kachel.label}
                                </div>
                            </div>
                        {/each}
                    </div>

                    {#if probleme.length === 0}
                        <p
                            style="margin-top: 1rem; color: var(--cds-support-success);"
                        >
                            Alles konsistent — eine erneute Abrechnung würde nichts
                            ändern.
                        </p>
                    {:else}
                        <div style="margin-top: 1rem;">
                            {#each probleme as p}
                                <div
                                    role="button"
                                    tabindex="0"
                                    on:click={() =>
                                        p.vertragId != null &&
                                        navigation.vertrag(p.vertragId)}
                                    on:keydown={(e) =>
                                        e.key === 'Enter' &&
                                        p.vertragId != null &&
                                        navigation.vertrag(p.vertragId)}
                                    style="display: flex; align-items: center; gap: 0.75rem; padding: 0.5rem 0; border-top: 1px solid var(--cds-border-subtle); {p.vertragId !=
                                    null
                                        ? 'cursor: pointer;'
                                        : ''}"
                                >
                                    <Tag size="sm" type={statusInfo[p.status].type}
                                        >{statusInfo[p.status].label}</Tag
                                    >
                                    <div style="flex: 1 1 0; min-width: 0;">
                                        <div style="font-size: 0.875rem;">
                                            {p.bezeichnung}
                                        </div>
                                        <div
                                            style="font-size: 0.75rem; color: var(--cds-text-secondary);"
                                        >
                                            {p.gruppe}{p.detail
                                                ? ` · ${p.detail}`
                                                : ''}
                                        </div>
                                    </div>
                                </div>
                            {/each}
                        </div>
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
                        erledigt)
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
                                        {#if row.verzichtet}
                                            <Tag
                                                size="sm"
                                                type="teal"
                                                title={row.verzichtGrund ?? ''}
                                                >Nicht erforderlich</Tag
                                            >
                                        {:else if row.abgesendet}
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
                                    {:else if cell.key === 'aktion'}
                                        {#if row.verzichtet}
                                            <Button
                                                size="small"
                                                kind="ghost"
                                                on:click={(e) => {
                                                    e.stopPropagation();
                                                    verzichtAufheben(
                                                        Number(row.id)
                                                    );
                                                }}>Verzicht aufheben</Button
                                            >
                                        {:else if !row.abgesendet}
                                            <Button
                                                size="small"
                                                kind="ghost"
                                                on:click={(e) => {
                                                    e.stopPropagation();
                                                    oeffneVerzichtDialog(
                                                        Number(row.id),
                                                        String(row.bezeichnung)
                                                    );
                                                }}
                                                >Keine Abrechnung nötig</Button
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
                                        {:else if row.verzichtet}
                                            <Tag size="sm" type="teal"
                                                >Verzichtet</Tag
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

<Modal
    bind:open={verzichtModalOpen}
    modalHeading="Keine Abrechnung erforderlich"
    primaryButtonText="Verzicht festhalten"
    secondaryButtonText="Abbrechen"
    primaryButtonDisabled={!verzichtGrund.trim()}
    on:click:button--secondary={() => (verzichtModalOpen = false)}
    on:submit={bestaetigeVerzicht}
>
    <p style="margin-bottom: 1rem;">
        Für <strong>{verzichtBezeichnung}</strong> wird für das Jahr
        {selectedJahr} bewusst keine Betriebskostenabrechnung erstellt. Es wird
        nichts gebucht — der Verzicht wird nur dokumentiert.
    </p>
    <TextArea
        labelText="Grund (Pflicht)"
        placeholder="z.B. Bestandsübernahme, Zeitraum vor Programmeinführung, kein Vorschuss vereinnahmt …"
        bind:value={verzichtGrund}
    />
</Modal>
