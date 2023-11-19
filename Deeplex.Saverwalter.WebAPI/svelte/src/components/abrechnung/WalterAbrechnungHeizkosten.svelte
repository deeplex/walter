<script lang="ts">
    import {
        convertEuro,
        convertFixed2,
        convertPercent
    } from '$walter/services/utils';
    import './katex.min.css';
    import type { WalterHeizkostenberechnungEntry } from '$walter/types/WalterBetriebskostenabrechnung.type';
    import {
        Row,
        StructuredList,
        StructuredListBody,
        StructuredListCell,
        StructuredListHead,
        StructuredListRow,
        Tile,
        Truncate
    } from 'carbon-components-svelte';
    import Katex from 'svelte-katex';

    export let heizkosten: WalterHeizkostenberechnungEntry;
    const betrag = heizkosten.gesamtBetrag + heizkosten.pauschalBetrag;
</script>

<Row>
    <StructuredList condensed style="margin: 2em; width: 50%">
        <StructuredListHead>
            <StructuredListRow head>
                <StructuredListCell head>Heizkosten</StructuredListCell>
                <StructuredListCell head style="text-align:right">
                    Betrag
                </StructuredListCell>
            </StructuredListRow>
        </StructuredListHead>
        <StructuredListBody>
            <StructuredListRow>
                <StructuredListCell>Kosten für Brennstoffe</StructuredListCell>
                <StructuredListCell style="text-align: right">
                    {convertEuro(heizkosten.gesamtBetrag)}
                </StructuredListCell>
            </StructuredListRow>
            <StructuredListRow>
                <StructuredListCell
                    >Betriebskosten der Anlage (5% pauschal)</StructuredListCell
                >
                <StructuredListCell style="text-align: right">
                    {convertEuro(heizkosten.gesamtBetrag * 0.05)}
                </StructuredListCell>
            </StructuredListRow>
            <StructuredListRow>
                <StructuredListCell head>Gesamt</StructuredListCell>
                <StructuredListCell head style="text-align: right">
                    {convertEuro(heizkosten.pauschalBetrag)}
                </StructuredListCell>
            </StructuredListRow>
        </StructuredListBody>
    </StructuredList>
</Row>

<Row style="margin-top: -4em">
    <Tile>
        <Truncate>Davon der Warmwasseranteil nach HeizkostenV §9(2):</Truncate>
    </Tile>
</Row>
<Tile>
    <Katex displayMode>
        2,5 \times \frac{'{V}'}{'{Q}'} \times (60°C-10°C) \frac{'{kWh}'}{'{m³ K}'}\rightarrow
        2,5 \times \frac{`{${convertFixed2(heizkosten.v)} m³}`}{`{${
            heizkosten.q / 1000
        } kWh}`} \times (60°C - 10°C) \frac{'{kWh}'}{'{m³ K}'} = {(
            heizkosten.para9_2 * 100
        ).toFixed(2)} \%
    </Katex>
</Tile>

<!-- TODO: Einheitenermittlung -->

<Row>
    <StructuredList condensed style="margin: 2em;">
        <StructuredListHead>
            <StructuredListRow head>
                <StructuredListCell head>Kostenanteil</StructuredListCell>
                <StructuredListCell head>Schlüssel</StructuredListCell>
                <StructuredListCell head>Betrag</StructuredListCell>
                <StructuredListCell head>Auft.§9(2)</StructuredListCell>
                <StructuredListCell head>Auft.§7,8</StructuredListCell>
                <StructuredListCell head>Ihr Anteil</StructuredListCell>
                <StructuredListCell head>Ihre Kosten</StructuredListCell>
            </StructuredListRow>
        </StructuredListHead>
        <StructuredListBody>
            <StructuredListRow>
                <StructuredListCell>Heizung</StructuredListCell>
                <StructuredListCell>n. NF</StructuredListCell>
                <StructuredListCell
                    >{convertEuro(
                        heizkosten.pauschalBetrag
                    )}</StructuredListCell
                >
                <StructuredListCell
                    >{convertPercent(
                        1 - heizkosten.para9_2
                    )}</StructuredListCell
                >
                <StructuredListCell
                    >{convertPercent(heizkosten.para7)}</StructuredListCell
                >
                <StructuredListCell
                    >{convertPercent(
                        heizkosten.nfZeitanteil
                    )}</StructuredListCell
                >
                <StructuredListCell
                    >{convertEuro(
                        heizkosten.waermeAnteilNF
                    )}</StructuredListCell
                >
            </StructuredListRow>
            <StructuredListRow>
                <StructuredListCell>Heizung</StructuredListCell>
                <StructuredListCell>n. Verb.</StructuredListCell>
                <StructuredListCell
                    >{convertEuro(
                        heizkosten.pauschalBetrag
                    )}</StructuredListCell
                >
                <StructuredListCell
                    >{convertPercent(
                        1 - heizkosten.para9_2
                    )}</StructuredListCell
                >
                <StructuredListCell
                    >{convertPercent(heizkosten.para8)}</StructuredListCell
                >
                <StructuredListCell
                    >{convertPercent(
                        heizkosten.heizkostenVerbrauchAnteil
                    )}</StructuredListCell
                >
                <StructuredListCell
                    >{convertEuro(
                        heizkosten.waermeAnteilVerb
                    )}</StructuredListCell
                >
            </StructuredListRow>
            <StructuredListRow>
                <StructuredListCell>Warmwasser</StructuredListCell>
                <StructuredListCell>n. NF</StructuredListCell>
                <StructuredListCell
                    >{convertEuro(
                        heizkosten.pauschalBetrag
                    )}</StructuredListCell
                >
                <StructuredListCell
                    >{convertPercent(heizkosten.para9_2)}</StructuredListCell
                >
                <StructuredListCell
                    >{convertPercent(1 - heizkosten.para7)}</StructuredListCell
                >
                <StructuredListCell
                    >{convertPercent(
                        heizkosten.nfZeitanteil
                    )}</StructuredListCell
                >
                <StructuredListCell
                    >{convertEuro(
                        heizkosten.warmwasserAnteilNF
                    )}</StructuredListCell
                >
            </StructuredListRow>
            <StructuredListRow>
                <StructuredListCell>Warmwasser</StructuredListCell>
                <StructuredListCell>n. Verb.</StructuredListCell>
                <StructuredListCell
                    >{convertEuro(
                        heizkosten.pauschalBetrag
                    )}</StructuredListCell
                >
                <StructuredListCell
                    >{convertPercent(heizkosten.para9_2)}</StructuredListCell
                >
                <StructuredListCell
                    >{convertPercent(1 - heizkosten.para8)}</StructuredListCell
                >
                <StructuredListCell
                    >{convertPercent(
                        heizkosten.warmwasserVerbrauchAnteil
                    )}</StructuredListCell
                >
                <StructuredListCell
                    >{convertEuro(
                        heizkosten.warmwasserAnteilVerb
                    )}</StructuredListCell
                >
            </StructuredListRow>
            <StructuredListRow>
                <StructuredListCell></StructuredListCell>
                <StructuredListCell></StructuredListCell>
                <StructuredListCell></StructuredListCell>
                <StructuredListCell></StructuredListCell>
                <StructuredListCell></StructuredListCell>
                <StructuredListCell head>Summe:</StructuredListCell>
                <StructuredListCell head
                    >{convertEuro(heizkosten.betrag)}</StructuredListCell
                >
            </StructuredListRow>
        </StructuredListBody>
    </StructuredList>
</Row>
