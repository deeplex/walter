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
        WalterNumberInput,
        WalterTextArea
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import { WalterZaehlerEntry, WalterZaehlerstandEntry } from '$walter/lib';
    import WalterTextInput from '../elements/WalterTextInput.svelte';
    import { onMount } from 'svelte';
    import { convertDateGerman } from '$walter/services/utils';

    export let entry: Partial<WalterZaehlerstandEntry> = {};
    export let fetchImpl: typeof fetch | undefined = undefined;
    export let readonly = false;
    $: {
        readonly = entry?.permissions?.update === false;
    }

    let maxDate: string | undefined;
    onMount(async () => {
        if (entry.zaehler?.id && fetchImpl) {
            const date = await WalterZaehlerEntry.GetOne<WalterZaehlerEntry>(
                `${entry.zaehler?.id}`,
                fetchImpl
            ).then((zaehler) => zaehler.ende);
            maxDate = convertDateGerman(new Date(date));
        }
    });
</script>

<Row>
    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.stand}
        label="Zählerstand"
    />
    <WalterDatePicker
        required
        disabled={readonly}
        bind:value={entry.datum}
        labelText="Ablesedatum"
    />
    <WalterTextInput
        required
        readonly
        bind:value={entry.einheit}
        labelText="Einheit"
    />
</Row>
<Row>
    <WalterTextArea {readonly} labelText="Notiz" bind:value={entry.notiz} />
</Row>
