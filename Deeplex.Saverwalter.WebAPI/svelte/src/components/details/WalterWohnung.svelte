<script lang="ts">
    import {
        WalterAdresse,
        WalterComboBox,
        WalterNumberInput,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import { Row, TextInputSkeleton } from 'carbon-components-svelte';
    import type { WalterWohnungEntry } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';

    export let entry: Partial<WalterWohnungEntry> = {};
    export let fetchImpl: typeof fetch;

    const kontakte = walter_selection.kontakte(fetchImpl);
</script>

<Row>
    {#await kontakte}
        <TextInputSkeleton />
    {:then entries}
        <WalterComboBox
            bind:value={entry.besitzer}
            titleText="Besitzer"
            entry={entries}
        />
    {/await}
</Row>
<WalterAdresse bind:entry={entry.adresse} />
<Row>
    <WalterTextInput bind:value={entry.bezeichnung} labelText="Bezeichnung" />
    <WalterNumberInput bind:value={entry.wohnflaeche} label="Wohnfläche" />
    <WalterNumberInput bind:value={entry.nutzflaeche} label="Nutzfläche" />
    <WalterNumberInput bind:value={entry.einheiten} label="Einheiten" />
</Row>
<Row>
    <WalterTextArea bind:value={entry.notiz} labelText="Notiz" />
</Row>
