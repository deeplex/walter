<script lang="ts">
    import { WalterDataWrapper, WalterMiete } from '$walter/components';
    import type { WalterMieteEntry } from '$walter/lib';
    import { walter_goto } from '$walter/services/utils';

    const headers = [
        { key: 'betreffenderMonat', value: 'Betreffender Monat' },
        { key: 'zahlungsdatum', value: 'Zahlungsdatum' },
        { key: 'betrag', value: 'Betrag' }
    ];

    const addUrl = `/api/mieten/`;

    export let rows: WalterMieteEntry[];
    export let fullHeight = false;
    export let title: string | undefined = undefined;

    const navigate = (e: CustomEvent) => walter_goto(`/mieten/${e.detail.id}`);

    export let entry: Partial<WalterMieteEntry> | undefined = undefined;
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
        <WalterMiete {entry} />
    {/if}
</WalterDataWrapper>
