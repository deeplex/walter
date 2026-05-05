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
    import { WalterDataWrapper, WalterMiete } from '$walter/components';
    import {
        WalterMietzahlungApiURL,
        WalterMietzahlungListEntry,
        type WalterMietzahlungInput,
        type WalterVertragEntry
    } from '$walter/lib';

    const headers = [
        { key: 'betreffenderMonat', value: 'Betreffender Monat' },
        { key: 'buchungsdatum', value: 'Buchungsdatum' },
        { key: 'kaltmieteZahlung', value: 'Kaltmiete' }
    ];

    export let rows: WalterMietzahlungListEntry[];
    $: sortedRows = [...(rows || [])].sort((a, b) => {
        const monthA = new Date(a.betreffenderMonat).getTime();
        const monthB = new Date(b.betreffenderMonat).getTime();

        if (!Number.isNaN(monthA) && !Number.isNaN(monthB)) {
            return monthB - monthA;
        }

        return `${b.betreffenderMonat || ''}`.localeCompare(
            `${a.betreffenderMonat || ''}`
        );
    });
    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterMietzahlungInput> | undefined = undefined;
    export let vertrag: WalterVertragEntry | undefined = undefined;
</script>

<WalterDataWrapper
    addUrl={WalterMietzahlungApiURL}
    addEntry={entry}
    {title}
    rows={sortedRows}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterMiete {entry} {vertrag} mietzahlungen={sortedRows} />
    {/if}
</WalterDataWrapper>
