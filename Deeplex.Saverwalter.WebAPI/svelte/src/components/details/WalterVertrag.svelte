<script lang="ts">
    import {
        WalterComboBox,
        WalterDatePicker,
        WalterMultiSelect,
        WalterTextArea
    } from '$walter/components';
    import type { WalterSelectionEntry, WalterVertragEntry } from '$walter/lib';
    import { Row, TextInput } from 'carbon-components-svelte';

    export let a: Partial<WalterVertragEntry> = {};
    export let wohnungen: WalterSelectionEntry[];
    export let kontakte: WalterSelectionEntry[];
</script>

<Row>
    <WalterDatePicker
        disabled
        bind:value={a.beginn}
        labelText="Beginn (aus Vertragsversion)"
    />
    <WalterDatePicker
        bind:value={a.ende}
        labelText="Ende"
        placeholder="Offen"
    />
</Row>
<Row>
    <WalterComboBox bind:value={a.wohnung} a={wohnungen} titleText="Wohnung" />
    <TextInput
        labelText="Vermieter"
        readonly
        value={kontakte.find((e) => e.id === a.wohnung?.filter)?.text}
    />
    <WalterComboBox
        bind:value={a.ansprechpartner}
        a={kontakte}
        titleText="Ansprechpartner"
    />
</Row>
<Row>
    <WalterMultiSelect
        bind:value={a.selectedMieter}
        a={kontakte}
        titleText="Mieter"
    />
</Row>
<Row>
    <WalterTextArea labelText="Notiz" bind:value={a.notiz} />
</Row>
