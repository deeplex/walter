<script lang="ts">
    import {
        WalterAdresse,
        WalterComboBox,
        WalterComboBoxWohnung,
        WalterMultiSelectUmlage,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import type { WalterZaehlerEntry } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';
    import { Row } from 'carbon-components-svelte';
    import WalterDatePicker from '../elements/WalterDatePicker.svelte';

    export let entry: Partial<WalterZaehlerEntry> = {};
    export let fetchImpl: typeof fetch;
    export let readonly = false;

    const zaehlertypen = walter_selection.zaehlertypen(fetchImpl);
</script>

<Row>
    <WalterTextInput
        required
        {readonly}
        bind:value={entry.kennnummer}
        labelText="Kennnummer"
    />
    <WalterComboBox
        required
        {readonly}
        bind:value={entry.typ}
        titleText="Typ"
        entries={zaehlertypen}
    />
    <WalterDatePicker
        disabled={readonly}
        bind:value={entry.ende}
        labelText="Ende"
    />
</Row>
<WalterAdresse {readonly} bind:entry={entry.adresse} />
<Row>
    <WalterComboBoxWohnung {fetchImpl} {readonly} bind:value={entry.wohnung} />
</Row>
<Row>
    <WalterMultiSelectUmlage
        {fetchImpl}
        {readonly}
        bind:value={entry.selectedUmlagen}
        titleText="Umlagen"
    />
</Row>
<Row>
    <WalterTextArea {readonly} bind:value={entry.notiz} labelText="Notiz" />
</Row>
