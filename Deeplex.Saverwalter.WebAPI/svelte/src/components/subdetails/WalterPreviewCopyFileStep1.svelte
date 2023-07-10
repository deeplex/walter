<script lang="ts">
    import { DataTableSkeleton, SkeletonText } from 'carbon-components-svelte';
    import { WalterDataTable } from '..';
    import type { WalterSelectionEntry } from '$walter/lib';

    export let step: number;
    export let rows: WalterSelectionEntry[] | undefined;
    export let selectedEntry_change: (e: CustomEvent<any>) => void;

    const headers = [{ key: 'text', value: 'Bezeichnung' }];
</script>

{#if step === 1}
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
