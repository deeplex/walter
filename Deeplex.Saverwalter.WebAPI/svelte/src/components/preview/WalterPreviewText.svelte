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
    import { WalterPreviewError } from '$walter/components';
    import type { WalterFile } from '$walter/types';
    import { Tile } from 'carbon-components-svelte';
    import { onMount } from 'svelte';

    export let file: WalterFile;
    let text = '';

    onMount(() => {
        const reader = new FileReader();
        reader.onload = function (event) {
            text = (event.target?.result as string) || '';
        };
        if (file.blob) {
            reader.readAsText(file.blob);
        }
    });
</script>

{#if file.blob}
    <Tile light>{text}</Tile>
{:else}
    <WalterPreviewError {file} />
{/if}
