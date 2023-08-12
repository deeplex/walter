<script lang="ts">
    import { goto } from '$app/navigation';
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import { WalterDataWrapper, WalterZaehler } from '$walter/components';
    import WalterDataWrapperQuickAdd from '../elements/WalterDataWrapperQuickAdd.svelte';
    import WalterZaehlerstand from '../details/WalterZaehlerstand.svelte';
    import { convertDateCanadian } from '$walter/services/utils';
    import type { WalterZaehlerEntry, WalterZaehlerstandEntry } from '$walter/lib';

    const headers = [
        { key: 'kennnummer', value: 'Kennnummer' },
        { key: 'wohnung.text', value: 'Wohnung' },
        { key: 'typ.text', value: 'Typ' },
        { key: 'lastZaehlerstand.datum', value: 'Letztes Ablesedatum' },
        { key: 'lastZaehlerstand.stand', value: 'Letzter Stand' },
        { key: 'lastZaehlerstand.einheit', value: 'Einheit' },
        { key: 'button', value: 'Stand hinzufügen' }
    ];

    const addUrl = `/api/zaehler/`;

    export let rows: WalterZaehlerEntry[];
    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterZaehlerEntry> | undefined = undefined;
    export let fetchImpl: typeof fetch;

    export let ablesedatum: Date = new Date();
    let quickAddEntry: Partial<WalterZaehlerstandEntry> = {};

    function add(e: CustomEvent, zaehler: WalterZaehlerEntry) {
        e.stopPropagation();
        quickAddEntry = {
            datum: convertDateCanadian(ablesedatum),
            stand: zaehler.lastZaehlerstand.stand,
            einheit: zaehler.lastZaehlerstand.einheit,
            zaehler: {id: zaehler.id, text: zaehler.kennnummer}
        }

        open = true;
    }
    const rowsAdd = rows.map(row => ({...row, button: (e: CustomEvent) => add(e, row) }));

    let open = false;

    const navigate = (e: CustomEvent<DataTableRow>) =>
        goto(`/zaehler/${e.detail.id}`);
</script>

<WalterDataWrapperQuickAdd
    title={quickAddEntry.zaehler?.text || "Zähler"}
    addEntry={quickAddEntry}
    addUrl="/api/zaehlerstand/"
    bind:addModalOpen={open}>
    <WalterZaehlerstand entry={quickAddEntry}/>
</WalterDataWrapperQuickAdd>

<WalterDataWrapper
    addEntry={entry}
    {addUrl}
    {title}
    {navigate}
    rows={rowsAdd}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterZaehler {fetchImpl} {entry} />
    {/if}
</WalterDataWrapper>
