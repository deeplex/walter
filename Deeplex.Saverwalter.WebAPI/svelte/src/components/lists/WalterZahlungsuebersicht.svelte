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
    bzw. Forderung, aufklappbar zu den einzelnen Zahlungen.
-->
<script lang="ts">
    import { Button, DataTable, Tag } from 'carbon-components-svelte';
    import { Add } from 'carbon-icons-svelte';
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';
    import WalterRueckgaengig from '../details/WalterRueckgaengig.svelte';
    import { zahlungsStatus, type WalterZahlungsZeile } from '$walter/lib';
    import { createEventDispatcher } from 'svelte';

    export let rows: WalterZahlungsZeile[] = [];
    /** Leere Liste: dieser Text statt der Tabelle. */
    export let leerText = 'Keine Forderungen vorhanden.';
    /** „+ Zahlung“ bei Zeilen mit `erfassenFuer` und „Rückgängig machen“ je Zahlung. */
    export let bearbeitbar = false;
    export let fetchImpl: typeof fetch = fetch;

    /** `zahlung`: „+ Zahlung“ geklickt; `geaendert`: eine Zahlung wurde rückgängig gemacht. */
    const dispatch = createEventDispatcher<{
        zahlung: WalterZahlungsZeile;
        geaendert: void;
    }>();

    const eur = (v: number) =>
        v.toLocaleString('de-DE', { style: 'currency', currency: 'EUR' });
    const datumText = (d: string) => new Date(d).toLocaleDateString('de-DE');
    const betragOderStrich = (v: number) =>
        Math.abs(v) < 0.005 ? '—' : eur(v);

    $: headers = [
        { key: 'periode', value: 'Zeitraum' },
        { key: 'faellig', value: 'Fällig' },
        { key: 'bezahlt', value: 'Bezahlt' },
        { key: 'offen', value: 'Offen' },
        { key: 'status', value: 'Status' },
        ...(bearbeitbar ? [{ key: 'aktion', empty: true }] : [])
    ];

    $: tableRows = rows as unknown as DataTableRow[];
    $: zeileVon = (row: DataTableRow) => row as unknown as WalterZahlungsZeile;
    $: nichtAufklappbar = rows
        .filter(
            (r) => r.zahlungen.length === 0 && r.aufschluesselung.length === 0
        )
        .map((r) => r.id);
</script>

{#if rows.length === 0}
    <p>{leerText}</p>
{:else}
    <DataTable
        size="short"
        expandable
        nonExpandableRowIds={nichtAufklappbar}
        {headers}
        rows={tableRows}
    >
        <svelte:fragment slot="cell" let:row let:cell>
            {@const zeile = zeileVon(row)}
            {#if cell.key === 'status'}
                {@const status = zahlungsStatus(zeile.faellig, zeile.bezahlt)}
                <Tag size="sm" type={status.tag}>{status.text}</Tag>
            {:else if cell.key === 'aktion'}
                {#if zeile.erfassenFuer}
                    <Button
                        kind="ghost"
                        size="small"
                        icon={Add}
                        on:click={(e) => {
                            e.stopPropagation();
                            dispatch('zahlung', zeile);
                        }}
                    >
                        Zahlung
                    </Button>
                {/if}
            {:else if cell.key === 'faellig' || cell.key === 'bezahlt'}
                {eur(cell.value)}
            {:else if cell.key === 'offen'}
                {betragOderStrich(cell.value)}
            {:else}
                {cell.value}
            {/if}
        </svelte:fragment>
        <svelte:fragment slot="expanded-row" let:row>
            {@const zeile = zeileVon(row)}
            {#if zeile.aufschluesselung.length > 0}
                <ul class="teile">
                    {#each zeile.aufschluesselung as teil}
                        <li>
                            {teil.text}: fällig {eur(teil.faellig)}, bezahlt {eur(
                                teil.bezahlt
                            )}
                        </li>
                    {/each}
                </ul>
            {/if}
            {#if zeile.zahlungen.length > 0}
                <ul class="zahlungen">
                    {#each zeile.zahlungen as zahlung (zahlung.id)}
                        <li>
                            <span class="datum">{datumText(zahlung.datum)}</span
                            >
                            <span class="betrag">{eur(zahlung.betrag)}</span>
                            <span class="text">{zahlung.text}</span>
                            <a href={zahlung.href}>Details →</a>
                            {#if bearbeitbar}
                                <WalterRueckgaengig
                                    {fetchImpl}
                                    transaktionId={zahlung.transaktionId}
                                    buchungssatzId={zahlung.buchungssatzId}
                                    on:erledigt={() => dispatch('geaendert')}
                                />
                            {/if}
                        </li>
                    {/each}
                </ul>
            {:else}
                <p>Noch keine Zahlung.</p>
            {/if}
        </svelte:fragment>
    </DataTable>
{/if}

<style>
    ul {
        list-style: none;
        margin: 0 0 0.5rem 0;
        padding: 0;
    }
    .teile li {
        color: var(--cds-text-secondary);
    }
    .zahlungen li {
        display: flex;
        gap: 1rem;
        flex-wrap: wrap;
        padding: 0.25rem 0;
    }
    .datum {
        min-width: 6rem;
    }
    .betrag {
        min-width: 7rem;
        text-align: right;
    }
    .text {
        flex: 1;
    }
</style>
