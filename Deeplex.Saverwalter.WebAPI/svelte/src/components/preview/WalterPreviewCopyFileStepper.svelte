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
        on:click={() => (step = 0)}
        label="Tabelle auswählen"
        complete={!!selectedTable}
    />
    <ProgressStep
        on:click={() => {
            if (!!selectedTable) step = 1;
        }}
        label="Eintrag auswählen"
        complete={step > 1}
    />
    <ProgressStep
        on:click={() => {
            if (selectedEntry) step = 2;
        }}
        label="Bestätigen"
        complete={step > 2}
    />
</ProgressIndicator>
