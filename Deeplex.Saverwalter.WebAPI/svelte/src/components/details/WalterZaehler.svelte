<script lang="ts">
    import {
        WalterAdresse,
        WalterComboBox,
        WalterMultiSelect,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import type { WalterSelectionEntry, WalterZaehlerEntry } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';
    import { Row, TextInputSkeleton } from 'carbon-components-svelte';

    export let entry: Partial<WalterZaehlerEntry> = {};
    export let fetchImpl: typeof fetch;
    export let readonly = false;

    const zaehlertypen = walter_selection.zaehlertypen(fetchImpl);
    const wohnungen = walter_selection.wohnungen(fetchImpl);
    const umlagen = walter_selection.umlagen(fetchImpl);
</script>

<Row>
    <WalterTextInput {readonly} bind:value={entry.kennnummer} labelText="Kennnummer" />
    <WalterComboBox
        {readonly}
        bind:value={entry.typ}
        titleText="Typ"
        entries={zaehlertypen}
    />
</Row>
<WalterAdresse {readonly} bind:entry={entry.adresse} />
<Row>
    <WalterComboBox
        {readonly}
        bind:value={entry.wohnung}
        titleText="Wohnung"
        entries={wohnungen}
    />
</Row>
<Row>
    <WalterMultiSelect
        disabled={readonly}
        bind:value={entry.selectedUmlagen}
        entries={umlagen}
        titleText="Umlagen"
    />
</Row>
<Row>
    <WalterTextArea {readonly} bind:value={entry.notiz} labelText="Notiz" />
</Row>
