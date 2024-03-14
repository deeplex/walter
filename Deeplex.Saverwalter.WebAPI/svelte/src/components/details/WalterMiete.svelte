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
    import {
        WalterDatePicker,
        WalterLinks,
        WalterMieten,
        WalterNumberInput,
        WalterTextArea
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type { WalterMieteEntry } from '$walter/lib';
    import WalterLinkTile from '../subdetails/WalterLinkTile.svelte';
    import { fileURL } from '$walter/services/files';

    export let entry: Partial<WalterMieteEntry> = {};
    export let mieten: WalterMieteEntry[] = [];
    export const fetchImpl: typeof fetch | undefined = undefined; // NOTE: Needed to load copy preview fetchImpl...?
    export let readonly = false;
    $: {
        readonly = entry?.permissions?.update === false;
    }
</script>

<Row>
    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.betrag}
        label="Betrag (Warmmiete)"
    />
    <WalterDatePicker
        required
        disabled={readonly}
        bind:value={entry.betreffenderMonat}
        labelText="Betreffender Monat"
    />
    <WalterDatePicker
        required
        disabled={readonly}
        bind:value={entry.zahlungsdatum}
        labelText="Zahlungsdatum"
    />
</Row>
<Row>
    <WalterTextArea {readonly} labelText="Notiz" bind:value={entry.notiz} />
</Row>

<WalterLinks>
    <WalterLinkTile
        fileref={fileURL.vertrag(`${entry.vertrag?.id}`)}
        name={`Vertrag: ${mieten[0]?.vertrag?.text || 'ansehen'}`}
        href={`/vertraege/${entry.vertrag?.id}`}
    />
    {#if entry.vertrag?.id}
        <WalterMieten
            title="Mieten"
            rows={mieten.filter(
                (e) => +e.vertrag.id === +(entry.vertrag?.id || 0)
            )}
        />
    {/if}
</WalterLinks>
