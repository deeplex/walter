<script lang="ts">
    import {
        WalterDatePicker,
        WalterTextArea,
        WalterComboBoxKontakt,
        WalterComboBoxWohnung,
        WalterMultiSelectKontakt
    } from '$walter/components';
    import type { WalterVertragEntry } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';
    import {
        Row,
        TextInput,
        TextInputSkeleton
    } from 'carbon-components-svelte';
    import { convertDateGerman } from '$walter/services/utils';

    export let entry: Partial<WalterVertragEntry> = {};
    export let fetchImpl: typeof fetch;
    export let readonly = false;

    const kontakte = walter_selection.kontakte(fetchImpl);
</script>

<Row>
    <TextInput
        placeholder="Wird aus Vertragsversion genommen"
        required
        readonly
        value={entry.beginn && convertDateGerman(new Date(entry.beginn))}
        labelText="Beginn (aus Vertragsversion)"
    />
    <WalterDatePicker
        disabled={readonly}
        bind:value={entry.ende}
        labelText="Ende"
        placeholder="Offen"
    />
</Row>
<Row>
    <WalterComboBoxWohnung
        {fetchImpl}
        required
        {readonly}
        bind:value={entry.wohnung}
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
    <WalterComboBoxKontakt
        {fetchImpl}
        {readonly}
        bind:value={entry.ansprechpartner}
        title="Ansprechpartner"
    />
</Row>
<Row>
    <WalterMultiSelectKontakt
        {fetchImpl}
        {readonly}
        bind:value={entry.selectedMieter}
        titleText="Mieter"
    />
</Row>
<Row>
    <WalterTextArea {readonly} labelText="Notiz" bind:value={entry.notiz} />
</Row>
