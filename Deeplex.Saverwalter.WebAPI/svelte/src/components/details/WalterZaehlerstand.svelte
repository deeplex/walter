<script lang="ts">
    import {
        WalterDatePicker,
        WalterNumberInput,
        WalterTextArea
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import { WalterZaehlerEntry, WalterZaehlerstandEntry } from '$walter/lib';
    import WalterTextInput from '../elements/WalterTextInput.svelte';
    import { onMount } from 'svelte';
    import {
        convertDateCanadian,
        convertDateGerman
    } from '$walter/services/utils';

    export let entry: Partial<WalterZaehlerstandEntry> = {};
    export let fetchImpl: typeof fetch | undefined = undefined;
    export let readonly = false;

    let maxDate: string | undefined;
    onMount(async () => {
        if (entry.zaehler?.id && fetchImpl) {
            const date = await WalterZaehlerEntry.GetOne<WalterZaehlerEntry>(
                `${entry.zaehler?.id}`,
                fetchImpl
            ).then((zaehler) => zaehler.ende);
            maxDate = convertDateGerman(new Date(date));
        }
    });
</script>

<Row>
    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.stand}
        label="Zählerstand"
    />
    <WalterDatePicker
        {maxDate}
        required
        disabled={readonly}
        bind:value={entry.datum}
        labelText="Ablesedatum"
    />
    <WalterTextInput
        required
        readonly
        bind:value={entry.einheit}
        labelText="Einheit"
    />
</Row>
<Row>
    <WalterTextArea {readonly} labelText="Notiz" bind:value={entry.notiz} />
</Row>
