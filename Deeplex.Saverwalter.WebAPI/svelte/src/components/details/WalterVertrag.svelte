<script lang="ts">
    import {
        WalterComboBox,
        WalterDatePicker,
        WalterMultiSelect,
        WalterTextArea
    } from '$walter/components';
    import type { WalterSelectionEntry, WalterVertragEntry } from '$walter/lib';
    import { Row, TextInput } from 'carbon-components-svelte';

    export let entry: Partial<WalterVertragEntry> = {};
    export let wohnungen: WalterSelectionEntry[];
    export let kontakte: WalterSelectionEntry[];
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
        entry={wohnungen}
        titleText="Wohnung"
    />
    <TextInput
        labelText="Vermieter"
        readonly
        value={kontakte.find((e) => e.id === entry.wohnung?.filter)?.text}
    />
    <WalterComboBox
        bind:value={entry.ansprechpartner}
        entry={kontakte}
        titleText="Ansprechpartner"
    />
</Row>
<Row>
    <WalterMultiSelect
        bind:value={entry.selectedMieter}
        entry={kontakte}
        titleText="Mieter"
    />
</Row>
<Row>
    <WalterTextArea labelText="Notiz" bind:value={entry.notiz} />
</Row>
