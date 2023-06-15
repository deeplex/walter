<script lang="ts">
    import {
        WalterDatePicker,
        WalterNumberInput,
        WalterTextArea
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type { WalterMietminderungEntry } from '$walter/lib';

    export let entry: Partial<WalterMietminderungEntry> = {};
</script>

<Row>
    <WalterNumberInput bind:value={entry.minderung} label="Minderung" />
    <WalterDatePicker bind:value={entry.beginn} labelText="Beginn" />
    <WalterDatePicker
        placeholder="Offen"
        bind:value={entry.ende}
        labelText="Ende"
    />
</Row>
<Row>
    <WalterTextArea labelText="Notiz" bind:value={entry.notiz} />
</Row>
