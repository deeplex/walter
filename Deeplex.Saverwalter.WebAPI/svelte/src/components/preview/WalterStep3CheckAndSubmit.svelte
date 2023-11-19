<script lang="ts">
    import { Tile } from 'carbon-components-svelte';
    import type { WalterPreviewCopyTable } from './WalterPreviewCopyFile';
    import { page } from '$app/stores';
    import type { WalterSelectionEntry } from '$walter/lib';
    import { WalterLink } from '..';

    export let step: number;
    export let selectedEntry: WalterSelectionEntry | undefined = undefined;
    export let selectedTable: WalterPreviewCopyTable | undefined;
</script>

{#if step === 3}
    <Tile light>
        {#if selectedTable?.key === 'stack'}
            <WalterLink href="{$page.url.origin}/{selectedTable?.key}">
                {selectedTable?.value}
            </WalterLink>
            ausgewählt.
        {:else}
            Ausgewählter Eintrag von {selectedTable?.value}:
            <WalterLink
                href="{$page.url
                    .origin}/{selectedTable?.key}/{selectedEntry?.id}"
            >
                {selectedEntry?.text}
            </WalterLink>
        {/if}
    </Tile>
{/if}
