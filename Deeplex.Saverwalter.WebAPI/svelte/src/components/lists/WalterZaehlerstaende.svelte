<script lang="ts">
    import { goto } from '$app/navigation';
    import { WalterDataWrapper, WalterZaehlerstand } from '$walter/components';

    import type { WalterZaehlerstandEntry } from '$walter/lib';

    const headers = [
        { key: 'datum', value: 'Datum' },
        { key: 'stand', value: 'Stand' },
        { key: 'einheit', value: 'Einheit' }
    ];

    const addUrl = `/api/zaehlerstaende/`;

    export let rows: WalterZaehlerstandEntry[];
    export let search = false;
    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterZaehlerstandEntry> | undefined = undefined;

    const navigate = (e: CustomEvent) => goto(`/zaehlerstaende/${e.detail.id}`);
</script>

<WalterDataWrapper
    {navigate}
    {addUrl}
    addEntry={entry}
    {title}
    {search}
    {rows}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterZaehlerstand {entry} />
    {/if}
</WalterDataWrapper>
