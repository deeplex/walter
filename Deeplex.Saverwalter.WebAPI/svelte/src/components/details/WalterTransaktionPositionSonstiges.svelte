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
    import { WalterNumberInput, WalterTextInput } from '$walter/components';
    import type { SonstigerBuchungssatzInput } from '$walter/lib';

    export let sonstiger: SonstigerBuchungssatzInput;
    export let availableBetrag = 0;
    export let isSinglePosition = false;

    $: if (isSinglePosition && availableBetrag > 0) {
        sonstiger.betrag = availableBetrag;
    }
</script>

<Row>
    <Column>
        <WalterNumberInput
            required
            label="Betrag (€)"
            bind:value={sonstiger.betrag}
        />
    </Column>
    <Column>
        <WalterTextInput
            labelText="Beschreibung"
            bind:value={sonstiger.beschreibung}
        />
    </Column>
</Row>
