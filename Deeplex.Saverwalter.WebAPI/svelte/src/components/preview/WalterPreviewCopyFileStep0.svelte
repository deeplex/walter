<script lang="ts">
    import { RadioButton, RadioButtonGroup } from 'carbon-components-svelte';
    import type { WalterPreviewCopyTable } from './WalterPreviewCopyFile';

    export let step: number;
    export let tables: WalterPreviewCopyTable[];
    export let selectedTable: WalterPreviewCopyTable | undefined;
    export let selectedTable_change: (e: any) => Promise<void>;
</script>

{#if step === 0}
    <RadioButtonGroup orientation="vertical">
        {#each tables as radio}
            <RadioButton
                checked={selectedTable?.key == radio.key}
                on:change={selectedTable_change}
                labelText={radio.value}
                value={radio.key}
            />
        {/each}
    </RadioButtonGroup>
{/if}
