<script lang="ts">
    import { goto } from '$app/navigation';
    import { WalterDataWrapper, WalterZaehlerstand } from '$WalterComponents';

    import type { WalterZaehlerstandEntry } from '$WalterLib';

    const headers = [
        { key: 'datum', value: 'Datum' },
        { key: 'stand', value: 'Stand' },
        { key: 'einheit', value: 'Einheit' }
    ];

    const addUrl = `/api/zaehlerstaende/`;

    export let rows: WalterZaehlerstandEntry[];
    export let search = false;
    export let title: string | undefined = undefined;
    export let a: Partial<WalterZaehlerstandEntry> | undefined = undefined;

    const navigate = (e: CustomEvent) => goto(`/zaehlerstaende/${e.detail.id}`);
</script>

<WalterDataWrapper
    {navigate}
    {addUrl}
    addEntry={a}
    {title}
    {search}
    {rows}
    {headers}
>
    {#if a}
        <WalterZaehlerstand {a} />
    {/if}
</WalterDataWrapper>
