<script lang="ts">
    import { Row } from 'carbon-components-svelte';

    import type { WalterAdresseEntry } from '$walter/lib';
    import { WalterTextInput } from '$walter/components';

    export let entry: Partial<WalterAdresseEntry> | undefined = {};

    let fallback: Partial<WalterAdresseEntry> = entry || {};

    const change = () => {
        entry = fallback;
    };
</script>

<Row>
    {#if entry}
        <WalterTextInput labelText="Straße" bind:value={entry.strasse} />
        <WalterTextInput labelText="Hausnr." bind:value={entry.hausnummer} />
        <WalterTextInput
            labelText="Postleitzahl"
            bind:value={entry.postleitzahl}
        />
        <WalterTextInput labelText="Stadt" bind:value={entry.stadt} />
    {:else}
        <WalterTextInput
            {change}
            labelText="Straße"
            bind:value={fallback.strasse}
        />
        <WalterTextInput
            {change}
            labelText="Hausnr."
            bind:value={fallback.hausnummer}
        />
        <WalterTextInput
            {change}
            labelText="Postleitzahl"
            bind:value={fallback.postleitzahl}
        />
        <WalterTextInput
            {change}
            labelText="Stadt"
            bind:value={fallback.stadt}
        />
    {/if}
</Row>
