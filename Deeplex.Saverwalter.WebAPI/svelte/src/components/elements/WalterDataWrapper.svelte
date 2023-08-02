<script lang="ts">
    import {
        AccordionItem,
        Loading,
        Modal,
        Tile
    } from 'carbon-components-svelte';

    import { WalterDataTable } from '$walter/components';
    import { handle_save } from './WalterDataWrapper';
    import { goto } from '$app/navigation';
    import { page } from '$app/stores';

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

    let addModalOpen = false;
    let open = false;

    async function submit() {
        if (!addUrl) return;

        const parsed = await handle_save(addUrl, addEntry, title!);

        rows = [...rows, parsed];
        open = true;
    }

    let quick_add = () => {
        addModalOpen = true;
    }

    function normal_add() {
        goto(`${$page.url.pathname}/new`);
    }
</script>

{#if title !== undefined}
    {#await rows}
        <AccordionItem>
            <svelte:fragment slot="title">
                <div
                    style="display: flex; flex-direction: row; margin-left: -1em"
                >
                    <p class="bx--accordion__title" style="width: auto;">
                        {title}
                    </p>
                    <Loading
                        withOverlay={false}
                        small
                        style="margin-left: 1em"
                    />
                </div>
            </svelte:fragment>
        </AccordionItem>
    {:then x}
        <AccordionItem title={`${title} (${x.length})`} bind:open>
             <Tile light>
                <WalterDataTable add={addUrl && addEntry && quick_add} {navigate} bind:rows {headers} />
            </Tile>
            {#if addUrl && addEntry}
                <Modal
                    secondaryButtonText="Abbrechen"
                    primaryButtonText="Bestätigen"
                    on:submit={submit}
                    on:click:button--secondary={() => (addModalOpen = false)}
                    on:click:button--primary={() => (addModalOpen = false)}
                    modalHeading="Eintrag zu {title} hinzufügen"
                    bind:open={addModalOpen}
                >
                    <slot />
                    <!-- Spacer for DatePickers. Otherwise the modal is too narrow -->
                    <Tile style="height: 13em" />
                </Modal>
            {/if}
        </AccordionItem>
    {/await}
{:else}
    <WalterDataTable add={normal_add} {fullHeight} {navigate} {rows} {headers} />
    {/if}
