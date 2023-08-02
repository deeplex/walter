<script lang="ts">
    import {
        WalterMultiSelect,
        WalterPerson,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import type {
        WalterJuristischePersonEntry,
    } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';
    import { Row } from 'carbon-components-svelte';
    import { removeSelf } from './WalterBetriebskostenrechnung';

    export let entry: Partial<WalterJuristischePersonEntry>;
    export let fetchImpl: typeof fetch;
    export let readonly = false;

    const juristischePersonen = walter_selection
        .juristischePersonen(fetchImpl)
        .then((entries) => removeSelf(entries, entry));
    const kontakte = walter_selection
        .kontakte(fetchImpl)
        .then((entries) => removeSelf(entries, entry));
</script>

<Row>
    <WalterTextInput labelText="Bezeichnung" value={entry.name} />
</Row>
<WalterPerson {readonly} value={entry} />
<Row>
    <WalterMultiSelect
        disabled={readonly}
        entries={juristischePersonen}
        titleText="Juristische Personen"
        bind:value={entry.selectedJuristischePersonen}
    />
    <WalterMultiSelect
        disabled={readonly}
        titleText="Mitglieder"
        entries={kontakte}
        bind:value={entry.selectedMitglieder}
    />
</Row>
<Row>
    <WalterTextArea {readonly} bind:value={entry.notiz} labelText="Notiz" />
</Row>
