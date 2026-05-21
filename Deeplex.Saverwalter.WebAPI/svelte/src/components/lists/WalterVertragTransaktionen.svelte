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
    import { Accordion, AccordionItem, Tile } from 'carbon-components-svelte';
    import { WalterDataTable, WalterTransaktion } from '$walter/components';
    import type { WalterTransaktionEntry, WalterVertragEntry, TransaktionsInput } from '$walter/lib';
    import { emptyTransaktionsInput } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    export let rows: WalterTransaktionEntry[];
    export let fetchImpl: typeof fetch;
    export let title: string | undefined = undefined;
    export let vertrag: WalterVertragEntry | undefined = undefined;

    const headers = [
        { key: 'zahlungsdatum', value: 'Datum' },
        { key: 'betrag', value: 'Betrag (€)' },
        { key: 'verwendungszweck', value: 'Verwendungszweck' }
    ];

    const on_click_row = (e: CustomEvent<DataTableRow>) =>
        navigation.transaktion(e.detail.id);
    const rowHref = (row: DataTableRow) => `/transaktionen/${row.id}`;

    $: buchungsInput = {
        ...emptyTransaktionsInput(),
        mieten: [
            {
                kaltmiete: 0,
                nkVorauszahlung: 0,
                vertragId: vertrag?.id as number | undefined
            }
        ]
    } as TransaktionsInput;

    $: sortedRows = [...(rows || [])].sort((a, b) => {
        const dateA = new Date(a.zahlungsdatum ?? '').getTime();
        const dateB = new Date(b.zahlungsdatum ?? '').getTime();
        return dateB - dateA;
    });
</script>

{#if title !== undefined}
    <Accordion>
        <AccordionItem title={`${title} (${(rows || []).length})`}>
            <Tile style="overflow: auto">
                <WalterDataTable
                    addUrl="/api/transaktionen/buchen"
                    bind:addEntry={buchungsInput}
                    rows={sortedRows}
                    {on_click_row}
                    {rowHref}
                    {headers}
                >
                    <WalterTransaktion
                        {fetchImpl}
                        bind:buchung={buchungsInput}
                    />
                </WalterDataTable>
            </Tile>
        </AccordionItem>
    </Accordion>
{:else}
    <WalterDataTable
        addUrl="/api/transaktionen/buchen"
        bind:addEntry={buchungsInput}
        rows={sortedRows}
        {on_click_row}
        {rowHref}
        {headers}
    >
        <WalterTransaktion {fetchImpl} bind:buchung={buchungsInput} />
    </WalterDataTable>
{/if}
