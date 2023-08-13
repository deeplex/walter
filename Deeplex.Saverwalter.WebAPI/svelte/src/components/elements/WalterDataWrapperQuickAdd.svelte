<script lang="ts">

    import { Modal, Tile } from "carbon-components-svelte";
    import { handle_save } from "./WalterDataWrapper";
    export let addModalOpen = false;
    export let addUrl: string;
    export let addEntry: any;
    export let title: string;
    export let rows: any[] | undefined = undefined;

    async function submit() {
        if (!addUrl)
        {
            return;
        }

        const parsed = await handle_save(addUrl, addEntry, title!);

        if (rows)
        {
            rows = [...rows, parsed];
        }
    }
</script>
<Modal
    secondaryButtonText="Abbrechen"
    primaryButtonText="Bestätigen"
    on:submit={submit}
    on:click:button--secondary={() => (addModalOpen = false)}
    on:click:button--primary={() => (addModalOpen = false)}
    modalHeading="Eintrag zu {title} hinzufügen"
    bind:open={addModalOpen}
    primaryButtonDisabled={!addUrl}
    >
    <slot />
    <!-- Spacer for DatePickers. Otherwise the modal is too narrow -->
    <Tile style="height: 13em" />
</Modal>
