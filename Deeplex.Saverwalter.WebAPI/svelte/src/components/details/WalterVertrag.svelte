<script lang="ts">
    import {
        WalterComboBox,
        WalterDatePicker,
        WalterMultiSelect,
        WalterTextArea
    } from '$walter/components';
    import type { WalterSelectionEntry, WalterVertragEntry } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';
    import {
        Row,
        TextInput,
        TextInputSkeleton
    } from 'carbon-components-svelte';

    export let entry: Partial<WalterVertragEntry> = {};
    export let fetchImpl: typeof fetch;

    const wohnungen = walter_selection.wohnungen(fetchImpl);
    const kontakte = walter_selection.kontakte(fetchImpl);
</script>

<Row>
    <WalterDatePicker
        disabled
        bind:value={entry.beginn}
        labelText="Beginn (aus Vertragsversion)"
    />
    <WalterDatePicker
        bind:value={entry.ende}
        labelText="Ende"
        placeholder="Offen"
    />
</Row>
<Row>
    <WalterComboBox
        bind:value={entry.wohnung}
        entries={wohnungen}
        titleText="Wohnung"
    />
    {#await kontakte}
        <TextInputSkeleton />
    {:then entries}
        <TextInput
            labelText="Vermieter"
            readonly
            value={entries.find((e) => e.id === entry.wohnung?.filter)?.text}
        />
    {/await}
    <WalterComboBox
        bind:value={entry.ansprechpartner}
        entries={kontakte}
        titleText="Ansprechpartner"
    />
</Row>
<Row>
    <WalterMultiSelect
        bind:value={entry.selectedMieter}
        entries={kontakte}
        titleText="Mieter"
    />
</Row>
<Row>
    <WalterTextArea labelText="Notiz" bind:value={entry.notiz} />
</Row>
