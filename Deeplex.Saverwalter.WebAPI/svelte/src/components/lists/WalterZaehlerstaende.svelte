<script lang="ts">
    import { WalterDataWrapper, WalterZaehlerstand } from '$walter/components';

    import type { WalterZaehlerstandEntry } from '$walter/lib';
    import { walter_goto } from '$walter/services/utils';
    export let fetchImpl: typeof fetch;

    const headers = [
        { key: 'datum', value: 'Datum' },
        { key: 'stand', value: 'Stand' },
        { key: 'einheit', value: 'Einheit' }
    ];

    const addUrl = `/api/zaehlerstaende/`;

    export let rows: WalterZaehlerstandEntry[];
    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterZaehlerstandEntry> | undefined = undefined;

    const navigate = (e: CustomEvent) => walter_goto(`/zaehlerstaende/${e.detail.id}`);
</script>

<WalterDataWrapper
    {navigate}
    {addUrl}
    addEntry={entry}
    {title}
    {rows}
    {headers}
    {fullHeight}
>
    {#if entry}
        <WalterZaehlerstand {fetchImpl} {entry} />
    {/if}
</WalterDataWrapper>
