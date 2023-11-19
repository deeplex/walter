<script lang="ts">
    import { AccordionItem, Tile } from 'carbon-components-svelte';

    import { WalterDataTable } from '$walter/components';
    import { page } from '$app/stores';
    import WalterDataWrapperQuickAdd from './WalterDataWrapperQuickAdd.svelte';
    import { walter_goto } from '$walter/services/utils';

    export let fullHeight = false;
    export let addUrl: string | undefined = undefined;
    export let addEntry: unknown | undefined = undefined;
    export let title: string | undefined = undefined;
    export let rows: unknown[];
    export let headers: {
        key: string;
        value: string;
    }[];
    export let navigate: (e: CustomEvent) => Promise<void> | void = () => {};

    let open = false;
    let addModalOpen = false;
    let quick_add = () => {
        addModalOpen = true;
    };

    function normal_add() {
        walter_goto(`${$page.url.pathname}/new`);
    }

    function onSubmit(new_value: unknown) {
        rows = [...rows, new_value];
    }
</script>

{#if title !== undefined}
    {#if addUrl}
        <WalterDataWrapperQuickAdd
            bind:addEntry
            {addUrl}
            bind:addModalOpen
            {onSubmit}
            {title}
        >
            <slot />
        </WalterDataWrapperQuickAdd>
    {/if}

    <AccordionItem title={`${title} (${rows.length})`} bind:open>
        <Tile>
            <WalterDataTable
                add={addUrl && addEntry && quick_add}
                {navigate}
                bind:rows
                {headers}
            />
        </Tile>
    </AccordionItem>
{:else}
    <WalterDataTable
        add={normal_add}
        {fullHeight}
        {navigate}
        {rows}
        {headers}
    />
{/if}
