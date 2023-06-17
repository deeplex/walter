<script lang="ts">
    import { goto } from '$app/navigation';
    import { WalterDataWrapper, WalterMiete } from '$walter/components';
    import type { WalterMieteEntry } from '$walter/lib';

    const headers = [
        { key: 'betreffenderMonat', value: 'Betreffender Monat' },
        { key: 'zahlungsdatum', value: 'Zahlungsdatum' },
        { key: 'betrag', value: 'Betrag' }
    ];

    const addUrl = `/api/mieten/`;

    export let rows: WalterMieteEntry[];
    export let fullHeight = false;
    export let search = false;
    export let title: string | undefined = undefined;

    const navigate = (e: CustomEvent) => goto(`/mieten/${e.detail.id}`);

    export let entry: Partial<WalterMieteEntry> | undefined = undefined;
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
        <WalterMiete {entry} />
    {/if}
</WalterDataWrapper>
