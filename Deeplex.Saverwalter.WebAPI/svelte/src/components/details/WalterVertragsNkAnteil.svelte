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
        WalterNumberInput,
        WalterDatePicker,
        WalterTextArea
    } from '$walter/components';
    import { walter_selection } from '$walter/services/requests';
    import type { WalterVertragsNkAnteilEntry } from '$walter/lib';

    export let entry: Partial<WalterVertragsNkAnteilEntry> = {};
    export let fetchImpl: typeof fetch;

    const umlagen = walter_selection.umlagen(fetchImpl);
    const vertraege = walter_selection.vertraege(fetchImpl);
</script>

<Row>
    <Column>
        <WalterComboBox
            required
            titleText="Umlage"
            entries={umlagen}
            bind:value={entry.umlage}
            initialId={entry.umlage?.id}
            on:select={(e) => {
                entry.umlage = e.detail?.selectedItem;
            }}
        />
    </Column>
    <Column>
        <WalterComboBox
            required
            titleText="Vertrag"
            entries={vertraege}
            bind:value={entry.vertrag}
            initialId={entry.vertrag?.id}
            on:select={(e) => {
                entry.vertrag = e.detail?.selectedItem;
            }}
        />
    </Column>
</Row>
<Row>
    <Column>
        <WalterNumberInput
            required
            label="Betreffendes Jahr"
            digits={0}
            bind:value={entry.betreffendesJahr}
        />
    </Column>
    <Column>
        <WalterNumberInput
            required
            label="Betrag (€)"
            bind:value={entry.betrag}
        />
    </Column>
</Row>
<Row>
    <Column>
        <WalterDatePicker labelText="Datum" bind:value={entry.datum} />
    </Column>
</Row>
