<script lang="ts">
    import {
        AccordionItem,
        Button,
        Loading,
        Modal,
        Tile
    } from 'carbon-components-svelte';

    import { WalterDataTable } from '$walter/components';
    import { Add } from 'carbon-icons-svelte';
    import { walter_post } from '$walter/services/requests';
    import { WalterToastContent } from '../../lib/WalterToastContent';
    import { addToast } from '$walter/store';
    import { handle_save } from './WalterDataWrapper';

    export let fullHeight = false;
    export let addUrl: string | undefined = undefined;
    export let addEntry: any | undefined = undefined;
    export let title: string | undefined = undefined;
    export let rows: any[];
    export let headers: {
        key: string;
        value: string;
    }[];
    export let search = false;
    export let navigate: (e: CustomEvent) => Promise<void> | void = (
        _e: unknown
    ) => {};

    let addModalOpen = false;
    let open = false;

    function submit() {
        if (!addUrl) return;

        const parsed = handle_save(addUrl, addEntry, title!);

        rows = [...rows, parsed];
        open = true;
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
            {#if x.length}
                <WalterDataTable {search} {navigate} bind:rows {headers} />
            {/if}
            {#if addUrl && addEntry}
                <div style="float: right">
                    <Button
                        on:click={() => (addModalOpen = true)}
                        iconDescription="Eintrag hinzufügen"
                        icon={Add}>Eintrag hinzufügen</Button
                    >
                </div>
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
    <WalterDataTable {fullHeight} {search} {navigate} {rows} {headers} />
{/if}
