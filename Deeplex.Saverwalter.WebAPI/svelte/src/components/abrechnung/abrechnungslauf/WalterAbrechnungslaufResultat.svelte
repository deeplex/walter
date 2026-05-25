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
    import type { AbrechnungsresultatInfo } from './AbrechnungslaufTypes';
    import { convertEuro } from '$walter/services/utils';
    import {
        Row,
        StructuredList,
        StructuredListBody,
        StructuredListCell,
        StructuredListHead,
        StructuredListRow
    } from 'carbon-components-svelte';

    export let resultat: AbrechnungsresultatInfo;
    export let kalteNk: { bezeichnung: string; betrag: number }[];
    export let warmeNk: { bezeichnung: string; betrag: number }[];

    $: result = resultat.vorauszahlung - resultat.rechnungsbetrag;
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
            {#if kalteNk.length > 0}
                <StructuredListRow>
                    <StructuredListCell head
                        >Kalte Betriebskosten</StructuredListCell
                    >
                    <StructuredListCell head style="text-align:right"
                    ></StructuredListCell>
                </StructuredListRow>
                {#each kalteNk as e}
                    <StructuredListRow>
                        <StructuredListCell style="padding-left: 1.5rem"
                            >{e.bezeichnung}:</StructuredListCell
                        >
                        <StructuredListCell style="text-align:right"
                            >{convertEuro(e.betrag)}</StructuredListCell
                        >
                    </StructuredListRow>
                {/each}
            {/if}
            {#if warmeNk.length > 0}
                <StructuredListRow>
                    <StructuredListCell head
                        >Warme Betriebskosten (HKVO)</StructuredListCell
                    >
                    <StructuredListCell head style="text-align:right"
                    ></StructuredListCell>
                </StructuredListRow>
                {#each warmeNk as e}
                    <StructuredListRow>
                        <StructuredListCell style="padding-left: 1.5rem"
                            >{e.bezeichnung}:</StructuredListCell
                        >
                        <StructuredListCell style="text-align:right">
                            {convertEuro(e.betrag)}
                        </StructuredListCell>
                    </StructuredListRow>
                {/each}
            {/if}
            <StructuredListRow>
                <StructuredListCell head>Nebenkosten gesamt:</StructuredListCell
                >
                <StructuredListCell head style="text-align:right">
                    {convertEuro(resultat.rechnungsbetrag)}
                </StructuredListCell>
            </StructuredListRow>
            <StructuredListRow>
                <StructuredListCell>Vorauszahlungen:</StructuredListCell>
                <StructuredListCell style="text-align:right">
                    {convertEuro(resultat.vorauszahlung)}
                </StructuredListCell>
            </StructuredListRow>
            <StructuredListRow>
                <StructuredListCell head
                    >Ergebnis der Abrechnung:</StructuredListCell
                >
                <StructuredListCell head style="text-align:right">
                    {convertEuro(result)}
                </StructuredListCell>
            </StructuredListRow>
        </StructuredListBody>
    </StructuredList>
</Row>
