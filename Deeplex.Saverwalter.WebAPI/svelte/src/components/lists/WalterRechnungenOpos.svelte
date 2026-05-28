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
    import { Accordion, AccordionItem, DataTable, Tag, Tile, Toolbar, ToolbarContent } from 'carbon-components-svelte';
    import { convertEuro } from '$walter/services/utils';
    import type { WalterBetriebskostenrechnungEntry } from '$walter/lib';

    export let rows: WalterBetriebskostenrechnungEntry[] = [];
    export let title = 'Rechnungen OPOS';

    $: unbezahltAnzahl = rows.filter((r) => !r.isBezahlt).length;
    $: offenVerteilt = rows.reduce((s, r) => s + Math.max(0, r.betrag - r.verteilt), 0);

    $: tableRows = rows.map((r, i) => ({
        id: String(i),
        jahr: r.betreffendesJahr,
        datum: r.datum ? new Date(r.datum).toLocaleDateString('de-DE') : '–',
        betrag: convertEuro(r.betrag),
        bezahlt: r.isBezahlt,
        verteilt: convertEuro(r.verteilt),
        offen: Math.max(0, r.betrag - r.verteilt),
        offenFormatted: convertEuro(Math.max(0, r.betrag - r.verteilt))
    })).sort((a, b) => b.jahr - a.jahr);

    const headers = [
        { key: 'jahr', value: 'Jahr' },
        { key: 'datum', value: 'Datum' },
        { key: 'betrag', value: 'Rechnung' },
        { key: 'bezahlt', value: 'Bezahlt' },
        { key: 'verteilt', value: 'Verteilt (NK)' },
        { key: 'offenFormatted', value: 'NK Offen' }
    ];
</script>

<Accordion>
    <AccordionItem>
        <svelte:fragment slot="title">
            {title} ({rows.length})
            {#if unbezahltAnzahl > 0}
                <Tag type="red" style="margin-left: 0.5rem;">{unbezahltAnzahl} unbezahlt</Tag>
            {/if}
            {#if offenVerteilt > 0.005}
                <Tag type="warm-gray" style="margin-left: 0.25rem;">{convertEuro(offenVerteilt)} NK offen</Tag>
            {/if}
        </svelte:fragment>
        <Tile style="overflow: auto">
            {#if rows.length === 0}
                <p>Keine Rechnungen vorhanden.</p>
            {:else}
                <DataTable
                    size="short"
                    {headers}
                    rows={tableRows}
                    style="margin-bottom: 1rem;"
                >
                    <Toolbar><ToolbarContent /></Toolbar>
                    <svelte:fragment slot="cell" let:row let:cell>
                        {#if cell.key === 'bezahlt'}
                            {#if row.bezahlt}
                                <Tag type="green" size="sm">Bezahlt</Tag>
                            {:else}
                                <Tag type="red" size="sm">Offen</Tag>
                            {/if}
                        {:else if cell.key === 'offenFormatted'}
                            <span style={row.offen > 0.005 ? 'color: var(--cds-support-error)' : ''}>
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
