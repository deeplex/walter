<script lang="ts">
    import { WalterDataWrapper, WalterZaehlerstand } from '$walter/components';

    import { WalterZaehlerstandEntry } from '$walter/lib';
    import { walter_goto } from '$walter/services/utils';
    export let fetchImpl: typeof fetch;

    const headers = [
        { key: 'datum', value: 'Datum' },
        { key: 'stand', value: 'Stand' },
        { key: 'einheit', value: 'Einheit' }
    ];

    export let rows: WalterZaehlerstandEntry[];
    export let fullHeight = false;
    export let title: string | undefined = undefined;
    export let entry: Partial<WalterZaehlerstandEntry> | undefined = undefined;

    const navigate = (e: CustomEvent) =>
        walter_goto(`/zaehlerstaende/${e.detail.id}`);
</script>

<WalterDataWrapper
    addUrl={WalterZaehlerstandEntry.ApiURL}
    {navigate}
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
