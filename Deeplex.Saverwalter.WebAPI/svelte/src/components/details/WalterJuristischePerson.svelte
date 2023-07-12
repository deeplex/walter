<script lang="ts">
    import {
        WalterMultiSelect,
        WalterPerson,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import type {
        WalterJuristischePersonEntry,
        WalterSelectionEntry
    } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';
    import { Row, TextInputSkeleton } from 'carbon-components-svelte';
    import { removeSelf } from './WalterBetriebskostenrechnung';

    export let entry: Partial<WalterJuristischePersonEntry>;
    export let fetchImpl: typeof fetch;

    const juristischePersonen = walter_selection
        .juristischePersonen(fetchImpl)
        .then((entries) => removeSelf(entries, entry));
    const kontakte = walter_selection
        .kontakte(fetchImpl)
        .then((entries) => removeSelf(entries, entry));
</script>

<Row>
    <WalterTextInput labelText="Bezeichnung" value={entry.name} />
</Row>
<WalterPerson value={entry} />
<Row>
    {#await juristischePersonen}
        <TextInputSkeleton />
    {:then entries}
        <WalterMultiSelect
            entry={entries}
            titleText="Juristische Personen"
            bind:value={entry.selectedJuristischePersonen}
        />
    {/await}
    {#await kontakte}
        <TextInputSkeleton />
    {:then entries}
        <WalterMultiSelect
            titleText="Mitglieder"
            entry={entries}
            bind:value={entry.selectedMitglieder}
        />
    {/await}
</Row>
<Row>
    <WalterTextArea bind:value={entry.notiz} labelText="Notiz" />
</Row>
