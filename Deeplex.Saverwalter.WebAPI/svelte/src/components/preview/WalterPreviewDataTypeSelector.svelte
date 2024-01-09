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
    import type { WalterFile } from '$walter/types';
    import { WalterPreviewPdf, WalterPreviewUnknown } from '..';

    import WalterPreviewImage from './WalterPreviewImage.svelte';
    import WalterPreviewText from './WalterPreviewText.svelte';

    export let file: WalterFile;
</script>

{#if file.type?.includes('image/')}
    <WalterPreviewImage bind:file />
{:else if file.type === 'text/plain' || file.type === 'application/json'}
    <WalterPreviewText bind:file />
{:else if file.type === 'application/pdf'}
    <WalterPreviewPdf bind:file />
{:else}
    <WalterPreviewUnknown bind:file />
{/if}
