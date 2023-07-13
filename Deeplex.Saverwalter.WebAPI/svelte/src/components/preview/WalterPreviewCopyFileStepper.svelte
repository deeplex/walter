<script lang="ts">
    import type { WalterSelectionEntry } from '$walter/lib';
    import {
        ProgressIndicator,
        ProgressStep,
        Tile
    } from 'carbon-components-svelte';
    import type { WalterPreviewCopyTable } from './WalterPreviewCopyFile';

    export let step: number;
    export let selectedEntry: WalterSelectionEntry | undefined = undefined;
    export let selectedTable: WalterPreviewCopyTable | undefined;

    function tableClick() {
        selectedTable = undefined;
        selectedEntry = undefined;
        step = 0;
    }

    function entryClick() {
        selectedEntry = undefined;
        if (!!selectedTable) {
            step = 1;
        }
    }
</script>

<Tile light style="padding-top: 1em; padding-bottom: 0.5em">
    <ProgressIndicator style="margin: 1em" spaceEqually currentIndex={step}>
        <ProgressStep
            label="Tabelle auswählen"
            on:click={tableClick}
            complete={step > 0}
        />
        <ProgressStep
            label="Eintrag auswählen"
            on:click={entryClick}
            complete={step > 1}
            disabled={!selectedTable}
        />
        <ProgressStep
            label="Vorschau"
            on:click={() => (step = 2)}
            disabled={!selectedEntry}
            complete={step > 2}
        />
        <ProgressStep
            label="Bestätigen"
            on:click={() => {
                if (selectedEntry) step = 3;
            }}
            disabled={!selectedEntry}
            complete={step > 3}
        />
    </ProgressIndicator>
</Tile>
