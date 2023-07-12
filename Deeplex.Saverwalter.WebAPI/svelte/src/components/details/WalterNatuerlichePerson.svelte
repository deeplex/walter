<script lang="ts">
    import {
        WalterMultiSelect,
        WalterPerson,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import type { WalterNatuerlichePersonEntry } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';
    import { Row, TextInputSkeleton } from 'carbon-components-svelte';

    export let entry: Partial<WalterNatuerlichePersonEntry>;
    export let fetchImpl: typeof fetch;

    const juristischePersonen = walter_selection.juristischePersonen(fetchImpl);
</script>

<Row>
    <WalterTextInput bind:value={entry.vorname} labelText="Vorname" />
    <WalterTextInput bind:value={entry.nachname} labelText="Nachname" />
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
</Row>
<Row>
    <WalterTextArea bind:value={entry.notiz} labelText="Notiz" />
</Row>
