<script lang="ts">
    import {
        WalterAdresse,
        WalterComboBox,
        WalterNumberInput,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type { WalterSelectionEntry, WalterWohnungEntry } from '$walter/lib';

    export let entry: Partial<WalterWohnungEntry> = {};
    export let kontakte: WalterSelectionEntry[];
</script>

<Row>
    <WalterComboBox
        bind:value={entry.besitzer}
        titleText="Besitzer"
        entry={kontakte}
    />
</Row>
<WalterAdresse bind:value={entry.adresse} />
<Row>
    <WalterTextInput bind:value={entry.bezeichnung} labelText="Bezeichnung" />
    <WalterNumberInput bind:value={entry.wohnflaeche} label="Wohnfläche" />
    <WalterNumberInput bind:value={entry.nutzflaeche} label="Nutzfläche" />
    <WalterNumberInput
        hideSteppers={false}
        bind:value={entry.einheiten}
        label="Einheiten"
    />
</Row>
<Row>
    <WalterTextArea bind:value={entry.notiz} labelText="Notiz" />
</Row>
