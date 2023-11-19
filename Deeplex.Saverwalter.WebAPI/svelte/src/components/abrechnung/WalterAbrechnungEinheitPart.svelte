<script lang="ts">
    import {
        convertDateGerman,
        convertM2,
        convertPercent
    } from '$walter/services/utils';
    import type { WalterZeitraum } from '$walter/types/WalterBetriebskostenabrechnung.type';
    import {
        StructuredListCell,
        StructuredListRow
    } from 'carbon-components-svelte';

    export let beginn: Date;
    export let ende: Date;
    export let value: string | number | undefined;
    export let gesamt: string | number | undefined;
    export let anteil: number;
    export let tage: number;
</script>

<StructuredListRow>
    <StructuredListCell>
        {value} / {gesamt}
    </StructuredListCell>
    <StructuredListCell>
        {convertDateGerman(new Date(beginn))} - {convertDateGerman(
            new Date(ende)
        )}
    </StructuredListCell>
    <StructuredListCell>{tage}</StructuredListCell>
    <StructuredListCell>{convertPercent(anteil)}</StructuredListCell>
</StructuredListRow>
