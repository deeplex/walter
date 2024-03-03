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
    import {
        HeaderGlobalAction,
        HeaderUtilities
    } from 'carbon-components-svelte';
    import { Save } from 'carbon-icons-svelte';

    import { WalterHeader } from '$walter/components';
    import { handle_save } from './WalterHeaderNew';

    export let title = 'Neu...';
    export let apiURL: string;
    export let entry: unknown;

    function click_post(): void {
        handle_save(apiURL, entry);
    }
</script>

<WalterHeader {title}>
    <slot />
    <HeaderUtilities>
        <HeaderGlobalAction on:click={click_post} icon={Save} />
    </HeaderUtilities>
</WalterHeader>
