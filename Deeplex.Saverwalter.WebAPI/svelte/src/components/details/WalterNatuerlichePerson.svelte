<script lang="ts">
    import {
        WalterComboBox,
        WalterMultiSelect,
        WalterPerson,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import type { WalterNatuerlichePersonEntry } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';
    import { Row } from 'carbon-components-svelte';

    export let entry: Partial<WalterNatuerlichePersonEntry>;
    export let fetchImpl: typeof fetch;
    export let readonly = false;

    const juristischePersonen = walter_selection.juristischePersonen(fetchImpl);
    const anreden = walter_selection.anreden(fetchImpl);
</script>

<Row>
    <WalterComboBox entries={anreden} bind:value={entry.anrede} titleText="Anrede"/>
    <WalterTextInput {readonly} bind:value={entry.vorname} labelText="Vorname" />
    <WalterTextInput required {readonly} bind:value={entry.nachname} labelText="Nachname" />
</Row>
<WalterPerson {readonly} value={entry} />
<Row>
    <WalterMultiSelect
        disabled={readonly}
        entries={juristischePersonen}
        titleText="Juristische Personen"
        bind:value={entry.selectedJuristischePersonen}
    />
</Row>
<Row>
    <WalterTextArea {readonly} bind:value={entry.notiz} labelText="Notiz" />
</Row>
