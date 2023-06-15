<script lang="ts">
    import {
        WalterMultiSelect,
        WalterPerson,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import type {
        WalterJuristischePersonEntry,
        WalterSelectionEntry
    } from '$walter/lib';
    import { Row } from 'carbon-components-svelte';

    export let entry: Partial<WalterJuristischePersonEntry>;
    export let juristischePersonen: WalterSelectionEntry[];
    export let kontakte: WalterSelectionEntry[];
</script>

<Row>
    <WalterTextInput labelText="Bezeichnung" value={entry.name} />
</Row>
<WalterPerson value={entry} />
<Row>
    <WalterMultiSelect
        bind:entry={juristischePersonen}
        titleText="Juristische Personen"
        bind:value={entry.selectedJuristischePersonen}
    />
    <WalterMultiSelect
        titleText="Mitglieder"
        entry={kontakte}
        bind:value={entry.selectedMitglieder}
    />
</Row>
<Row>
    <WalterTextArea bind:value={entry.notiz} labelText="Notiz" />
</Row>
