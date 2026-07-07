<!-- Copyright (C) 2023-2026  Kai Lawrence -->
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
        WalterComboBox,
        WalterMultiSelect,
        WalterDatePicker
    } from '$walter/components';
    import WalterSlider from '$walter/components/elements/WalterSlider.svelte';
    import { Row } from 'carbon-components-svelte';
    import type { WalterHKVOEntry, WalterSelectionEntry } from '$walter/lib';
    import { walter_selection } from '$walter/services/requests';

    export let entry: Partial<WalterHKVOEntry> = {};
    export let fetchImpl: typeof fetch | undefined = undefined;
    $: readonly = entry?.permissions?.update === false;

    const selectableUmlagen = walter_selection.umlagen(fetchImpl!);
    // Nur nachladen, wenn sich die Umlage ändert — NICHT bei jeder Auswahländerung.
    // (Sonst invalidiert die MultiSelect via bind:value das entry, das reaktive
    //  Statement fetcht neu, die MultiSelect feuert erneut → Endlosschleife.)
    let selectableWaermezaehler: Promise<WalterSelectionEntry[]> = Promise.resolve([]);
    let letzteUmlageId: number | undefined;
    $: if (entry.umlageId !== letzteUmlageId) {
        letzteUmlageId = entry.umlageId;
        selectableWaermezaehler = entry.umlageId !== undefined
            ? walter_selection.waermezaehler(entry.umlageId, fetchImpl!)
            : Promise.resolve([]);
    }
    const hkvo_p9a2 = walter_selection.hkvo_p9a2(fetchImpl!);
</script>

<Row>
    <WalterDatePicker
        required
        disabled={readonly}
        bind:value={entry.beginn}
        labelText="Beginn"
    />
</Row>
{#await hkvo_p9a2 then p9a2}
    <Row>
        <WalterComboBox
            required
            {readonly}
            entries={selectableUmlagen}
            bind:value={entry.stromrechnung}
            titleText="Stromrechnung (Betriebsstrom)"
        />
        <WalterMultiSelect
            disabled={readonly}
            entries={selectableWaermezaehler}
            bind:value={entry.allgemeinWaerme}
            titleText="AllgemeinWärme-Zähler für Q (§9 Abs. 2)"
        />
    </Row>
    <Row>
        <WalterSlider
            disabled={readonly}
            step={5}
            min={50}
            max={70}
            fullWidth
            labelText="HKVO §7 (in %)"
            bind:value={entry.hkvO_P7}
        />
        <WalterSlider
            disabled={readonly}
            step={5}
            min={50}
            max={70}
            fullWidth
            labelText="HKVO §8 (in %)"
            bind:value={entry.hkvO_P8}
        />
    </Row>
    <Row>
        <WalterSlider
            disabled={readonly}
            step={1}
            min={0}
            max={10}
            fullWidth
            labelText="Strompauschale (in %)"
            bind:value={entry.strompauschale}
        />
        <WalterComboBox
            required
            {readonly}
            entries={Promise.resolve(p9a2)}
            bind:value={entry.hkvO_P9}
            titleText="HKVO §9"
        />
    </Row>
{/await}
