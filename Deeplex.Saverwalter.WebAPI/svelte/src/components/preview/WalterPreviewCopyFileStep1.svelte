<script lang="ts">
    import {
        Button,
        DataTableSkeleton,
        SkeletonText
    } from 'carbon-components-svelte';
    import { WalterDataTable } from '..';
    import type { WalterSelectionEntry } from '$walter/lib';

    export let step: number;
    export let rows: WalterSelectionEntry[] | undefined;
    export let selectedEntry: WalterSelectionEntry | undefined;
    export let selectedEntry_change: (e: CustomEvent<any>) => void;

    const headers = [{ key: 'text', value: 'Bezeichnung' }];

    function click() {
        selectedEntry = undefined;
        setTimeout(() => (step = 2), 0);
    }
</script>

{#if step === 1}
    <Button on:click={click}>Neuer Eintrag</Button>
    {#if rows}
        <WalterDataTable
            fullHeight
            navigate={selectedEntry_change}
            {headers}
            {rows}
            search
        />
    {:else}
        <SkeletonText style="margin: 0; height: 48px" />
        <DataTableSkeleton {headers} showHeader={false} showToolbar={false} />
    {/if}
{/if}
