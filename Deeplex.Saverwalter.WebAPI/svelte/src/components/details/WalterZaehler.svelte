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

    const zaehlertypen = walter_selection.zaehlertypen(fetchImpl);
    const wohnungen = walter_selection.wohnungen(fetchImpl);
    const umlagen = walter_selection.umlagen(fetchImpl);
</script>

<Row>
    <WalterTextInput bind:value={entry.kennnummer} labelText="Kennnummer" />
    <WalterComboBox
        bind:value={entry.typ}
        titleText="Typ"
        entries={zaehlertypen}
    />
</Row>
<WalterAdresse bind:entry={entry.adresse} />
<Row>
    <WalterComboBox
        bind:value={entry.wohnung}
        titleText="Wohnung"
        entries={wohnungen}
    />
</Row>
<Row>
    {#await umlagen}
        <TextInputSkeleton />
    {:then entries}
        <WalterMultiSelect
            bind:value={entry.selectedUmlagen}
            entry={entries}
            titleText="Umlagen"
        />
    {/await}
</Row>
<Row>
    <WalterTextArea bind:value={entry.notiz} labelText="Notiz" />
</Row>
