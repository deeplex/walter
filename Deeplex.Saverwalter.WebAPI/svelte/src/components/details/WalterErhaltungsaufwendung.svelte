<script lang="ts">
    import {
        WalterComboBox,
        WalterDatePicker,
        WalterNumberInput,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type {
        WalterErhaltungsaufwendungEntry,
        WalterSelectionEntry
    } from '$walter/lib';

    export let kontakte: WalterSelectionEntry[];
    export let wohnungen: WalterSelectionEntry[];

    export let entry: Partial<WalterErhaltungsaufwendungEntry> = {};
</script>

<Row>
    <WalterTextInput bind:value={entry.bezeichnung} labelText="Bezeichnung" />
    <WalterComboBox
        bind:value={entry.aussteller}
        titleText="Aussteller"
        entry={kontakte}
    />
    <WalterDatePicker bind:value={entry.datum} labelText="Datum" />
</Row>
<Row>
    <WalterComboBox
        bind:value={entry.wohnung}
        titleText="Wohnung"
        entry={wohnungen}
    />
    <WalterNumberInput bind:value={entry.betrag} label="Betrag" />
</Row>
<Row>
    <WalterTextArea bind:value={entry.notiz} labelText="Notiz" />
</Row>
