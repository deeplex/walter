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
        WalterDataWrapper,
        WalterMiete,
        WalterNumberInput
    } from '$walter/components';
    import { WalterMieteEntry } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';

    const headers = [
        { key: 'betreffenderMonat', value: 'Betreffender Monat' },
        { key: 'zahlungsdatum', value: 'Zahlungsdatum' },
        { key: 'betrag', value: 'Betrag' }
    ];

    export let rows: WalterMieteEntry[];
    const sortedRows = rows.sort((a, b) =>
        b.betreffenderMonat.localeCompare(a.betreffenderMonat)
    );
    export let fullHeight = false;
    export let title: string | undefined = undefined;

    const on_click_row = (e: CustomEvent) => navigation.miete(e.detail.id);

    export let entry: Partial<WalterMieteEntry> | undefined = undefined;
</script>

<WalterDataWrapper
    addUrl={WalterMieteEntry.ApiURL}
    {on_click_row}
    addEntry={entry}
    {title}
    rows={sortedRows}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterMiete {entry} />
        <WalterNumberInput
            label="Auch anwenden auf die nächsten Monate:"
            min={0}
            max={11}
            hideSteppers={false}
            bind:value={entry.repeat}
        />
    {/if}
</WalterDataWrapper>
