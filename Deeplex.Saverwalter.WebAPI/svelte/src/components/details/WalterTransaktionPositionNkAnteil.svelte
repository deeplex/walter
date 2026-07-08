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
    import { WalterComboBox, WalterNumberInput } from '$walter/components';
    import { walter_selection } from '$walter/services/requests';
    import type { NkAnteilEingangInput } from '$walter/lib';

    export let fetchImpl: typeof fetch;
    export let nk: NkAnteilEingangInput;
    export let invalid = false;

    import type { WalterSelectionEntry } from '$walter/lib';
    let umlage: WalterSelectionEntry | undefined = undefined;
    let vertrag: WalterSelectionEntry | undefined = undefined;

    const umlagen = walter_selection.umlagen(fetchImpl);
    const vertraege = walter_selection.vertraege(fetchImpl);

    $: invalid = !nk.umlageId || !nk.vertragId || nk.betrag <= 0;

    function onUmlageSelect(e: CustomEvent) {
        umlage = e.detail?.selectedItem;
        nk.umlageId = umlage?.id != null ? +umlage.id : undefined;
    }

    function onVertragSelect(e: CustomEvent) {
        vertrag = e.detail?.selectedItem;
        nk.vertragId = vertrag?.id != null ? +vertrag.id : undefined;
    }
</script>

<Row>
    <Column>
        <WalterComboBox
            required
            titleText="Umlage"
            entries={umlagen}
            bind:value={umlage}
            initialId={nk.umlageId}
            on:select={onUmlageSelect}
        />
    </Column>
    <Column>
        <WalterComboBox
            required
            titleText="Vertrag"
            entries={vertraege}
            bind:value={vertrag}
            initialId={nk.vertragId}
            on:select={onVertragSelect}
        />
    </Column>
</Row>
<Row>
    <Column>
        <WalterNumberInput
            required
            label="Betreffendes Jahr"
            digits={0}
            bind:value={nk.betreffendesJahr}
        />
    </Column>
    <Column>
        <WalterNumberInput required label="Betrag (€)" bind:value={nk.betrag} />
    </Column>
</Row>
