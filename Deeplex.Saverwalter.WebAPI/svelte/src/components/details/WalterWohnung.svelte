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
    export let readonly = false;

    const kontakte = walter_selection.kontakte(fetchImpl);
</script>

<Row>
    <WalterComboBox
        {readonly}
        bind:value={entry.besitzer}
        titleText="Besitzer"
        entries={kontakte}
    />
</Row>
<WalterAdresse {readonly} bind:entry={entry.adresse} />
<Row>
    <WalterTextInput {readonly} bind:value={entry.bezeichnung} labelText="Bezeichnung" />
    <WalterNumberInput {readonly} bind:value={entry.wohnflaeche} label="Wohnfläche" />
    <WalterNumberInput {readonly} bind:value={entry.nutzflaeche} label="Nutzfläche" />
    <WalterNumberInput {readonly} bind:value={entry.einheiten} label="Einheiten" />
</Row>
<Row>
    <WalterTextArea {readonly} bind:value={entry.notiz} labelText="Notiz" />
</Row>
