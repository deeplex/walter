<script lang="ts">
    import {
        WalterDatePicker,
        WalterNumberInput,
        WalterTextArea
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type { WalterMietminderungEntry } from '$walter/lib';

    export let entry: Partial<WalterMietminderungEntry> = {};
    export let fetchImpl: typeof fetch | undefined = undefined; // NOTE: Needed to load copy preview fetchImpl...?
    export let readonly = false;
</script>

<Row>
    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.minderung}
        label="Minderung" />
    <WalterDatePicker
        required
        disabled={readonly}
        bind:value={entry.beginn}
        labelText="Beginn" />
    <WalterDatePicker
        disabled={readonly}
        placeholder="Offen"
        bind:value={entry.ende}
        labelText="Ende"
    />
</Row>
<Row>
    <WalterTextArea {readonly} labelText="Notiz" bind:value={entry.notiz} />
</Row>
