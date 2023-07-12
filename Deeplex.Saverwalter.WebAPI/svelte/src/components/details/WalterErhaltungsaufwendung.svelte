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

    const kontakte = walter_selection.kontakte(fetchImpl);
    const wohnungen = walter_selection.wohnungen(fetchImpl);
</script>

<Row>
    <WalterTextInput bind:value={entry.bezeichnung} labelText="Bezeichnung" />
    {#await kontakte}
        <TextInputSkeleton />
    {:then entries}
        <WalterComboBox
            bind:value={entry.aussteller}
            titleText="Aussteller"
            entry={entries}
        />
        <WalterDatePicker bind:value={entry.datum} labelText="Datum" />
    {/await}
</Row>
<Row>
    {#await wohnungen}
        <TextInputSkeleton />
    {:then entries}
        <WalterComboBox
            bind:value={entry.wohnung}
            titleText="Wohnung"
            entry={entries}
        />
    {/await}
    <WalterNumberInput bind:value={entry.betrag} label="Betrag" />
</Row>
<Row>
    <WalterTextArea bind:value={entry.notiz} labelText="Notiz" />
</Row>
