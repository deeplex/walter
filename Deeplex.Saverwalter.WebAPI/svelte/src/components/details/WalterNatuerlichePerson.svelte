<script lang="ts">
    import {
        WalterMultiSelect,
        WalterPerson,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import type {
        WalterNatuerlichePersonEntry,
        WalterSelectionEntry
    } from '$walter/lib';
    import { Row } from 'carbon-components-svelte';

    export let entry: Partial<WalterNatuerlichePersonEntry>;
    export let juristischePersonen: WalterSelectionEntry[];
</script>

<Row>
    <WalterTextInput bind:value={entry.vorname} labelText="Vorname" />
    <WalterTextInput bind:value={entry.nachname} labelText="Nachname" />
</Row>
<WalterPerson value={entry} />
<Row>
    <WalterMultiSelect
        bind:entry={juristischePersonen}
        titleText="Juristische Personen"
        bind:value={entry.selectedJuristischePersonen}
    />
</Row>
<Row>
    <WalterTextArea bind:value={entry.notiz} labelText="Notiz" />
</Row>
