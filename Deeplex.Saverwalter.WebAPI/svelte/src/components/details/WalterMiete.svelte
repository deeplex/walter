<script lang="ts">
    import {
        WalterDatePicker,
        WalterNumberInput,
        WalterTextArea
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type { WalterMieteEntry } from '$walter/lib';

    export let entry: Partial<WalterMieteEntry> = {};
</script>

<Row>
    <WalterNumberInput bind:value={entry.betrag} label="Betrag" />
    <WalterDatePicker
        bind:value={entry.betreffenderMonat}
        labelText="Betreffender Monat"
    />
    <WalterDatePicker
        bind:value={entry.zahlungsdatum}
        labelText="Zahlungsdatum"
    />
</Row>
<Row>
    <WalterTextArea labelText="Notiz" bind:value={entry.notiz} />
</Row>
