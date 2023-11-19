<script lang="ts">
    import {
        WalterDatePicker,
        WalterNumberInput,
        WalterTextArea,
        WalterTextInput,
        WalterComboBoxPerson,
        WalterComboBoxWohnung
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type {
        WalterErhaltungsaufwendungEntry,
        WalterWohnungEntry
    } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';

    export let entry: Partial<WalterErhaltungsaufwendungEntry> = {};
    export let fetchImpl: typeof fetch;
    export let readonly = false;

    const kontakte = walter_selection.kontakte(fetchImpl);
</script>

<Row>
    <WalterTextInput
        required
        {readonly}
        bind:value={entry.bezeichnung}
        labelText="Bezeichnung"
    />
    <WalterComboBoxPerson
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
