<script lang="ts">
    import {
        WalterDatePicker,
        WalterNumberInput,
        WalterTextArea
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type { WalterZaehlerstandEntry } from '$walter/lib';
    import WalterTextInput from '../elements/WalterTextInput.svelte';

    export let entry: Partial<WalterZaehlerstandEntry> = {};
    export let fetchImpl: typeof fetch | undefined = undefined; // NOTE: Needed to load copy preview fetchImpl...?
    export let readonly = false;
</script>

<Row>
    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.stand}
        label="Zählerstand" />
    <WalterDatePicker
        required
        disabled={readonly}
        bind:value={entry.datum}
        labelText="Ablesedatum" />
    <WalterTextInput
        required
        {readonly}
        bind:value={entry.einheit}
        labelText="Einheit" />
</Row>
<Row>
    <WalterTextArea {readonly} labelText="Notiz" bind:value={entry.notiz} />
</Row>
