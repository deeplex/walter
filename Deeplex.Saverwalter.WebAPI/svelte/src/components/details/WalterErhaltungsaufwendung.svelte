<script lang="ts">
    import {
        WalterDatePicker,
        WalterNumberInput,
        WalterTextArea,
        WalterTextInput,
        WalterComboBoxKontakt,
        WalterComboBoxWohnung
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type { WalterErhaltungsaufwendungEntry } from '$walter/lib';

    export let entry: Partial<WalterErhaltungsaufwendungEntry> = {};
    export let fetchImpl: typeof fetch;
    export let readonly = false;
</script>

<Row>
    <WalterTextInput
        required
        {readonly}
        bind:value={entry.bezeichnung}
        labelText="Bezeichnung"
    />
    <WalterComboBoxKontakt
        {fetchImpl}
        required
        {readonly}
        bind:value={entry.aussteller}
        title="Aussteller"
    />
    <WalterDatePicker
        required
        disabled={readonly}
        bind:value={entry.datum}
        labelText="Datum"
    />
</Row>
<Row>
    <WalterComboBoxWohnung
        required
        {readonly}
        {fetchImpl}
        bind:value={entry.wohnung}
    />
    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.betrag}
        label="Betrag"
    />
</Row>
<Row>
    <WalterTextArea {readonly} bind:value={entry.notiz} labelText="Notiz" />
</Row>
