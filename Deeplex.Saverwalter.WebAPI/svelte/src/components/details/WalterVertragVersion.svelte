<script lang="ts">
    import {
        WalterDatePicker,
        WalterNumberInput,
        WalterTextArea
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type { WalterVertragVersionEntry } from '$walter/lib';

    export let entry: Partial<WalterVertragVersionEntry> = {};
    export let fetchImpl: typeof fetch | undefined = undefined; // NOTE: Needed to load copy preview fetchImpl...?
</script>

<Row>
    <WalterDatePicker bind:value={entry.beginn} labelText="Beginn" />
    <WalterNumberInput
        hideSteppers
        bind:value={entry.grundmiete}
        label="Grundmiete"
    />
    <WalterNumberInput bind:value={entry.personenzahl} label="Personenzahl" />
</Row>
<Row>
    <WalterTextArea labelText="Notiz" bind:value={entry.notiz} />
</Row>
