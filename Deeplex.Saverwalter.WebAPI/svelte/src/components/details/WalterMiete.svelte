<script lang="ts">
    import {
        WalterDatePicker,
        WalterLinks,
        WalterNumberInput,
        WalterTextArea
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type { WalterMieteEntry, WalterS3FileWrapper } from '$walter/lib';
    import WalterLinkTile from '../subdetails/WalterLinkTile.svelte';
    import { page } from '$app/stores';

    export let entry: Partial<WalterMieteEntry> = {};
    export const fetchImpl: typeof fetch | undefined = undefined; // NOTE: Needed to load copy preview fetchImpl...?
    export let readonly = false;
</script>

<Row>
    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.betrag}
        label="Betrag" />
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

{#if $page.url.pathname !== `/vertraege/${entry.vertrag?.id}`}
<WalterLinks>
    <WalterLinkTile name={"Vertrag ansehen"} href={`/vertraege/${entry.vertrag?.id}`} />
</WalterLinks>
{/if}
