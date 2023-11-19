<script lang="ts">
    import { WalterDataWrapper, WalterZaehlerstand } from '$walter/components';

    import { WalterZaehlerstandEntry } from '$walter/lib';
    import { navigation } from '$walter/services/navigation';
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

    const on_click_row = (e: CustomEvent) =>
        navigation.zaehlerstand(e.detail.id);
</script>

<WalterDataWrapper
    addUrl={WalterZaehlerstandEntry.ApiURL}
    {on_click_row}
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
