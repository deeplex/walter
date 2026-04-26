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
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import {
        WalterDataWrapper,
        WalterMiete,
        WalterNumberInput
    } from '$walter/components';
    import { WalterMieteEntry } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';
    import { openModal } from '$walter/store';

    const headers = [
        { key: 'betreffenderMonat', value: 'Betreffender Monat' },
        { key: 'zahlungsdatum', value: 'Zahlungsdatum' },
        { key: 'betrag', value: 'Betrag' }
    ];

    export let rows: WalterMieteEntry[];
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

    const on_click_row = (e: CustomEvent) => navigation.miete(e.detail.id);
    const rowHref = (row: DataTableRow) => `/mieten/${row.id}`;

    function dateToMonthKey(value: string | undefined) {
        if (!value) {
            return undefined;
        }

        const parsed = new Date(value);
        if (Number.isNaN(parsed.getTime())) {
            return undefined;
        }

        return `${parsed.getFullYear()}-${`${parsed.getMonth() + 1}`.padStart(2, '0')}`;
    }

    function confirmDuplicateMonth(entryToCheck: unknown) {
        const miete = entryToCheck as Partial<WalterMieteEntry>;
        const vertragId = +(miete.vertrag?.id || entry?.vertrag?.id || 0);
        const monthKey = dateToMonthKey(miete.betreffenderMonat);
        const currentId = +(miete.id || 0);

        const hasDuplicate =
            !currentId &&
            vertragId > 0 &&
            !!monthKey &&
            rows.some(
                (row) =>
                    +row.vertrag?.id === vertragId &&
                    dateToMonthKey(row.betreffenderMonat) === monthKey
            );

        if (!hasDuplicate) {
            return Promise.resolve(true);
        }

        return new Promise<boolean>((resolve) => {
            openModal({
                modalHeading: 'Miete bereits vorhanden',
                content:
                    'Für diesen Vertrag gibt es im ausgewählten Monat bereits mindestens eine Miete. Möchtest du trotzdem speichern?',
                primaryButtonText: 'Trotzdem speichern',
                submit: async () => {
                    resolve(true);
                    return true;
                },
                cancel: () => resolve(false),
                danger: false
            });
        });
    }

    export let entry: Partial<WalterMieteEntry> | undefined = undefined;
</script>

<WalterDataWrapper
    addUrl={WalterMieteEntry.ApiURL}
    beforeSubmit={confirmDuplicateMonth}
    {on_click_row}
    {rowHref}
    addEntry={entry}
    {title}
    rows={sortedRows}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterMiete entry={entry} mieten={rows} />
        <WalterNumberInput
            label="Auch anwenden auf die nächsten Monate:"
            min={0}
            max={11}
            hideSteppers={false}
            bind:value={entry.repeat}
        />
    {/if}
</WalterDataWrapper>
