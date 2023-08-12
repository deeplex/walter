<script lang="ts">
    import { goto } from '$app/navigation';
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import WalterDataWrapperQuickAdd from '../elements/WalterDataWrapperQuickAdd.svelte';
    import { WalterDataWrapper, WalterMiete, WalterVertrag } from '$walter/components';
    import type { WalterMieteEntry, WalterVertragEntry } from '$walter/lib';
    import { convertDateCanadian } from '$walter/services/utils';

    const headers = [
        { key: 'wohnung.text', value: 'Wohnung' },
        { key: 'mieterAuflistung', value: 'Mieter' },
        { key: 'beginn', value: 'Beginn' },
        { key: 'ende', value: 'Ende' },
        { key: 'button', value: 'Miete hinzufügen' }
    ];

    const addUrl = `/api/vertraege/`;

    const navigate = (e: CustomEvent<DataTableRow>) =>
        goto(`/vertraege/${e.detail.id}`);

    export let rows: WalterVertragEntry[];
    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let fetchImpl: typeof fetch;
    export let entry: Partial<WalterVertragEntry> | undefined = undefined;

    let earliest: Date = new Date();
    let quickAddEntry: Partial<WalterMieteEntry> = {};

    function add(e: CustomEvent, vertrag: WalterVertragEntry) {
        e.stopPropagation();

        const dateMiete = vertrag.lastMiete ? new Date(vertrag.lastMiete?.betreffenderMonat) : new Date();
        dateMiete.setDate(dateMiete.getDate() + new Date(dateMiete.getFullYear(), dateMiete.getMonth(), 0).getDate());
        if (dateMiete < earliest)
        {
            dateMiete.setDate(earliest.getDate());
        }
        quickAddEntry = {
            vertrag: vertrag.lastMiete?.vertrag || {id: vertrag.id, text: vertrag.wohnung.text},
            zahlungsdatum: convertDateCanadian(new Date()),
            betrag: vertrag.lastMiete?.betrag,
            betreffenderMonat: convertDateCanadian(dateMiete)
        }

        open = true;
    }
    const rowsAdd = rows.map(row => ({...row, button: (e: CustomEvent) => add(e, row) }));

    let open = false;
</script>

<WalterDataWrapperQuickAdd
    title={quickAddEntry.vertrag?.text || "Vertrag"}
    addEntry={quickAddEntry}
    addUrl="/api/vertraege/"
    bind:addModalOpen={open}>
    <WalterMiete entry={quickAddEntry}/>
</WalterDataWrapperQuickAdd>

<WalterDataWrapper
    {addUrl}
    addEntry={entry}
    {title}
    {navigate}
    rows={rowsAdd}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterVertrag {fetchImpl} {entry} />
    {/if}
</WalterDataWrapper>
