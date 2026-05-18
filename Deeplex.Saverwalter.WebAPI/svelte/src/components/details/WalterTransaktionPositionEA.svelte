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
    import { Row, Column } from 'carbon-components-svelte';
    import {
        WalterComboBox,
        WalterComboBoxWohnung,
        WalterNumberInput,
        WalterTextInput
    } from '$walter/components';
    import { walter_selection } from '$walter/services/requests';
    import type { WalterSelectionEntry } from '$walter/lib';
    import type { ErhaltungsaufwendungsInput } from '$walter/lib';

    export let fetchImpl: typeof fetch;
    export let ea: ErhaltungsaufwendungsInput;

    let wohnung: WalterSelectionEntry | undefined = undefined;
    let habenKonto: WalterSelectionEntry | undefined = undefined;

    const buchungskonten = walter_selection.buchungskonten(fetchImpl);

    function onWohnungSelect(woh: WalterSelectionEntry | undefined) {
        ea.wohnungId = woh?.id as number | undefined;
    }

    function onHabenKontoSelect(e: CustomEvent) {
        habenKonto = e.detail?.selectedItem;
        ea.habenKontoId = habenKonto?.id as number | undefined;
    }

    $: onWohnungSelect(wohnung);
</script>

<Row>
    <Column>
        <WalterComboBoxWohnung
            required
            {fetchImpl}
            bind:value={wohnung}
        />
    </Column>
    <Column>
        <WalterComboBox
            required
            titleText="Haben-Konto (wer hat gezahlt)"
            entries={buchungskonten}
            bind:value={habenKonto}
            initialId={ea.habenKontoId}
            on:select={onHabenKontoSelect}
        />
    </Column>
</Row>

<Row>
    <Column>
        <WalterNumberInput
            required
            label="Betrag (€)"
            bind:value={ea.betrag}
        />
    </Column>
    <Column>
        <WalterTextInput
            labelText="Beschreibung"
            bind:value={ea.beschreibung}
        />
    </Column>
</Row>
