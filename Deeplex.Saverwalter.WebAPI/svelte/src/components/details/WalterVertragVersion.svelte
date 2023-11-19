<script lang="ts">
    import {
        WalterDatePicker,
        WalterNumberInput,
        WalterTextArea
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type { WalterVertragVersionEntry } from '$walter/lib';

    export let entry: Partial<WalterVertragVersionEntry> = {};
    export const fetchImpl: typeof fetch | undefined = undefined; // NOTE: Needed to load copy preview fetchImpl...?
    export let readonly = false;
</script>

<Row>
    <WalterDatePicker
        required
        disabled={readonly}
        bind:value={entry.beginn}
        labelText="Beginn"
    />
    <WalterNumberInput
        required
        {readonly}
        hideSteppers
        bind:value={entry.grundmiete}
        label="Grundmiete"
    />
    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.personenzahl}
        label="Personenzahl"
    />
</Row>
<Row>
    <WalterTextArea {readonly} labelText="Notiz" bind:value={entry.notiz} />
</Row>
