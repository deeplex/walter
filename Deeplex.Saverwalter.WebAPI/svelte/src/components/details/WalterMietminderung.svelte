<script lang="ts">
    import {
        WalterDatePicker,
        WalterNumberInput,
        WalterTextArea
    } from '$WalterComponents';
    import { Row } from 'carbon-components-svelte';
    import type { WalterMietminderungEntry } from '$WalterLib';

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
