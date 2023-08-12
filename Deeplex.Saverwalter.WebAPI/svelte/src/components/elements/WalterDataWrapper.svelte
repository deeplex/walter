<script lang="ts">
    import {
        AccordionItem,
        Modal,
        Tile
    } from 'carbon-components-svelte';

    import { WalterDataTable } from '$walter/components';
    import { handle_save } from './WalterDataWrapper';
    import { goto } from '$app/navigation';
    import { page } from '$app/stores';
    import WalterDataWrapperQuickAdd from './WalterDataWrapperQuickAdd.svelte';

    export let fullHeight = false;
    export let addUrl: string | undefined = undefined;
    export let addEntry: any | undefined = undefined;
    export let title: string | undefined = undefined;
    export let rows: any[];
    export let headers: {
        key: string;
        value: string;
    }[];
    export let navigate: (e: CustomEvent) => Promise<void> | void = (
        _e: unknown
    ) => {};

    let open = false;
    let addModalOpen = false;
    let quick_add = () => {
        addModalOpen = true;
    }

    function normal_add() {
        goto(`${$page.url.pathname}/new`);
    }
</script>

{#if title !== undefined}
    {#if addUrl}
        <WalterDataWrapperQuickAdd
            bind:addEntry
            {addUrl}
            {addModalOpen}
            bind:rows
            {title}>
            <slot />
        </WalterDataWrapperQuickAdd>
    {/if}

    <AccordionItem title={`${title} (${rows.length})`} bind:open>
        <Tile>
            <WalterDataTable add={addUrl && addEntry && quick_add} {navigate} bind:rows {headers} />
        </Tile>
    </AccordionItem>
{:else}
    <WalterDataTable add={normal_add} {fullHeight} {navigate} {rows} {headers} />
{/if}
