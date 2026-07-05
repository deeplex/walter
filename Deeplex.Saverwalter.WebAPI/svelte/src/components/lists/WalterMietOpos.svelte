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

<script context="module" lang="ts">
    export type MietOposMonat = {
        jahr: number;
        monat: number;
        soll: number;
        ausgeglichen: number;
        offen: number;
    };
</script>

<script lang="ts">
    import {
        Accordion,
        AccordionItem,
        DataTable,
        Tag,
        Tile,
        Toolbar,
        ToolbarContent
    } from 'carbon-components-svelte';
    import { convertEuro } from '$walter/services/utils';

    export let rows: MietOposMonat[] = [];
    export let title = 'Mieten';

    const MONAT = [
        'Jan',
        'Feb',
        'Mär',
        'Apr',
        'Mai',
        'Jun',
        'Jul',
        'Aug',
        'Sep',
        'Okt',
        'Nov',
        'Dez'
    ];

    $: offeneGesamt = rows.reduce((s, r) => s + r.offen, 0);
    $: tableRows = rows.map((r, i) => ({
        id: String(i),
        // monat === 0 → Jahres-Summe, sonst einzelner Monat.
        periode: r.monat === 0 ? `${r.jahr}` : `${MONAT[r.monat - 1]} ${r.jahr}`,
        soll: convertEuro(r.soll),
        ausgeglichen: convertEuro(r.ausgeglichen),
        offen: r.offen,
        offenFormatted: convertEuro(r.offen)
    }));

    const headers = [
        { key: 'periode', value: 'Zeitraum' },
        { key: 'soll', value: 'Soll' },
        { key: 'ausgeglichen', value: 'Ausgeglichen' },
        { key: 'offenFormatted', value: 'Offen' }
    ];
</script>

<Accordion>
    <AccordionItem>
        <svelte:fragment slot="title">
            {title} ({rows.length})
            {#if offeneGesamt > 0.005}
                <Tag type="red" style="margin-left: 0.5rem;"
                    >{convertEuro(offeneGesamt)} offen</Tag
                >
            {:else if offeneGesamt < -0.005}
                <Tag type="blue" style="margin-left: 0.5rem;"
                    >{convertEuro(Math.abs(offeneGesamt))} Guthaben</Tag
                >
            {/if}
        </svelte:fragment>
        <Tile style="overflow: auto">
            {#if rows.length === 0}
                <p>Keine Mietbuchungen vorhanden.</p>
            {:else}
                <DataTable
                    size="short"
                    {headers}
                    rows={tableRows}
                    style="margin-bottom: 1rem;"
                >
                    <Toolbar><ToolbarContent /></Toolbar>
                    <svelte:fragment slot="cell" let:row let:cell>
                        {#if cell.key === 'offenFormatted'}
                            <span
                                style={row.offen > 0.005
                                    ? 'color: var(--cds-support-error)'
                                    : row.offen < -0.005
                                      ? 'color: var(--cds-link-primary)'
                                      : ''}
                            >
                                {cell.value}
                            </span>
                        {:else}
                            {cell.value}
                        {/if}
                    </svelte:fragment>
                </DataTable>
            {/if}
        </Tile>
    </AccordionItem>
</Accordion>
