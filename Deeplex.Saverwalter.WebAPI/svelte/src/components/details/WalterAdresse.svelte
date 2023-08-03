<script lang="ts">
    import { Row } from 'carbon-components-svelte';

    import type { WalterAdresseEntry } from '$walter/lib';
    import { WalterTextInput } from '$walter/components';

    export let entry: Partial<WalterAdresseEntry> | undefined = {};
    export const fetchImpl: typeof fetch | undefined = undefined; // NOTE: Needed to load copy preview fetchImpl...?
    export let readonly = false;
    export let required = false;

    let fallback: Partial<WalterAdresseEntry> = entry || {};

    const change = () => {
        entry = fallback;
    };
</script>

<Row>
    {#if entry}
        <WalterTextInput
            {required}
            {readonly}
            labelText="Straße"
            bind:value={entry.strasse} />
        <WalterTextInput
            {required}
            {readonly}
            labelText="Hausnr."
            bind:value={entry.hausnummer} />
        <WalterTextInput
            {required}
            {readonly}
            labelText="Postleitzahl"
            bind:value={entry.postleitzahl}
        />
        <WalterTextInput
            {required}
            {readonly}
            labelText="Stadt"
            bind:value={entry.stadt} />
    {:else}
        <WalterTextInput
            {required}
            {readonly}
            {change}
            labelText="Straße"
            bind:value={fallback.strasse}
        />
        <WalterTextInput
            {required}
            {readonly}
            {change}
            labelText="Hausnr."
            bind:value={fallback.hausnummer}
        />
        <WalterTextInput
            {required}
            {readonly}
            {change}
            labelText="Postleitzahl"
            bind:value={fallback.postleitzahl}
        />
        <WalterTextInput
            {required}
            {readonly}
            {change}
            labelText="Stadt"
            bind:value={fallback.stadt}
        />
    {/if}
</Row>
