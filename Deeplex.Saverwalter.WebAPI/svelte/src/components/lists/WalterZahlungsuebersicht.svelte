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
    Zahlungsübersicht: was war fällig, was kam, was ist offen. Eine Zeile je Periode
    bzw. Forderung in einer aufklappbaren WalterDataTable; aufgeklappt die Aufschlüsselung
    und die einzelnen Zahlungen als WalterItemList.
-->
<script lang="ts">
    import { formatEuro, formatEuroOderStrich } from '$walter/services/format';
    import { Add } from 'carbon-icons-svelte';
    import WalterDataTable from '../elements/WalterDataTable.svelte';
    import WalterItemList from '../elements/WalterItemList.svelte';
    import WalterRueckgaengig from '../details/WalterRueckgaengig.svelte';
    import {
        zahlungsStatus,
        type WalterListItem,
        type WalterZahlungsZeile
    } from '$walter/lib';
    import { createEventDispatcher } from 'svelte';

    export let rows: WalterZahlungsZeile[] = [];
    /** Leere Liste: dieser Text statt der Tabelle. */
    export let leerText = 'Keine Forderungen vorhanden.';
    /** „+ Zahlung“ bei Zeilen mit `erfassenFuer` und „Rückgängig machen“ je Zahlung. */
    export let bearbeitbar = false;
    export let fetchImpl: typeof fetch = fetch;
    /** Spaltenköpfe für Zeitraum und Fällig, z. B. „Rechnung“ und „Betrag“. */
    export let periodeTitel = 'Zeitraum';
    export let faelligTitel = 'Fällig';
    /** Mit Titel: Zusatzspalte aus `zusatz` der Zeilen (z. B. „Auf Mieter verteilt“). */
    export let zusatzTitel: string | undefined = undefined;
    /**
     * Mietkonto: statt „Bezahlt“ die Transaktionen hinter der Zeile (ganze Beträge), mit
     * `mitNk` dazu deren NK-Vorauszahlungs-Anteil.
     */
    export let mitTransaktionen = false;
    export let mitNk = false;
    /** Zeilen je Seite, z. B. 12 Monate im Mietkonto. */
    export let pageSize = 25;

    /** `zahlung`: „+ Zahlung“ geklickt; `geaendert`: eine Zahlung wurde rückgängig gemacht. */
    const dispatch = createEventDispatcher<{
        zahlung: WalterZahlungsZeile;
        geaendert: void;
    }>();

    // Beträge sind vorformatiert; sortiert wird in dieser Übersicht nicht.
    $: headers = [
        { key: 'periode', value: periodeTitel },
        { key: 'faellig', value: faelligTitel },
        ...(mitTransaktionen
            ? [
                  { key: 'transaktionen', value: 'Transaktion' },
                  ...(mitNk
                      ? [{ key: 'nkVorauszahlung', value: 'davon NK-VZ' }]
                      : [])
              ]
            : [{ key: 'bezahlt', value: 'Bezahlt' }]),
        { key: 'offen', value: 'Offen' },
        { key: 'status', value: 'Status' },
        ...(zusatzTitel ? [{ key: 'zusatz', value: zusatzTitel }] : []),
        ...(bearbeitbar ? [{ key: 'button', value: '' }] : [])
    ].map((h) => ({ ...h, sort: false as const }));

    $: zeilen = new Map(rows.map((z) => [z.id, z]));

    $: tableRows = rows.map((z) => ({
        id: z.id,
        periode: z.periode,
        faellig: formatEuro(z.faellig),
        bezahlt: formatEuro(z.bezahlt),
        transaktionen: formatEuroOderStrich(z.transaktionen),
        nkVorauszahlung: formatEuroOderStrich(z.nkVorauszahlung),
        offen: formatEuroOderStrich(z.offen),
        status: z.status ?? zahlungsStatus(z.faellig, z.bezahlt),
        zusatz: z.zusatz ?? '',
        button:
            bearbeitbar && z.erfassenFuer
                ? {
                      icon: Add,
                      kind: 'ghost' as const,
                      iconDescription: 'Zahlung erfassen',
                      onClick: () => dispatch('zahlung', z)
                  }
                : null
    }));

    $: nichtAufklappbar = rows
        .filter(
            (r) =>
                r.zahlungen.length === 0 &&
                r.breakdown.length === 0 &&
                !r.sections?.length
        )
        .map((r) => r.id);

    function breakdown(z: WalterZahlungsZeile): WalterListItem[] {
        return z.breakdown.map((part, i) => ({
            id: `part-${i}`,
            text: `${part.text}: fällig ${formatEuro(part.faellig)}, bezahlt ${formatEuro(part.bezahlt)}`
        }));
    }
</script>

{#if rows.length === 0}
    <p>{leerText}</p>
{:else}
    <WalterDataTable
        size="short"
        search={false}
        {pageSize}
        expandable
        nonExpandableRowIds={nichtAufklappbar}
        {headers}
        rows={tableRows}
        rowHref={(row) => zeilen.get(`${row.id}`)?.href}
    >
        <svelte:fragment slot="expanded-row" let:row>
            {@const zeile = zeilen.get(`${row.id}`)}
            {#if zeile}
                <WalterItemList items={breakdown(zeile)} />
                {#each zeile.sections ?? [] as section}
                    <h6 class="section">{section.title}</h6>
                    <WalterItemList items={section.items} />
                {/each}
                {#if zeile.sections?.length}
                    <h6 class="section">Zahlungen</h6>
                {/if}
                <WalterItemList
                    items={zeile.zahlungen}
                    leerText="Noch keine Zahlung."
                >
                    <svelte:fragment slot="aktion" let:item>
                        {#if bearbeitbar}
                            <WalterRueckgaengig
                                {fetchImpl}
                                transaktionId={item.transaktionId}
                                buchungssatzId={item.buchungssatzId}
                                on:erledigt={() => dispatch('geaendert')}
                            />
                        {/if}
                    </svelte:fragment>
                </WalterItemList>
            {/if}
        </svelte:fragment>
    </WalterDataTable>
{/if}

<style>
    .section {
        margin: 0.75rem 0 0.25rem 0;
        color: var(--cds-text-02);
    }
    .section:first-child {
        margin-top: 0.25rem;
    }
</style>
