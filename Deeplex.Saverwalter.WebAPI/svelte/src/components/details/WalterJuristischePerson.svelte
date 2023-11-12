<script lang="ts">
    import {
        WalterMultiSelectPerson,
        WalterMultiSelectJuristischePerson,
        WalterPerson,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import type {
        WalterJuristischePersonEntry,
    } from '$walter/lib';
    import { Row } from 'carbon-components-svelte';

    export let entry: Partial<WalterJuristischePersonEntry>;
    export let fetchImpl: typeof fetch;
    export let readonly = false;
</script>

<Row>
    <WalterTextInput required {readonly} labelText="Bezeichnung" bind:value={entry.name} />
</Row>
<WalterPerson {readonly} value={entry} />
<Row>
    <WalterMultiSelectJuristischePerson
        {readonly}
        {fetchImpl}
        titleText="Juristische Personen"
        bind:value={entry.selectedJuristischePersonen}
    />
    <WalterMultiSelectPerson
        {readonly}
        {fetchImpl}
        titleText="Mitglieder"
        bind:value={entry.selectedMitglieder}
    />
</Row>
<Row>
    <WalterTextArea {readonly} bind:value={entry.notiz} labelText="Notiz" />
</Row>
