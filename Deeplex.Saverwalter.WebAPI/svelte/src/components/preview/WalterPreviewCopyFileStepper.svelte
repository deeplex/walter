<script lang="ts">
    import type { WalterSelectionEntry } from '$walter/lib';
    import { ProgressIndicator, ProgressStep } from 'carbon-components-svelte';
    import type { WalterPreviewCopyTable } from './WalterPreviewCopyFile';

    export let step: number;
    export let selectedEntry: WalterSelectionEntry | undefined = undefined;
    export let selectedTable: WalterPreviewCopyTable | undefined;
</script>

<ProgressIndicator style="margin: 1em" spaceEqually currentIndex={step}>
    <ProgressStep
        label="Tabelle auswählen"
        on:click={() => (step = 0)}
        complete={!!selectedTable}
    />
    <ProgressStep
        label="Eintrag auswählen"
        on:click={() => {
            if (!!selectedTable) step = 1;
        }}
        complete={step > 1}
    />
    <ProgressStep
        label="Vorschau"
        on:click={() => {
            step = 2;
        }}
        complete={step > 2}
    />
    <ProgressStep
        label="Bestätigen"
        on:click={() => {
            if (selectedEntry) step = 3;
        }}
        complete={step > 3}
    />
</ProgressIndicator>
