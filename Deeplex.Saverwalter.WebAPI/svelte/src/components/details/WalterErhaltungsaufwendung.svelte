<script lang="ts">
    import {
        WalterComboBox,
        WalterDatePicker,
        WalterNumberInput,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import { Row, TextInputSkeleton } from 'carbon-components-svelte';
    import type { WalterErhaltungsaufwendungEntry } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';

    export let entry: Partial<WalterErhaltungsaufwendungEntry> = {};
    export let fetchImpl: typeof fetch;
    export let readonly = false;

    const kontakte = walter_selection.kontakte(fetchImpl);
    const wohnungen = walter_selection.wohnungen(fetchImpl);
</script>

<Row>
    <WalterTextInput
        required
        {readonly}
        bind:value={entry.bezeichnung}
        labelText="Bezeichnung" />
    <WalterComboBox
        required
        {readonly}
        bind:value={entry.aussteller}
        titleText="Aussteller"
        entries={kontakte}
    />
    <WalterDatePicker
        required
        disabled={readonly}
        bind:value={entry.datum}
        labelText="Datum" />
</Row>
<Row>
    <WalterComboBox
        required
        {readonly}
        bind:value={entry.wohnung}
        titleText="Wohnung"
        entries={wohnungen}
    />
    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.betrag}
        label="Betrag" />
</Row>
<Row>
    <WalterTextArea {readonly} bind:value={entry.notiz} labelText="Notiz" />
</Row>
