<script lang="ts">
    import { goto } from '$app/navigation';
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import { WalterDataWrapper, WalterZaehler } from '$WalterComponents';
    import type { WalterSelectionEntry, WalterZaehlerEntry } from '$WalterLib';

    const headers = [
        { key: 'kennnummer', value: 'Kennnummer' },
        { key: 'wohnung.text', value: 'Wohnung' },
        { key: 'typ.text', value: 'Typ' },
        { key: 'lastZaehlerstand.datum', value: 'Letztes Ablesedatum' },
        { key: 'lastZaehlerstand.stand', value: 'Letzter Stand' },
        { key: 'lastZaehlerstand.einheit', value: 'Einheit' }
    ];

    const addUrl = `/api/zaehler/`;

    export let rows: WalterZaehlerEntry[];
    export let search = false;
    export let title: string | undefined = undefined;
    export let wohnungen: WalterSelectionEntry[];
    export let umlagen: WalterSelectionEntry[];
    export let zaehlertypen: WalterSelectionEntry[];
    export let a: Partial<WalterZaehlerEntry> | undefined = undefined;

    const navigate = (e: CustomEvent<DataTableRow>) =>
        goto(`/zaehler/${e.detail.id}`);
</script>

<WalterDataWrapper
    addEntry={a}
    {addUrl}
    {title}
    {search}
    {navigate}
    {rows}
    {headers}
>
    {#if a}
        <WalterZaehler {wohnungen} {umlagen} {zaehlertypen} {a} />
    {/if}
</WalterDataWrapper>
