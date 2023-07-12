<script lang="ts">
    import {
        WalterComboBox,
        WalterDatePicker,
        WalterNumberInput,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import { Row, TextInputSkeleton } from 'carbon-components-svelte';
    import type { WalterErhaltungsaufwendungEntry } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';

    export let entry: Partial<WalterErhaltungsaufwendungEntry> = {};
    export let fetchImpl: typeof fetch;

    const kontakte = walter_selection.kontakte(fetchImpl);
    const wohnungen = walter_selection.wohnungen(fetchImpl);
</script>

<Row>
    <WalterTextInput bind:value={entry.bezeichnung} labelText="Bezeichnung" />
    <WalterComboBox
        bind:value={entry.aussteller}
        titleText="Aussteller"
        entries={kontakte}
    />
    <WalterDatePicker bind:value={entry.datum} labelText="Datum" />
</Row>
<Row>
    <WalterComboBox
        bind:value={entry.wohnung}
        titleText="Wohnung"
        entries={wohnungen}
    />
    <WalterNumberInput bind:value={entry.betrag} label="Betrag" />
</Row>
<Row>
    <WalterTextArea bind:value={entry.notiz} labelText="Notiz" />
</Row>
