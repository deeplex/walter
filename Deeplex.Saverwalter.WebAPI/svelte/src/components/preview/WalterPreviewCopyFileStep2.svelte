<script lang="ts">
    import { Button } from 'carbon-components-svelte';
    import type { WalterPreviewCopyTable } from './WalterPreviewCopyFile';
    import { handle_save } from './WalterPreviewCopyFileStep2';
    import type { WalterSelectionEntry } from '$walter/lib';

    export let step: number;
    export let fetchImpl: typeof fetch;
    export let entry: any;
    export let selectedTable: WalterPreviewCopyTable | undefined;
    export let selectedEntry: WalterSelectionEntry | undefined;
    export let updateRows: () => void;
    export let selectEntryFromId: (id: string) => void;

    async function save() {
        const apiURL = `/api/${selectedTable?.key}`;
        var saved_entry = await handle_save(apiURL, entry);
        await updateRows();
        await selectEntryFromId(`${saved_entry.id}`);
        setTimeout(() => (step = 3), 0);
        entry = undefined;
    }

    function proceed() {
        setTimeout(() => (step = 3), 0);
    }
</script>

{#if step === 2 && selectedTable}
    {#if !selectedEntry}
        <p>
            Neuen Eintrag zu <b style="font-weight: bold"
                >{selectedTable.value}</b
            > hinzufügen
        </p>
    {/if}
    <svelte:component
        this={selectedTable.newPage()}
        readonly={!!selectedEntry}
        bind:entry
        bind:fetchImpl
    />
    {#if !selectedEntry}
        <Button on:click={save}>Eintrag speichern</Button>
    {:else}
        <Button on:click={proceed}>Weiter</Button>
    {/if}
{/if}
