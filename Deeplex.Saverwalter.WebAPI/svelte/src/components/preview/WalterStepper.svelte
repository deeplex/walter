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
    import type { WalterSelectionEntry } from '$walter/lib';
    import {
        ProgressIndicator,
        ProgressStep,
        Tile
    } from 'carbon-components-svelte';
    import type { WalterPreviewCopyTable } from './WalterPreviewCopyFile';

    export let step: number;
    export let selectedEntry: WalterSelectionEntry | undefined = undefined;
    export let selectedTable: WalterPreviewCopyTable | undefined;

    function tableClick() {
        selectedTable = undefined;
        selectedEntry = undefined;
        step = 0;
    }

    function entryClick() {
        selectedEntry = undefined;
        if (selectedTable) {
            step = 1;
        }
    }
</script>

<Tile light style="padding-top: 1em; padding-bottom: 0.5em">
    <ProgressIndicator style="margin: 1em" spaceEqually currentIndex={step}>
        <ProgressStep
            label={selectedTable?.value || 'Tabelle auswählen'}
            on:click={tableClick}
            complete={step > 0}
        />
        <ProgressStep
            label="Eintrag auswählen"
            on:click={entryClick}
            complete={step > 1}
            disabled={!selectedTable || selectedTable.key === 'stack'}
        />
        <ProgressStep
            label="Vorschau"
            on:click={() => (step = 2)}
            disabled={!selectedEntry}
            complete={step > 2}
        />
        <ProgressStep
            label="Bestätigen"
            on:click={() => {
                if (selectedEntry) step = 3;
            }}
            disabled={!selectedEntry}
            complete={step > 3}
        />
    </ProgressIndicator>
</Tile>
