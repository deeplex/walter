<script lang="ts">
    import {
        WalterDatePicker,
        WalterNumberInput,
        WalterTextArea
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type { WalterMietminderungEntry } from '$walter/lib';

    export let a: Partial<WalterMietminderungEntry> = {};
</script>

<Row>
    <WalterNumberInput bind:value={a.minderung} label="Minderung" />
    <WalterDatePicker bind:value={a.beginn} labelText="Beginn" />
    <WalterDatePicker
        placeholder="Offen"
        bind:value={a.ende}
        labelText="Ende"
    />
</Row>
<Row>
    <WalterTextArea labelText="Notiz" bind:value={a.notiz} />
</Row>
