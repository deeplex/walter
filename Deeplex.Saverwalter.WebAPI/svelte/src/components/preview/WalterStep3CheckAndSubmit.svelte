<!-- Copyright (C) 2023-2024  Kai Lawrence -->
<!--
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
-->

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
