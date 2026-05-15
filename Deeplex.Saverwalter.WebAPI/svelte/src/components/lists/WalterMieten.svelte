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
    import { Accordion, AccordionItem, Tile } from 'carbon-components-svelte';
    import { WalterDataTable } from '$walter/components';
    import WalterDataWrapperQuickAdd from '../elements/WalterDataWrapperQuickAdd.svelte';
    import WalterBuchung from '../details/WalterBuchung.svelte';
    import type { WalterMietzahlungListEntry, WalterVertragEntry, TransaktionsInput } from '$walter/lib';
    import { emptyTransaktionsInput } from '$walter/lib';
    import { invalidateAll } from '$app/navigation';

    const headers = [
        { key: 'betreffenderMonat', value: 'Betreffender Monat' },
        { key: 'buchungsdatum', value: 'Buchungsdatum' },
        { key: 'kaltmieteZahlung', value: 'Kaltmiete' }
    ];

    export let rows: WalterMietzahlungListEntry[];
    $: sortedRows = [...(rows || [])].sort((a, b) => {
        const monthA = new Date(a.betreffenderMonat).getTime();
        const monthB = new Date(b.betreffenderMonat).getTime();
        if (!Number.isNaN(monthA) && !Number.isNaN(monthB)) return monthB - monthA;
        return `${b.betreffenderMonat || ''}`.localeCompare(`${a.betreffenderMonat || ''}`);
    });

    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let vertrag: WalterVertragEntry | undefined = undefined;
    export let fetchImpl: typeof fetch;

    let modalOpen = false;
    let buchungsInput: TransaktionsInput = emptyTransaktionsInput();

    $: if (modalOpen) {
        buchungsInput = {
            ...emptyTransaktionsInput(),
            mieten: [{ kaltmiete: 0, nkVorauszahlung: 0, vertragId: vertrag?.id as number | undefined }]
        };
    }

    async function onSubmit() {
        await invalidateAll();
    }
</script>

<WalterDataWrapperQuickAdd
    title="Mietzahlung"
    addUrl="/api/transaktionen/buchen"
    bind:addEntry={buchungsInput}
    bind:addModalOpen={modalOpen}
    onSubmit={onSubmit}
>
    <WalterBuchung {fetchImpl} bind:buchung={buchungsInput} />
</WalterDataWrapperQuickAdd>

{#if title !== undefined}
    <Accordion>
        <AccordionItem title={`${title} (${(rows || []).length})`}>
            <Tile style="overflow: auto">
                <WalterDataTable
                    add={() => (modalOpen = true)}
                    rows={sortedRows}
                    {headers}
                />
            </Tile>
        </AccordionItem>
    </Accordion>
{:else}
    <WalterDataTable
        add={() => (modalOpen = true)}
        {fullHeight}
        rows={sortedRows}
        {headers}
    />
{/if}
