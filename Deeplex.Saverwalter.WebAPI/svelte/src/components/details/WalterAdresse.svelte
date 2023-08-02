<script lang="ts">
    import { Row } from 'carbon-components-svelte';

    import type { WalterAdresseEntry } from '$walter/lib';
    import { WalterTextInput } from '$walter/components';

    export let entry: Partial<WalterAdresseEntry> | undefined = {};
    export let fetchImpl: typeof fetch | undefined = undefined; // NOTE: Needed to load copy preview fetchImpl...?
    export let readonly = false;

    let fallback: Partial<WalterAdresseEntry> = entry || {};

    const change = () => {
        entry = fallback;
    };
</script>

<Row>
    {#if entry}
        <WalterTextInput {readonly} labelText="Straße" bind:value={entry.strasse} />
        <WalterTextInput {readonly} labelText="Hausnr." bind:value={entry.hausnummer} />
        <WalterTextInput
            {readonly}
            labelText="Postleitzahl"
            bind:value={entry.postleitzahl}
        />
        <WalterTextInput {readonly} labelText="Stadt" bind:value={entry.stadt} />
    {:else}
        <WalterTextInput
            {readonly}
            {change}
            labelText="Straße"
            bind:value={fallback.strasse}
        />
        <WalterTextInput
            {readonly}
            {change}
            labelText="Hausnr."
            bind:value={fallback.hausnummer}
        />
        <WalterTextInput
            {readonly}
            {change}
            labelText="Postleitzahl"
            bind:value={fallback.postleitzahl}
        />
        <WalterTextInput
            {readonly}
            {change}
            labelText="Stadt"
            bind:value={fallback.stadt}
        />
    {/if}
</Row>
