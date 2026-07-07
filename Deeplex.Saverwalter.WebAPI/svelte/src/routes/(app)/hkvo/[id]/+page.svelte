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
    import type { PageData } from './$types';
    import {
        WalterGrid,
        WalterHeaderDetail,
        WalterLinkTile
    } from '$walter/components';
    import WalterSlider from '$walter/components/elements/WalterSlider.svelte';
    import WalterComboBox from '$walter/components/elements/WalterComboBox.svelte';
    import WalterMultiSelect from '$walter/components/elements/WalterMultiSelect.svelte';
    import WalterDatePicker from '$walter/components/elements/WalterDatePicker.svelte';
    import { Row } from 'carbon-components-svelte';
    import { walter_selection } from '$walter/services/requests';
    import { changeTracker } from '$walter/store';
    import { fileURL } from '$walter/services/files';
    import { WalterFileWrapper } from '$walter/lib';

    export let data: PageData;

    $: entry = data.entry;

    const selectableUmlagen = walter_selection.umlagen(data.fetchImpl);
    // Einmalig laden - die Zaehler-Auswahl haengt nicht von der aktuellen Auswahl ab.
    // (Reaktiv auf entry wuerde bei jeder Auswahlaenderung neu fetchen - Endlosschleife.)
    const selectableWaermezaehler =
        data.entry.umlageId !== undefined
            ? walter_selection.waermezaehler(
                  data.entry.umlageId,
                  data.fetchImpl
              )
            : Promise.resolve([]);
    const hkvo_p9a2 = walter_selection.hkvo_p9a2(data.fetchImpl);

    $: title = `HKVO ab ${entry.beginn}`;

    let fileWrapper = new WalterFileWrapper(data.fetchImpl);
    fileWrapper.registerStack();

    $: submitDisabled = $changeTracker === 0;
</script>

<WalterHeaderDetail
    {entry}
    apiURL={data.apiURL}
    {title}
    bind:fileWrapper
    disabled={submitDisabled}
/>

<WalterGrid>
    <div>
        {#await hkvo_p9a2 then p9a2}
            <Row>
                <WalterDatePicker
                    required
                    disabled={entry.permissions?.update === false}
                    bind:value={entry.beginn}
                    labelText="Beginn"
                />
            </Row>
            <Row>
                <WalterComboBox
                    bind:value={entry.stromrechnung}
                    readonly={entry.permissions?.update === false}
                    required
                    entries={selectableUmlagen}
                    titleText="Stromrechnung (Betriebsstrom)"
                />
                <WalterMultiSelect
                    bind:value={entry.allgemeinWaerme}
                    disabled={entry.permissions?.update === false}
                    entries={selectableWaermezaehler}
                    titleText="AllgemeinWärme-Zähler (§9 Abs. 2)"
                />
            </Row>
            <Row>
                <WalterSlider
                    disabled={entry.permissions?.update === false}
                    step={5}
                    min={50}
                    max={70}
                    fullWidth
                    labelText="HKVO §7 (in %)"
                    bind:value={entry.hkvO_P7}
                />
                <WalterSlider
                    disabled={entry.permissions?.update === false}
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
                    disabled={entry.permissions?.update === false}
                    step={1}
                    min={0}
                    max={10}
                    fullWidth
                    labelText="Strompauschale (in %)"
                    bind:value={entry.strompauschale}
                />
                <WalterComboBox
                    required
                    readonly={entry.permissions?.update === false}
                    entries={Promise.resolve(p9a2)}
                    bind:value={entry.hkvO_P9}
                    titleText="HKVO §9"
                />
            </Row>
        {/await}
    </div>

    {#await data.umlage then umlage}
        <WalterLinkTile
            bind:fileWrapper
            fileref={fileURL.umlage(`${umlage.id}`)}
            name={`Umlage: ${umlage.typ?.text} – ${umlage.wohnungenBezeichnung}`}
            href={`/umlagen/${umlage.id}`}
        />
    {/await}
</WalterGrid>
