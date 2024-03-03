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
    import { Button } from 'carbon-components-svelte';
    import type { WalterPreviewCopyTable } from './WalterPreviewCopyFile';
    import { handle_save } from './WalterStep2ViewOrCreateEntry';
    import type { WalterSelectionEntry } from '$walter/lib';

    export let step: number;
    export let fetchImpl: typeof fetch;
    export let entry: unknown;
    export let selectedTable: WalterPreviewCopyTable | undefined;
    export let selectedEntry: WalterSelectionEntry | undefined;
    export let updateRows: () => void;
    export let selectEntryFromId: (id: string) => void;

    async function save() {
        if (selectedTable) {
            var saved_entry = await handle_save(selectedTable.ApiURL, entry);
            updateRows();
            selectEntryFromId(`${saved_entry.id}`);
            setTimeout(() => (step = 3), 0);
        }
    }

    function proceed() {
        setTimeout(() => (step = 3), 0);
    }
</script>

{#if step === 2 && selectedTable}
    {#if !selectedEntry}
        <p>
            Neuen Eintrag zu <b style="font-weight: bold"
                >{selectedTable.value}</b
            > hinzufügen
        </p>
    {/if}
    <svelte:component
        this={selectedTable.newPage()}
        readonly={!!selectedEntry}
        bind:entry
        bind:fetchImpl
    />
    {#if !selectedEntry}
        <Button on:click={save}>Eintrag speichern</Button>
    {:else}
        <Button on:click={proceed}>Weiter</Button>
    {/if}
{/if}
