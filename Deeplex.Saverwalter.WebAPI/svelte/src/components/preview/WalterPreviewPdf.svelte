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
    import { onDestroy, onMount } from 'svelte';
    import PDFObject from 'pdfobject';
    import type { WalterFile } from '$walter/types';
    import { WalterPreviewError } from '$walter/components';

    export let file: WalterFile;

    let src: string;
    onDestroy(() => {
        URL.revokeObjectURL(src);
    });

    onMount(() => {
        if (file.blob) {
            src = URL.createObjectURL(file.blob);
            PDFObject.embed(src, '#pdf-container');
        }
    });
</script>

{#if file.blob}
    <div style="height:50rem; overflow: hidden" id="pdf-container" />
{:else}
    <WalterPreviewError {file} />
{/if}
