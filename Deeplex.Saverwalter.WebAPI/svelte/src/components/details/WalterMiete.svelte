<script lang="ts">
    import {
        WalterDatePicker,
        WalterLinks,
        WalterMieten,
        WalterNumberInput,
        WalterTextArea
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type { WalterMieteEntry } from '$walter/lib';
    import WalterLinkTile from '../subdetails/WalterLinkTile.svelte';
    import { S3URL } from '$walter/services/s3';

    export let entry: Partial<WalterMieteEntry> = {};
    export let mieten: WalterMieteEntry[] = [];
    export const fetchImpl: typeof fetch | undefined = undefined; // NOTE: Needed to load copy preview fetchImpl...?
    export let readonly = false;
    $: {
        readonly = entry?.permissions?.update === false;
    }
</script>

<Row>
    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.betrag}
        label="Betrag"
    />
    <WalterDatePicker
        required
        disabled={readonly}
        bind:value={entry.betreffenderMonat}
        labelText="Betreffender Monat"
    />
    <WalterDatePicker
        required
        disabled={readonly}
        bind:value={entry.zahlungsdatum}
        labelText="Zahlungsdatum"
    />
</Row>
<Row>
    <WalterTextArea {readonly} labelText="Notiz" bind:value={entry.notiz} />
</Row>

<WalterLinks>
    <WalterLinkTile
        s3ref={S3URL.vertrag(`${entry.vertrag?.id}`)}
        name={`Vertrag: ${mieten[0]?.vertrag?.text || 'ansehen'}`}
        href={`/vertraege/${entry.vertrag?.id}`}
    />
    {#if entry.vertrag?.id}
        <WalterMieten
            title="Mieten"
            rows={mieten.filter(
                (e) => +e.vertrag.id === +(entry.vertrag?.id || 0)
            )}
        />
    {/if}
</WalterLinks>
