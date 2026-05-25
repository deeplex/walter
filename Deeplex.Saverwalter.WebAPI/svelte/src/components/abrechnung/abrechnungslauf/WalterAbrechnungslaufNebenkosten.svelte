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
    import type { NkZeileInfo } from './AbrechnungslaufTypes';
    import {
        Accordion,
        AccordionItem,
        StructuredList,
        StructuredListBody,
        StructuredListCell,
        StructuredListHead,
        StructuredListRow
    } from 'carbon-components-svelte';

    export let zeilen: NkZeileInfo[];

    const nWF = zeilen.some((z) => z.schluessel === 'n. WF');
    const nNF = zeilen.some((z) => z.schluessel === 'n. NF');
    const nNE = zeilen.some((z) => z.schluessel === 'n. NE');
    const nMEA = zeilen.some((z) => z.schluessel === 'n. MEA');
    const nPs = zeilen.some((z) => z.schluessel === 'n. Pers.');
    const nVb = zeilen.some((z) => z.schluessel === 'n. Verb.');
</script>

<Accordion>
    <AccordionItem
        title="Genutzte Umlageschlüssel in dieser Abrechnung"
        style="border-top: 0px"
    >
        <StructuredList condensed>
            <StructuredListHead>
                <StructuredListRow>
                    <StructuredListCell head>Umlageschlüssel</StructuredListCell
                    >
                    <StructuredListCell head>Bedeutung</StructuredListCell>
                </StructuredListRow>
            </StructuredListHead>
            <StructuredListBody>
                {#if nWF}
                    <StructuredListRow>
                        <StructuredListCell>n. WF</StructuredListCell>
                        <StructuredListCell
                            >nach Wohnfläche in m²</StructuredListCell
                        >
                    </StructuredListRow>
                {/if}
                {#if nNF}
                    <StructuredListRow>
                        <StructuredListCell>n. NF</StructuredListCell>
                        <StructuredListCell
                            >nach Nutzfläche in m²</StructuredListCell
                        >
                    </StructuredListRow>
                {/if}
                {#if nMEA}
                    <StructuredListRow>
                        <StructuredListCell>n. MEA</StructuredListCell>
                        <StructuredListCell
                            >nach Miteigentumsanteilen</StructuredListCell
                        >
                    </StructuredListRow>
                {/if}
                {#if nNE}
                    <StructuredListRow>
                        <StructuredListCell>n. NE</StructuredListCell>
                        <StructuredListCell
                            >nach Anzahl der Wohn-/Nutzeinheiten</StructuredListCell
                        >
                    </StructuredListRow>
                {/if}
                {#if nPs}
                    <StructuredListRow>
                        <StructuredListCell>n. Pers.</StructuredListCell>
                        <StructuredListCell
                            >nach Personenzahl/Anzahl der Bewohner</StructuredListCell
                        >
                    </StructuredListRow>
                {/if}
                {#if nVb}
                    <StructuredListRow>
                        <StructuredListCell>n. Verb.</StructuredListCell>
                        <StructuredListCell>nach Verbrauch</StructuredListCell>
                    </StructuredListRow>
                {/if}
            </StructuredListBody>
        </StructuredList>
    </AccordionItem>
    <AccordionItem title="Erläuterungen zu einzelnen Betriebskostenarten">
        <StructuredList condensed>
            <StructuredListHead>
                <StructuredListRow>
                    <StructuredListCell head
                        >Betriebskostentyp</StructuredListCell
                    >
                    <StructuredListCell head>Beschreibung</StructuredListCell>
                </StructuredListRow>
            </StructuredListHead>
            <StructuredListBody>
                {#each zeilen as z}
                    <StructuredListRow>
                        <StructuredListCell>
                            <a href="/umlagen/{z.umlageId}">{z.bezeichnung}</a>
                        </StructuredListCell>
                        <StructuredListCell>{z.beschreibung}</StructuredListCell
                        >
                    </StructuredListRow>
                {/each}
            </StructuredListBody>
        </StructuredList>
    </AccordionItem>
</Accordion>

<br />
