<script lang="ts">
    import {
        WalterAdresse,
        WalterComboBox,
        WalterMultiSelect,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import type { WalterSelectionEntry, WalterZaehlerEntry } from '$walter/lib';
    import { Row } from 'carbon-components-svelte';

    export let entry: Partial<WalterZaehlerEntry> = {};
    export let zaehlertypen: WalterSelectionEntry[];
    export let wohnungen: WalterSelectionEntry[];
    export let umlagen: WalterSelectionEntry[];
</script>

<Row>
    <WalterTextInput bind:value={entry.kennnummer} labelText="Kennnummer" />
    <WalterComboBox
        bind:value={entry.typ}
        titleText="Typ"
        entry={zaehlertypen}
    />
</Row>
<WalterAdresse bind:value={entry.adresse} />
<Row>
    <WalterComboBox
        bind:value={entry.wohnung}
        titleText="Wohnung"
        entry={wohnungen}
    />
</Row>
<Row>
    <WalterMultiSelect
        bind:value={entry.selectedUmlagen}
        entry={umlagen}
        titleText="Umlagen"
    />
</Row>
<Row>
    <WalterTextArea bind:value={entry.notiz} labelText="Notiz" />
</Row>
