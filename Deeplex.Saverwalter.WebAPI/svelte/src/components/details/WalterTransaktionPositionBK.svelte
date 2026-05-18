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
        WalterDatePicker,
        WalterNumberInput
    } from '$walter/components';
    import { walter_selection } from '$walter/services/requests';
    import type { WalterSelectionEntry } from '$walter/lib';
    import type { BetriebskostenEingangInput } from '$walter/lib';

    export let fetchImpl: typeof fetch;
    export let bk: BetriebskostenEingangInput;

    let umlage: WalterSelectionEntry | undefined = undefined;

    const umlagen = walter_selection.umlagen(fetchImpl);
    const currentYear = new Date().getFullYear();

    if (!bk.betreffendesJahr) {
        bk.betreffendesJahr = currentYear - 1;
    }

    function onUmlageSelect(e: CustomEvent) {
        umlage = e.detail?.selectedItem;
        bk.umlageId = umlage?.id as number | undefined;
    }
</script>

<Row>
    <Column>
        <WalterComboBox
            required
            titleText="Umlage"
            entries={umlagen}
            bind:value={umlage}
            initialId={bk.umlageId}
            on:select={onUmlageSelect}
        />
    </Column>
    <Column>
        <WalterNumberInput
            required
            label="Betreffendes Jahr"
            digits={0}
            bind:value={bk.betreffendesJahr}
        />
    </Column>
</Row>

<Row>
    <Column>
        <WalterDatePicker
            required
            labelText="Rechnungsdatum"
            bind:value={bk.rechnungsDatum}
        />
    </Column>
    <Column>
        <WalterNumberInput
            required
            label="Betrag (€)"
            bind:value={bk.betrag}
        />
    </Column>
</Row>
