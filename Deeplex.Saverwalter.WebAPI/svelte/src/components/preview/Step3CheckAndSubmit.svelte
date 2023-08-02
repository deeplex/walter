<script lang="ts">
    import { Link, Tile } from 'carbon-components-svelte';
    import type { WalterPreviewCopyTable } from './WalterPreviewCopyFile';
    import { page } from '$app/stores';
    import type { WalterSelectionEntry } from '$walter/lib';

    export let step: number;
    export let selectedEntry: WalterSelectionEntry | undefined = undefined;
    export let selectedTable: WalterPreviewCopyTable | undefined;
</script>

{#if step === 3}
    <Tile light>
        {#if selectedTable?.key === 'stack'}
            <Link href="{$page.url.origin}/{selectedTable?.key}">
                {selectedTable?.value}
            </Link>
            ausgewählt.
        {:else}
            Ausgewählter Eintrag von {selectedTable?.value}:
            <Link href="{$page.url.origin}/{selectedTable?.key}/{selectedEntry?.id}">
                {selectedEntry?.text}
            </Link>
        {/if}
    </Tile>
{/if}
