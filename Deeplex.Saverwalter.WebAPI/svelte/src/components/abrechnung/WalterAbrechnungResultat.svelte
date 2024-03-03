<!-- Copyright (C) 2023-2024  Kai Lawrence -->
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
    import { convertEuro } from '$walter/services/utils';
    import type { WalterBetriebskostenabrechnungEntry } from '$walter/types';
    import {
        Row,
        StructuredList,
        StructuredListBody,
        StructuredListCell,
        StructuredListHead,
        StructuredListRow
    } from 'carbon-components-svelte';

    export let entry: WalterBetriebskostenabrechnungEntry;
</script>

<Row>
    <StructuredList condensed style="margin: 2em; width: 75%">
        <StructuredListHead>
            <StructuredListRow head>
                <StructuredListCell head>Teil</StructuredListCell>
                <StructuredListCell head style="text-align:right"
                    >Betrag</StructuredListCell
                >
            </StructuredListRow>
        </StructuredListHead>
        <StructuredListBody>
            {#each entry.abrechnungseinheiten as abrechnungseinheit}
                {#if abrechnungseinheit.betragKalt}
                    <StructuredListRow>
                        <StructuredListCell
                            >Abrechnungseinheit: {abrechnungseinheit.bezeichnung}
                            (kalte Nebenkosten):</StructuredListCell
                        >
                        <StructuredListCell style="text-align: right"
                            >{convertEuro(
                                abrechnungseinheit.betragKalt || 0
                            )}</StructuredListCell
                        >
                    </StructuredListRow>
                {/if}
                {#if abrechnungseinheit.betragWarm}
                    <StructuredListRow>
                        <StructuredListCell
                            >Abrechnungseinheit: {abrechnungseinheit.bezeichnung}
                            (warme Nebenkosten):</StructuredListCell
                        >
                        <StructuredListCell style="text-align: right"
                            >{convertEuro(
                                abrechnungseinheit.betragWarm
                            )}</StructuredListCell
                        >
                    </StructuredListRow>
                {/if}
            {/each}
            <StructuredListRow>
                <StructuredListCell>Kaltmiete:</StructuredListCell>
                <StructuredListCell style="text-align: right"
                    >{convertEuro(entry.kaltMiete)}</StructuredListCell
                >
            </StructuredListRow>
            <StructuredListRow>
                <StructuredListCell>Gezahlt:</StructuredListCell>
                <StructuredListCell style="text-align: right"
                    >{convertEuro(entry.gezahltMiete)}</StructuredListCell
                >
            </StructuredListRow>
            <StructuredListRow>
                <StructuredListCell head
                    >Ergebnis der Abrechnung:</StructuredListCell
                >
                <StructuredListCell head style="text-align: right">
                    {convertEuro(entry.result)}
                </StructuredListCell>
            </StructuredListRow>
        </StructuredListBody>
    </StructuredList>
</Row>
