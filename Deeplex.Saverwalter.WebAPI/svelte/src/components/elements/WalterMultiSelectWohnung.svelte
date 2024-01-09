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
    import { WalterWohnungEntry, type WalterSelectionEntry } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';
    import WalterQuickAddButton from './WalterQuickAddButton.svelte';
    import WalterMultiSelect from './WalterMultiSelect.svelte';
    import WalterWohnung from '../details/WalterWohnung.svelte';

    export let value: WalterSelectionEntry[] | undefined;
    export let titleText: string;
    export let fetchImpl: typeof fetch;
    export let readonly: boolean = false;

    let entries = walter_selection.wohnungen(fetchImpl);
    function onSubmit() {
        entries = walter_selection.wohnungen(fetchImpl);
    }

    let addEntry = {};
</script>

<div
    style="
    flex: 1 1 auto !important;
    display: flex !important;
    flex-wrap: wrap !important"
>
    <WalterMultiSelect disabled={readonly} bind:value {titleText} {entries} />

    {#if !readonly}
        <WalterQuickAddButton
            title="Wohnungen"
            bind:addEntry
            addUrl={WalterWohnungEntry.ApiURL}
            {onSubmit}
        >
            <WalterWohnung entry={addEntry} {fetchImpl} />
        </WalterQuickAddButton>
    {/if}
</div>
