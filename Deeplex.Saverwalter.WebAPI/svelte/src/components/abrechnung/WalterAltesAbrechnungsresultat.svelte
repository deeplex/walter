<!-- Copyright (C) 2023-2025  Kai Lawrence -->
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
    import type { WalterBetriebskostenabrechnungResultatEntry } from '$walter/types';
    import {
        Row,
        StructuredList,
        StructuredListBody,
        StructuredListCell,
        StructuredListHead,
        StructuredListRow,
        Tile
    } from 'carbon-components-svelte';

    export let resultat:
        | WalterBetriebskostenabrechnungResultatEntry
        | undefined;
</script>

{#if resultat}
    <Tile style="margin-bottom: -2em">
        <h4>Letzter Stand dieser Abrechnung:</h4>
    </Tile>
    <Row>
        <StructuredList style="margin: 2em" condensed>
            <StructuredListHead>
                <StructuredListRow>
                    <StructuredListCell head
                        >Vorauszahlung Gesamt</StructuredListCell
                    >
                    <StructuredListCell head>Kaltmiete</StructuredListCell>
                    <StructuredListCell head
                        >Vorauszahlung Nebenkosten</StructuredListCell
                    >
                    <StructuredListCell head>Nebenkosten</StructuredListCell>
                    <StructuredListCell head>Saldo</StructuredListCell>
                    <StructuredListCell head>Ist versendet</StructuredListCell>
                    <StructuredListCell head>Ist beglichen</StructuredListCell>
                </StructuredListRow>
            </StructuredListHead>
            <StructuredListBody>
                <StructuredListRow>
                    <StructuredListCell>
                        {convertEuro(resultat.vorauszahlung)}
                    </StructuredListCell>
                    <StructuredListCell>
                        {convertEuro(resultat.kaltmiete)}
                    </StructuredListCell>
                    <StructuredListCell>
                        {convertEuro(
                            resultat.vorauszahlung - resultat.kaltmiete
                        )}
                    </StructuredListCell>
                    <StructuredListCell>
                        {convertEuro(resultat.rechnungsbetrag)}
                    </StructuredListCell>
                    <StructuredListCell>
                        {convertEuro(
                            resultat.vorauszahlung -
                                resultat.kaltmiete -
                                resultat.rechnungsbetrag
                        )}
                    </StructuredListCell>
                    <StructuredListCell>
                        {resultat.abgesendet ? 'Ja' : 'Nein'}
                    </StructuredListCell>
                    <StructuredListCell>
                        {resultat.istBeglichen ? 'Ja' : 'Nein'}
                    </StructuredListCell>
                </StructuredListRow>
            </StructuredListBody>
        </StructuredList>
    </Row>
{:else}
    <Tile>Bisher keine Abrechnung erstellt</Tile>
{/if}
