<script lang="ts">
    import {
        WalterAdresse,
        WalterDropdown,
        WalterMultiSelectJuristischePerson,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import type { WalterKontaktEntry } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';
    import { Row } from 'carbon-components-svelte';
    import WalterMultiSelectKontakt from '../elements/WalterMultiSelectKontakt.svelte';

    export let entry: Partial<WalterKontaktEntry>;
    export let fetchImpl: typeof fetch;
    export let readonly = entry?.permissions?.update === false;
    export let juristisch = false;

    const anreden = walter_selection.anreden(fetchImpl);
    const rechtsformen = walter_selection
        .rechtsformen(fetchImpl)
        .then((res) => {
            if (juristisch) {
                // Disable selection of natuerliche Person
                (res[0] as any).disabled = true;
                if (entry.rechtsform?.id === 0) {
                    entry.rechtsform = res[1];
                }
            }
            return res;
        });
</script>

<Row>
    <WalterDropdown
        entries={rechtsformen}
        bind:value={entry.rechtsform}
        titleText="Rechtsform"
    />
    {#if entry.rechtsform?.id === 0}
        <WalterDropdown
            entries={anreden}
            bind:value={entry.anrede}
            titleText="Anrede"
        />
        <WalterTextInput
            {readonly}
            bind:value={entry.vorname}
            labelText="Vorname"
        />
    {/if}
    <WalterTextInput
        required
        {readonly}
        bind:value={entry.name}
        labelText="Name"
    />
</Row>

<WalterAdresse {readonly} bind:entry={entry.adresse} />
<Row>
    <WalterTextInput {readonly} bind:value={entry.email} labelText="E-Mail" />
    <WalterTextInput {readonly} bind:value={entry.fax} labelText="Fax" />
</Row>
<Row>
    <WalterTextInput
        {readonly}
        bind:value={entry.telefon}
        labelText="Telefon"
    />
    <WalterTextInput {readonly} bind:value={entry.mobil} labelText="Mobil" />
</Row>

<Row>
    <WalterMultiSelectJuristischePerson
        {readonly}
        {fetchImpl}
        titleText="Juristische Personen"
        bind:value={entry.selectedJuristischePersonen}
    />
    {#if entry.rechtsform?.id !== 0}
        <WalterMultiSelectKontakt
            {readonly}
            {fetchImpl}
            titleText="Mitglieder"
            bind:value={entry.selectedMitglieder}
        />
    {/if}
</Row>
<Row>
    <WalterTextArea {readonly} bind:value={entry.notiz} labelText="Notiz" />
</Row>
