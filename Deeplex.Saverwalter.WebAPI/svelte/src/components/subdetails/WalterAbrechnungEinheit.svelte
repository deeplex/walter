<script lang="ts">
    import { convertM2 } from '$walter/services/utils';
    import type { WalterBetriebskostenabrechnungsRechnungsgruppeEntry } from '$walter/types';
    import {
        Row,
        StructuredList,
        StructuredListBody,
        StructuredListCell,
        StructuredListHead,
        StructuredListRow,
        Tile
    } from 'carbon-components-svelte';

    export let entry: WalterBetriebskostenabrechnungsRechnungsgruppeEntry;
    export let abrechnungstage: number;
</script>

<Row>
    <Tile light>
        <h4>Abrechnungseinheit: {entry.bezeichnung}</h4>
    </Tile>
</Row>
<Row>
    <StructuredList>
        <StructuredListHead>
            <StructuredListRow head>
                <StructuredListCell head>Nutzeinheiten</StructuredListCell>
                <StructuredListCell head>Wohnfläche</StructuredListCell>
                <StructuredListCell head>Nutzfläche</StructuredListCell>
                <StructuredListCell head>Bewohner</StructuredListCell>
                <StructuredListCell head>Nutzungsintervall</StructuredListCell>
                <StructuredListCell head>Tage</StructuredListCell>
            </StructuredListRow>
        </StructuredListHead>
        <StructuredListBody>
            {#each entry.personenZeitanteil as intervall, index}
                <StructuredListRow>
                    <StructuredListCell
                        >{!index
                            ? entry.gesamtEinheiten
                            : ''}</StructuredListCell
                    >
                    <StructuredListCell
                        >{!index
                            ? convertM2(entry.gesamtWohnflaeche)
                            : ''}</StructuredListCell
                    >
                    <StructuredListCell
                        >{!index
                            ? convertM2(entry.gesamtNutzflaeche)
                            : ''}</StructuredListCell
                    >
                    <StructuredListCell
                        >{intervall.personenzahl}</StructuredListCell
                    >
                    <StructuredListCell
                        >{new Date(intervall.beginn).toLocaleDateString(
                            'de-DE'
                        )} - {new Date(intervall.ende).toLocaleDateString(
                            'de-DE'
                        )}</StructuredListCell
                    >
                    <StructuredListCell
                        >{intervall.tage} / {abrechnungstage}</StructuredListCell
                    >
                </StructuredListRow>
            {/each}
        </StructuredListBody>
    </StructuredList>
</Row>
