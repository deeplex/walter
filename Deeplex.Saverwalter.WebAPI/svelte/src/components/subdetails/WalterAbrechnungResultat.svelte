<script lang="ts">
    import { convertEuro } from '$WalterServices/utils';
    import type { WalterBetriebskostenabrechnungEntry } from '$WalterTypes';
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
    <StructuredList style="margin-right: 2em">
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
                {#if abrechnungseinheit.betragKalteNebenkosten}
                    <StructuredListRow>
                        <StructuredListCell
                            >Abrechnungseinheit: {abrechnungseinheit.bezeichnung}
                            (kalte Nebenkosten) :</StructuredListCell
                        >
                        <StructuredListCell style="text-align: right"
                            >{convertEuro(
                                abrechnungseinheit.betragKalteNebenkosten || 0
                            )}</StructuredListCell
                        >
                    </StructuredListRow>
                {/if}
                {#if abrechnungseinheit.betragWarmeNebenkosten}
                    <StructuredListRow>
                        <StructuredListCell
                            >Abrechnungseinheit: {abrechnungseinheit.bezeichnung}
                            (warme Nebenkosten) :</StructuredListCell
                        >
                        <StructuredListCell style="text-align: right"
                            >{convertEuro(
                                abrechnungseinheit.betragWarmeNebenkosten
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
                <StructuredListCell>Gezahlt :</StructuredListCell>
                <StructuredListCell style="text-align: right"
                    >{convertEuro(entry.gezahlt)}</StructuredListCell
                >
            </StructuredListRow>
            <StructuredListRow>
                <StructuredListCell head
                    >Ergebnis der Abrechnung :</StructuredListCell
                >
                <StructuredListCell head style="text-align: right">
                    {convertEuro(entry.result)}
                </StructuredListCell>
            </StructuredListRow>
        </StructuredListBody>
    </StructuredList>
</Row>
