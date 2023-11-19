<script lang="ts">
    import { RadioTile, TileGroup } from 'carbon-components-svelte';
    import type { WalterPreviewCopyTable } from './WalterPreviewCopyFile';

    export let step: number;
    export let tables: WalterPreviewCopyTable[];
    export let selectedTable: WalterPreviewCopyTable | undefined;
    export let selectedTable_change: (e: Event) => Promise<void>;
</script>

{#if step === 0}
    <TileGroup>
        {#each tables as radio}
            <RadioTile
                checked={selectedTable?.key == radio.key}
                on:change={selectedTable_change}
                value={radio.key}
                >{radio.value}
            </RadioTile>
        {/each}
    </TileGroup>
{/if}
