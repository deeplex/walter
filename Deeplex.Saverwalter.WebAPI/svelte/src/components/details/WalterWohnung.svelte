<script lang="ts">
    import {
        WalterAdresse,
        WalterComboBoxKontakt,
        WalterNumberInput,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type { WalterWohnungEntry } from '$walter/lib';

    export let entry: Partial<WalterWohnungEntry> = {};
    export let fetchImpl: typeof fetch;
    export let readonly = false;
</script>

<Row>
    <WalterComboBoxKontakt
        {fetchImpl}
        required
        {readonly}
        bind:value={entry.besitzer}
        title="Besitzer"
    />
</Row>
<WalterAdresse required {readonly} bind:entry={entry.adresse} />
<Row>
    <WalterTextInput
        required
        {readonly}
        bind:value={entry.bezeichnung}
        labelText="Bezeichnung"
    />
    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.wohnflaeche}
        label="Wohnfläche"
    />
    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.nutzflaeche}
        label="Nutzfläche"
    />
    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.einheiten}
        label="Einheiten"
    />
</Row>
<Row>
    <WalterTextArea {readonly} bind:value={entry.notiz} labelText="Notiz" />
</Row>
