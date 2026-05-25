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
    import { WalterTextArea } from '$walter/components';
    import { Row, Select, SelectItem } from 'carbon-components-svelte';
    import WalterTextInput from '../elements/WalterTextInput.svelte';
    import type { WalterUmlagetypEntry } from '$walter/lib';
    import { BETRK_V_NUMMERN } from '$walter/lib';

    export let entry: Partial<WalterUmlagetypEntry> = {};
    export const fetchImpl: typeof fetch | undefined = undefined; // NOTE: Needed to load copy preview fetchImpl...?
    export let readonly = false;
    $: {
        readonly = entry?.permissions?.update === false;
    }

    function onBetrKVChange(e: Event) {
        const val = (e.target as HTMLSelectElement).value;
        entry.betrKVNummer = val ? parseInt(val) : null;
    }
</script>

<Row>
    <WalterTextInput
        labelText="Bezeichnung"
        required
        bind:value={entry.bezeichnung}
    />
</Row>

<Row>
    <Select
        labelText="BetrKV §2 Kategorie"
        disabled={readonly}
        selected={entry.betrKVNummer != null ? String(entry.betrKVNummer) : ''}
        on:change={onBetrKVChange}
    >
        <SelectItem value="" text="— nicht klassifiziert —" />
        {#each BETRK_V_NUMMERN as opt}
            <SelectItem value={String(opt.id)} text={opt.text} />
        {/each}
    </Select>
</Row>

<Row>
    <WalterTextArea {readonly} labelText="Notiz" bind:value={entry.notiz} />
</Row>
