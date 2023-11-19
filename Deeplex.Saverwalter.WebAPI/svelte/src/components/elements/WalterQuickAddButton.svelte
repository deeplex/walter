<script lang="ts">
    import { Button } from 'carbon-components-svelte';
    import { Add } from 'carbon-icons-svelte';
    import WalterDataWrapperQuickAdd from './WalterDataWrapperQuickAdd.svelte';

    export let addUrl: string;
    export let addEntry: unknown = {};
    export let onSubmit: undefined | ((e: unknown) => void) = undefined;
    export let title: string | undefined = undefined;

    let addModalOpen = false;
</script>

<div style="margin-top: 34px; margin-left: -0.75em">
    <Button
        on:click={() => {
            addModalOpen = true;
        }}
        icon={Add}
        iconDescription="Hinzufügen"
        size="field"
        kind="tertiary"
    />
</div>
<WalterDataWrapperQuickAdd
    {title}
    bind:addModalOpen
    {addUrl}
    bind:addEntry
    {onSubmit}
>
    {#if addModalOpen}
        <slot />
    {/if}
</WalterDataWrapperQuickAdd>
